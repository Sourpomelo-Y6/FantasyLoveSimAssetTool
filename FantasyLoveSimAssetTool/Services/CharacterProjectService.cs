using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class CharacterProjectService
    {
        private const string ProfileFileName = "profile.json";
        private readonly JsonSerializerOptions jsonOptions;

        public string WorkspaceRoot { get; }

        public string CharactersDirectory
        {
            get { return Path.Combine(WorkspaceRoot, "Characters"); }
        }

        public CharacterProjectService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public CharacterProjectService(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
            {
                throw new ArgumentException("Workspace root is required.", nameof(workspaceRoot));
            }

            WorkspaceRoot = workspaceRoot;
            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public HeroineProfile CreateCharacter(string heroineId, string displayName)
        {
            ValidateHeroineId(heroineId);

            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = heroineId.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? heroineId.Trim() : displayName.Trim()
            };

            EnsureCharacterDirectories(profile.HeroineId);
            SaveProfile(profile);
            return profile;
        }

        public void SaveProfile(HeroineProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            ValidateHeroineId(profile.HeroineId);
            profile.Assets ??= new ObservableCollection<HeroineAsset>();
            EnsureCharacterDirectories(profile.HeroineId);

            string json = JsonSerializer.Serialize(profile, jsonOptions);
            File.WriteAllText(GetProfilePath(profile.HeroineId), json);
        }

        public HeroineAsset AddImageAsset(
            HeroineProfile profile,
            string sourceImagePath,
            AssetUsage usage,
            string assetId,
            AssetStatus status)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            ValidateHeroineId(profile.HeroineId);
            ValidateAssetId(assetId);
            profile.Assets ??= new ObservableCollection<HeroineAsset>();

            if (string.IsNullOrWhiteSpace(sourceImagePath))
            {
                throw new ArgumentException("Source image path is required.", nameof(sourceImagePath));
            }

            if (!File.Exists(sourceImagePath))
            {
                throw new FileNotFoundException("Source image file was not found.", sourceImagePath);
            }

            if (profile.Assets.Any(asset => asset.AssetId == assetId.Trim()))
            {
                throw new InvalidOperationException("AssetId already exists in this heroine profile.");
            }

            EnsureCharacterDirectories(profile.HeroineId);

            string extension = Path.GetExtension(sourceImagePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = assetId.Trim() + extension;
            string imageDirectory = GetImageUsageDirectory(profile.HeroineId, usage);
            string storedPath = Path.Combine(imageDirectory, fileName);
            if (File.Exists(storedPath))
            {
                throw new IOException("Destination image file already exists.");
            }

            File.Copy(sourceImagePath, storedPath);

            string relativeStoredPath = Path.Combine("Images", usage.ToString(), fileName);
            string relativePromptPath = Path.Combine("Prompts", assetId.Trim() + ".prompt.json");
            HeroineAsset assetRecord = new HeroineAsset
            {
                AssetId = assetId.Trim(),
                Usage = usage,
                Status = status,
                FileName = fileName,
                SourcePath = sourceImagePath,
                StoredPath = relativeStoredPath,
                PromptRecordPath = relativePromptPath
            };

            profile.Assets.Add(assetRecord);
            SaveProfile(profile);

            return assetRecord;
        }

        public HeroineProfile LoadProfile(string heroineId)
        {
            ValidateHeroineId(heroineId);

            string path = GetProfilePath(heroineId);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Profile file was not found.", path);
            }

            string json = File.ReadAllText(path);
            HeroineProfile profile = JsonSerializer.Deserialize<HeroineProfile>(json, jsonOptions);
            if (profile == null)
            {
                throw new InvalidOperationException("Profile file could not be deserialized.");
            }

            profile.Assets ??= new ObservableCollection<HeroineAsset>();

            return profile;
        }

        public IReadOnlyList<HeroineProfile> LoadProfiles()
        {
            if (!Directory.Exists(CharactersDirectory))
            {
                return new List<HeroineProfile>();
            }

            return Directory.GetDirectories(CharactersDirectory)
                .Select(Path.GetFileName)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => LoadProfile(id))
                .OrderBy(profile => profile.HeroineId)
                .ToList();
        }

        public void EnsureCharacterDirectories(string heroineId)
        {
            ValidateHeroineId(heroineId);

            Directory.CreateDirectory(GetCharacterDirectory(heroineId));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Sprites"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Event"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Actions"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Ending"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Prompts"));
        }

        public string GetCharacterDirectory(string heroineId)
        {
            ValidateHeroineId(heroineId);
            return Path.Combine(CharactersDirectory, heroineId.Trim());
        }

        public string GetProfilePath(string heroineId)
        {
            return Path.Combine(GetCharacterDirectory(heroineId), ProfileFileName);
        }

        public string GetImageUsageDirectory(string heroineId, AssetUsage usage)
        {
            return Path.Combine(GetCharacterDirectory(heroineId), "Images", usage.ToString());
        }

        private static void ValidateHeroineId(string heroineId)
        {
            if (string.IsNullOrWhiteSpace(heroineId))
            {
                throw new ArgumentException("HeroineId is required.", nameof(heroineId));
            }

            if (heroineId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("HeroineId contains invalid file name characters.", nameof(heroineId));
            }
        }

        private static void ValidateAssetId(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
            {
                throw new ArgumentException("AssetId is required.", nameof(assetId));
            }

            if (assetId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("AssetId contains invalid file name characters.", nameof(assetId));
            }
        }
    }
}
