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
            profile.AppearancePrompt ??= string.Empty;
            profile.Assets ??= new ObservableCollection<HeroineAsset>();
            profile.StillWorkItems ??= new ObservableCollection<StillWorkItem>();
            EnsureCharacterDirectories(profile.HeroineId);

            string json = JsonSerializer.Serialize(profile, jsonOptions);
            File.WriteAllText(GetProfilePath(profile.HeroineId), json);
        }

        public HeroineAsset AddImageAsset(
            HeroineProfile profile,
            string sourceImagePath,
            AssetUsage usage,
            string assetId,
            AssetStatus status,
            bool overwriteExisting = false)
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

            string normalizedAssetId = assetId.Trim();
            HeroineAsset existingAsset = profile.Assets.FirstOrDefault(asset => asset.AssetId == normalizedAssetId);
            if (existingAsset != null && !overwriteExisting)
            {
                throw new InvalidOperationException("AssetId already exists in this heroine profile.");
            }

            EnsureCharacterDirectories(profile.HeroineId);

            string extension = Path.GetExtension(sourceImagePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = normalizedAssetId + extension;
            string imageDirectory = GetImageUsageDirectory(profile.HeroineId, usage);
            string storedPath = Path.Combine(imageDirectory, fileName);
            if (File.Exists(storedPath) && !overwriteExisting)
            {
                throw new IOException("Destination image file already exists.");
            }

            string relativeStoredPath = Path.Combine("Images", usage.ToString(), fileName);
            string oldStoredPath = existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.StoredPath)
                ? Path.Combine(GetCharacterDirectory(profile.HeroineId), existingAsset.StoredPath)
                : string.Empty;

            if (!IsSamePath(sourceImagePath, storedPath))
            {
                File.Copy(sourceImagePath, storedPath, overwriteExisting);
            }

            if (!string.IsNullOrWhiteSpace(oldStoredPath) && oldStoredPath != storedPath && File.Exists(oldStoredPath))
            {
                File.Delete(oldStoredPath);
            }

            string relativePromptPath = Path.Combine("Prompts", normalizedAssetId + ".prompt.json");
            HeroineAsset assetRecord = existingAsset ?? new HeroineAsset();
            assetRecord.AssetId = normalizedAssetId;
            assetRecord.Usage = usage;
            assetRecord.Status = status;
            assetRecord.FileName = fileName;
            assetRecord.SourcePath = sourceImagePath;
            assetRecord.StoredPath = relativeStoredPath;
            if (string.IsNullOrWhiteSpace(assetRecord.PromptRecordPath))
            {
                assetRecord.PromptRecordPath = relativePromptPath;
            }

            if (existingAsset == null)
            {
                profile.Assets.Add(assetRecord);
            }
            SaveProfile(profile);

            return assetRecord;
        }

        public bool UnregisterImageAsset(HeroineProfile profile, HeroineAsset asset)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            ValidateHeroineId(profile.HeroineId);
            profile.Assets ??= new ObservableCollection<HeroineAsset>();

            HeroineAsset existingAsset = profile.Assets.FirstOrDefault(item => item.AssetId == asset.AssetId);
            if (existingAsset == null)
            {
                return false;
            }

            profile.Assets.Remove(existingAsset);
            SaveProfile(profile);
            return true;
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
            profile.StillWorkItems ??= new ObservableCollection<StillWorkItem>();
            profile.AppearancePrompt ??= string.Empty;

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

        private static bool IsSamePath(string left, string right)
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
