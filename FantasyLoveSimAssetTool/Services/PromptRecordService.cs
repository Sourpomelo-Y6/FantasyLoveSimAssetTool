using FantasyLoveSimAssetTool.Models;
using System;
using System.IO;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class PromptRecordService
    {
        private readonly CharacterProjectService characterProjectService;
        private readonly JsonSerializerOptions jsonOptions;

        public PromptRecordService(CharacterProjectService characterProjectService)
        {
            this.characterProjectService = characterProjectService ?? throw new ArgumentNullException(nameof(characterProjectService));
            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public PromptRecord LoadOrCreatePromptRecord(HeroineProfile profile, HeroineAsset asset)
        {
            Validate(profile, asset);

            string path = GetPromptRecordPath(profile, asset);
            if (!File.Exists(path))
            {
                return new PromptRecord();
            }

            string json = File.ReadAllText(path);
            PromptRecord record = JsonSerializer.Deserialize<PromptRecord>(json, jsonOptions);
            return record ?? new PromptRecord();
        }

        public void SavePromptRecord(HeroineProfile profile, HeroineAsset asset, PromptRecord record)
        {
            Validate(profile, asset);

            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            string path = GetPromptRecordPath(profile, asset);
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(record, jsonOptions);
            File.WriteAllText(path, json);
        }

        public string GetPromptRecordPath(HeroineProfile profile, HeroineAsset asset)
        {
            Validate(profile, asset);

            string relativePath = asset.PromptRecordPath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                relativePath = Path.Combine("Prompts", asset.AssetId + ".prompt.json");
                asset.PromptRecordPath = relativePath;
            }

            return Path.Combine(characterProjectService.GetCharacterDirectory(profile.HeroineId), relativePath);
        }

        private static void Validate(HeroineProfile profile, HeroineAsset asset)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (string.IsNullOrWhiteSpace(profile.HeroineId))
            {
                throw new ArgumentException("HeroineId is required.", nameof(profile));
            }

            if (string.IsNullOrWhiteSpace(asset.AssetId))
            {
                throw new ArgumentException("AssetId is required.", nameof(asset));
            }
        }
    }
}
