using FantasyLoveSimAssetTool.Models;
using System;
using System.IO;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class ComfySettingsService
    {
        private const string SettingsDirectoryName = "ComfySettings";
        private const string SettingsFileName = "comfyui.json";

        private readonly string workspaceRoot;

        public string SettingsPath
        {
            get { return Path.Combine(workspaceRoot, SettingsDirectoryName, SettingsFileName); }
        }

        public ComfySettingsService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public ComfySettingsService(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
            {
                throw new ArgumentException("Workspace root is required.", nameof(workspaceRoot));
            }

            this.workspaceRoot = workspaceRoot;
        }

        public ComfySettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return new ComfySettings();
            }

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                ComfySettings settings = JsonSerializer.Deserialize<ComfySettings>(
                    File.ReadAllText(SettingsPath),
                    options);

                return Normalize(settings);
            }
            catch
            {
                return new ComfySettings();
            }
        }

        private static ComfySettings Normalize(ComfySettings settings)
        {
            ComfySettings normalized = settings ?? new ComfySettings();
            ComfySettings defaults = new ComfySettings();

            if (string.IsNullOrWhiteSpace(normalized.EndpointUrl))
            {
                normalized.EndpointUrl = defaults.EndpointUrl;
            }

            if (string.IsNullOrWhiteSpace(normalized.WorkflowTemplatePath))
            {
                normalized.WorkflowTemplatePath = defaults.WorkflowTemplatePath;
            }

            if (string.IsNullOrWhiteSpace(normalized.PositivePromptPlaceholder))
            {
                normalized.PositivePromptPlaceholder = defaults.PositivePromptPlaceholder;
            }

            if (string.IsNullOrWhiteSpace(normalized.NegativePromptPlaceholder))
            {
                normalized.NegativePromptPlaceholder = defaults.NegativePromptPlaceholder;
            }

            normalized.OutputNodeId ??= string.Empty;
            return normalized;
        }
    }
}
