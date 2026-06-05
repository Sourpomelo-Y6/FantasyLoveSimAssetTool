using FantasyLoveSimAssetTool.Models;
using System;
using System.IO;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class ComfyWorkflowService
    {
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
            return JsonSerializer.Serialize(
                document.RootElement,
                new JsonSerializerOptions { WriteIndented = true });
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
    }
}
