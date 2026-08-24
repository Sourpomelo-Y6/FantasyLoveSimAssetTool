using FantasyLoveSimAssetTool.Models;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class LocalAiSettingsService
    {
        private readonly string workspaceRoot;

        public LocalAiSettingsService(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
                throw new ArgumentException("Workspace root is required.", nameof(workspaceRoot));

            this.workspaceRoot = workspaceRoot;
        }

        public string SettingsPath => Path.Combine(workspaceRoot, "LocalAISettings", "connection.json");

        public LocalAiSettings Load()
        {
            if (!File.Exists(SettingsPath)) return new LocalAiSettings();

            try
            {
                LocalAiSettings settings = JsonSerializer.Deserialize<LocalAiSettings>(
                    File.ReadAllText(SettingsPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Normalize(settings);
            }
            catch
            {
                return new LocalAiSettings();
            }
        }

        public void Save(LocalAiSettings settings)
        {
            LocalAiSettings normalized = Normalize(settings);
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(normalized,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));
        }

        private static LocalAiSettings Normalize(LocalAiSettings settings)
        {
            LocalAiSettings value = settings ?? new LocalAiSettings();
            if (string.IsNullOrWhiteSpace(value.ServerUrl)) value.ServerUrl = "http://127.0.0.1:8080";
            value.ServerUrl = value.ServerUrl.Trim();
            value.ModelId = (value.ModelId ?? string.Empty).Trim();
            if (value.TimeoutSeconds < 1) value.TimeoutSeconds = 120;
            if (value.Temperature < 0 || value.Temperature > 2) value.Temperature = 0.7;
            if (value.MaxTokens < 1) value.MaxTokens = 1024;
            return value;
        }
    }
}
