using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FantasyLoveSimAssetTool.Services
{
    public class ComfyWorkflowService
    {
        private static readonly Regex DateTokenPattern = new Regex(
            "%date:([^%]+)%",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly string workspaceRoot;

        public ComfyWorkflowService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public ComfyWorkflowService(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
            {
                throw new ArgumentException("Workspace root is required.", nameof(workspaceRoot));
            }

            this.workspaceRoot = workspaceRoot;
        }

        public string BuildWorkflowPreview(ComfySettings settings, PromptRecord promptRecord)
        {
            string workflowJson = BuildWorkflowJson(settings, promptRecord);
            using JsonDocument document = JsonDocument.Parse(workflowJson);
            return JsonSerializer.Serialize(
                document.RootElement,
                new JsonSerializerOptions { WriteIndented = true });
        }

        public string BuildWorkflowJson(ComfySettings settings, PromptRecord promptRecord)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (promptRecord == null)
            {
                throw new ArgumentNullException(nameof(promptRecord));
            }

            string templatePath = ResolveWorkflowTemplatePath(settings.WorkflowTemplatePath);
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("ComfyUI workflow template was not found.", templatePath);
            }

            string workflowJson = File.ReadAllText(templatePath);
            workflowJson = workflowJson.Replace(
                settings.PositivePromptPlaceholder,
                EscapeJsonStringValue(promptRecord.PositivePrompt ?? string.Empty));
            workflowJson = workflowJson.Replace(
                settings.NegativePromptPlaceholder,
                EscapeJsonStringValue(promptRecord.NegativePrompt ?? string.Empty));

            using JsonDocument document = JsonDocument.Parse(workflowJson);
            return ConvertUiWorkflowToApiPrompt(document.RootElement);
        }

        private static string ConvertUiWorkflowToApiPrompt(JsonElement rootElement)
        {
            if (!rootElement.TryGetProperty("nodes", out JsonElement nodesElement) ||
                !rootElement.TryGetProperty("links", out JsonElement linksElement) ||
                nodesElement.ValueKind != JsonValueKind.Array ||
                linksElement.ValueKind != JsonValueKind.Array)
            {
                return rootElement.GetRawText();
            }

            Dictionary<int, JsonElement> nodesById = new Dictionary<int, JsonElement>();
            foreach (JsonElement nodeElement in nodesElement.EnumerateArray())
            {
                if (nodeElement.TryGetProperty("id", out JsonElement idElement) && idElement.TryGetInt32(out int nodeId))
                {
                    nodesById[nodeId] = nodeElement;
                }
            }

            Dictionary<int, LinkReference> linksById = new Dictionary<int, LinkReference>();
            foreach (JsonElement linkElement in linksElement.EnumerateArray())
            {
                if (linkElement.ValueKind != JsonValueKind.Array || linkElement.GetArrayLength() < 5)
                {
                    continue;
                }

                int linkId = linkElement[0].GetInt32();
                linksById[linkId] = new LinkReference
                {
                    OriginNodeId = linkElement[1].GetInt32(),
                    OriginSlotIndex = linkElement[2].GetInt32()
                };
            }

            Dictionary<string, ApiPromptNode> apiPrompt = new Dictionary<string, ApiPromptNode>();
            foreach (JsonElement nodeElement in nodesElement.EnumerateArray())
            {
                int nodeId = nodeElement.GetProperty("id").GetInt32();
                string nodeType = nodeElement.GetProperty("type").GetString();
                if (IsFrontendPrimitiveNode(nodeType))
                {
                    continue;
                }

                Dictionary<string, object> inputs = new Dictionary<string, object>();
                AddLinkedInputs(nodeElement, inputs, nodesById, linksById);
                AddWidgetInputs(nodeElement, inputs);

                apiPrompt[nodeId.ToString()] = new ApiPromptNode
                {
                    ClassType = nodeType,
                    Inputs = inputs
                };
            }

            return JsonSerializer.Serialize(apiPrompt);
        }

        private static void AddLinkedInputs(
            JsonElement nodeElement,
            Dictionary<string, object> inputs,
            Dictionary<int, JsonElement> nodesById,
            Dictionary<int, LinkReference> linksById)
        {
            if (!nodeElement.TryGetProperty("inputs", out JsonElement inputElements) ||
                inputElements.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (JsonElement inputElement in inputElements.EnumerateArray())
            {
                if (!inputElement.TryGetProperty("name", out JsonElement nameElement) ||
                    !inputElement.TryGetProperty("link", out JsonElement linkElement) ||
                    linkElement.ValueKind != JsonValueKind.Number)
                {
                    continue;
                }

                string inputName = nameElement.GetString();
                int linkId = linkElement.GetInt32();
                if (string.IsNullOrWhiteSpace(inputName) || !linksById.TryGetValue(linkId, out LinkReference linkReference))
                {
                    continue;
                }

                inputs[inputName] = ResolveLinkedInputValue(linkReference, nodesById, inputName);
            }
        }

        private static object ResolveLinkedInputValue(
            LinkReference linkReference,
            Dictionary<int, JsonElement> nodesById,
            string inputName)
        {
            if (nodesById.TryGetValue(linkReference.OriginNodeId, out JsonElement originNode) &&
                originNode.TryGetProperty("type", out JsonElement typeElement) &&
                IsFrontendPrimitiveNode(typeElement.GetString()))
            {
                return NormalizePrimitiveValue(inputName, GetWidgetValue(originNode, 0));
            }

            return new object[] { linkReference.OriginNodeId.ToString(), linkReference.OriginSlotIndex };
        }

        private static object NormalizePrimitiveValue(string inputName, object value)
        {
            if (!IsSeedInput(inputName))
            {
                return value;
            }

            if (value is long longValue && longValue < 0)
            {
                return CreateRandomSeed();
            }

            if (value is int intValue && intValue < 0)
            {
                return CreateRandomSeed();
            }

            if (value is double doubleValue && doubleValue < 0)
            {
                return CreateRandomSeed();
            }

            return value;
        }

        private static bool IsSeedInput(string inputName)
        {
            return !string.IsNullOrWhiteSpace(inputName) &&
                inputName.IndexOf("seed", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static long CreateRandomSeed()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(bytes, 0) & long.MaxValue;
        }

        private static void AddWidgetInputs(JsonElement nodeElement, Dictionary<string, object> inputs)
        {
            string nodeType = nodeElement.GetProperty("type").GetString();
            switch (nodeType)
            {
                case "CheckpointLoaderSimple":
                    AddWidgetInput(nodeElement, inputs, "ckpt_name", 0);
                    break;
                case "CLIPTextEncode":
                    AddWidgetInput(nodeElement, inputs, "text", 0);
                    break;
                case "EmptyLatentImage":
                    AddWidgetInput(nodeElement, inputs, "width", 0);
                    AddWidgetInput(nodeElement, inputs, "height", 1);
                    AddWidgetInput(nodeElement, inputs, "batch_size", 2);
                    break;
                case "KSamplerAdvanced":
                    AddWidgetInput(nodeElement, inputs, "add_noise", 0);
                    AddWidgetInput(nodeElement, inputs, "noise_seed", 1);
                    AddWidgetInput(nodeElement, inputs, "steps", 3);
                    AddWidgetInput(nodeElement, inputs, "cfg", 4);
                    AddWidgetInput(nodeElement, inputs, "sampler_name", 5);
                    AddWidgetInput(nodeElement, inputs, "scheduler", 6);
                    AddWidgetInput(nodeElement, inputs, "start_at_step", 7);
                    AddWidgetInput(nodeElement, inputs, "end_at_step", 8);
                    AddWidgetInput(nodeElement, inputs, "return_with_leftover_noise", 9);
                    break;
                case "SaveImage":
                    AddWidgetInput(nodeElement, inputs, "filename_prefix", 0);
                    break;
            }
        }

        private static void AddWidgetInput(JsonElement nodeElement, Dictionary<string, object> inputs, string inputName, int widgetIndex)
        {
            if (inputs.ContainsKey(inputName))
            {
                return;
            }

            object widgetValue = GetWidgetValue(nodeElement, widgetIndex);
            if (inputName == "filename_prefix" && widgetValue is string filenamePrefix)
            {
                widgetValue = ExpandDateTokens(filenamePrefix);
            }

            inputs[inputName] = widgetValue;
        }

        private static string ExpandDateTokens(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            DateTime now = DateTime.Now;
            return DateTokenPattern.Replace(
                value,
                match => now.ToString(match.Groups[1].Value, CultureInfo.InvariantCulture));
        }

        private static object GetWidgetValue(JsonElement nodeElement, int widgetIndex)
        {
            if (!nodeElement.TryGetProperty("widgets_values", out JsonElement widgetsElement) ||
                widgetsElement.ValueKind != JsonValueKind.Array ||
                widgetsElement.GetArrayLength() <= widgetIndex)
            {
                return string.Empty;
            }

            return ConvertJsonElementToObject(widgetsElement[widgetIndex]);
        }

        private static object ConvertJsonElementToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }

                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    return element.Clone();
            }
        }

        private static bool IsFrontendPrimitiveNode(string nodeType)
        {
            return nodeType == "PrimitiveInt" ||
                nodeType == "PrimitiveFloat" ||
                nodeType == "PrimitiveString" ||
                nodeType == "PrimitiveBoolean";
        }

        private string ResolveWorkflowTemplatePath(string workflowTemplatePath)
        {
            if (string.IsNullOrWhiteSpace(workflowTemplatePath))
            {
                return string.Empty;
            }

            return Path.IsPathRooted(workflowTemplatePath)
                ? workflowTemplatePath
                : Path.Combine(workspaceRoot, workflowTemplatePath);
        }

        private static string EscapeJsonStringValue(string value)
        {
            string jsonString = JsonSerializer.Serialize(value ?? string.Empty);
            return jsonString.Substring(1, jsonString.Length - 2);
        }

        private class ApiPromptNode
        {
            [JsonPropertyName("class_type")]
            public string ClassType { get; set; }

            [JsonPropertyName("inputs")]
            public Dictionary<string, object> Inputs { get; set; }
        }

        private struct LinkReference
        {
            public int OriginNodeId { get; set; }

            public int OriginSlotIndex { get; set; }
        }
    }
}
