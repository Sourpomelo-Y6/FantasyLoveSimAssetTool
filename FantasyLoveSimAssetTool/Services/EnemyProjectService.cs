using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class EnemyProjectService
    {
        private const string EnemyProfileFileName = "enemy.json";
        private readonly JsonSerializerOptions jsonOptions;

        public string WorkspaceRoot { get; }

        public string EnemiesDirectory
        {
            get { return Path.Combine(WorkspaceRoot, "Enemies"); }
        }

        public EnemyProjectService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public EnemyProjectService(string workspaceRoot)
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

        public EnemyProfile CreateEnemy(string enemyId, string displayName, string enemyType)
        {
            ValidateEnemyId(enemyId);

            EnemyProfile profile = new EnemyProfile
            {
                EnemyId = enemyId.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? enemyId.Trim() : displayName.Trim(),
                EnemyType = enemyType?.Trim() ?? string.Empty
            };

            EnsureEnemyDirectories(profile.EnemyId);
            SaveProfile(profile);
            return profile;
        }

        public void SaveProfile(EnemyProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            ValidateEnemyId(profile.EnemyId);
            profile.SchemaVersion = profile.SchemaVersion <= 0 ? 1 : profile.SchemaVersion;
            profile.DisplayName ??= string.Empty;
            profile.EnemyType ??= string.Empty;
            profile.AppearancePrompt ??= string.Empty;
            profile.BattleCommonPositivePrompt ??= string.Empty;
            profile.NegativePrompt ??= string.Empty;
            profile.Memo ??= string.Empty;
            profile.Assets ??= new ObservableCollection<EnemyAsset>();

            foreach (EnemyAsset asset in profile.Assets)
            {
                NormalizeAsset(asset);
            }

            EnsureEnemyDirectories(profile.EnemyId);
            string json = JsonSerializer.Serialize(profile, jsonOptions);
            File.WriteAllText(GetProfilePath(profile.EnemyId), json);
        }

        public EnemyAsset AddImageAsset(
            EnemyProfile profile,
            string sourceImagePath,
            string assetId,
            AssetStatus status,
            bool overwriteExisting = false)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            ValidateEnemyId(profile.EnemyId);
            ValidateAssetId(assetId);
            profile.Assets ??= new ObservableCollection<EnemyAsset>();

            if (string.IsNullOrWhiteSpace(sourceImagePath))
            {
                throw new ArgumentException("Source image path is required.", nameof(sourceImagePath));
            }

            if (!File.Exists(sourceImagePath))
            {
                throw new FileNotFoundException("Source image file was not found.", sourceImagePath);
            }

            string normalizedAssetId = assetId.Trim();
            EnemyAsset existingAsset = profile.Assets.FirstOrDefault(asset => asset.AssetId == normalizedAssetId);
            if (existingAsset != null && !overwriteExisting)
            {
                throw new InvalidOperationException("AssetId already exists in this enemy profile.");
            }

            EnsureEnemyDirectories(profile.EnemyId);

            string extension = Path.GetExtension(sourceImagePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = normalizedAssetId + extension;
            string imageDirectory = GetImageUsageDirectory(profile.EnemyId, AssetUsage.Battle);
            string storedPath = Path.Combine(imageDirectory, fileName);
            if (File.Exists(storedPath) && !overwriteExisting)
            {
                throw new IOException("Destination image file already exists.");
            }

            string relativeStoredPath = Path.Combine("Images", AssetUsage.Battle.ToString(), fileName);
            string oldStoredPath = existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.StoredPath)
                ? Path.Combine(GetEnemyDirectory(profile.EnemyId), existingAsset.StoredPath)
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
            EnemyAsset assetRecord = existingAsset ?? new EnemyAsset();
            assetRecord.AssetId = normalizedAssetId;
            assetRecord.Usage = AssetUsage.Battle;
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

        public bool UnregisterImageAsset(EnemyProfile profile, EnemyAsset asset)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            ValidateEnemyId(profile.EnemyId);
            profile.Assets ??= new ObservableCollection<EnemyAsset>();

            EnemyAsset existingAsset = profile.Assets.FirstOrDefault(item => item.AssetId == asset.AssetId);
            if (existingAsset == null)
            {
                return false;
            }

            profile.Assets.Remove(existingAsset);
            SaveProfile(profile);
            return true;
        }

        public EnemyProfile LoadProfile(string enemyId)
        {
            ValidateEnemyId(enemyId);

            string path = GetProfilePath(enemyId);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Enemy profile file was not found.", path);
            }

            string json = File.ReadAllText(path);
            EnemyProfile profile = JsonSerializer.Deserialize<EnemyProfile>(json, jsonOptions);
            if (profile == null)
            {
                throw new InvalidOperationException("Enemy profile file could not be deserialized.");
            }

            NormalizeProfile(profile);
            return profile;
        }

        public IReadOnlyList<EnemyProfile> LoadProfiles()
        {
            if (!Directory.Exists(EnemiesDirectory))
            {
                return new List<EnemyProfile>();
            }

            return Directory.GetDirectories(EnemiesDirectory)
                .Select(Path.GetFileName)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => LoadProfile(id))
                .OrderBy(profile => profile.EnemyId)
                .ToList();
        }

        public void EnsureEnemyDirectories(string enemyId)
        {
            ValidateEnemyId(enemyId);

            Directory.CreateDirectory(GetEnemyDirectory(enemyId));
            Directory.CreateDirectory(Path.Combine(GetEnemyDirectory(enemyId), "Images"));
            Directory.CreateDirectory(Path.Combine(GetEnemyDirectory(enemyId), "Images", AssetUsage.Battle.ToString()));
            Directory.CreateDirectory(Path.Combine(GetEnemyDirectory(enemyId), "Prompts"));
        }

        public string GetEnemyDirectory(string enemyId)
        {
            ValidateEnemyId(enemyId);
            return Path.Combine(EnemiesDirectory, enemyId.Trim());
        }

        public string GetProfilePath(string enemyId)
        {
            return Path.Combine(GetEnemyDirectory(enemyId), EnemyProfileFileName);
        }

        public string GetImageUsageDirectory(string enemyId, AssetUsage usage)
        {
            if (usage != AssetUsage.Battle)
            {
                throw new ArgumentException("Enemy assets currently support only Battle usage.", nameof(usage));
            }

            return Path.Combine(GetEnemyDirectory(enemyId), "Images", usage.ToString());
        }

        private static void NormalizeProfile(EnemyProfile profile)
        {
            profile.SchemaVersion = profile.SchemaVersion <= 0 ? 1 : profile.SchemaVersion;
            profile.EnemyId ??= string.Empty;
            profile.DisplayName ??= string.Empty;
            profile.EnemyType ??= string.Empty;
            profile.AppearancePrompt ??= string.Empty;
            profile.BattleCommonPositivePrompt ??= string.Empty;
            profile.NegativePrompt ??= string.Empty;
            profile.Memo ??= string.Empty;
            profile.Assets ??= new ObservableCollection<EnemyAsset>();

            foreach (EnemyAsset asset in profile.Assets)
            {
                NormalizeAsset(asset);
            }
        }

        private static void NormalizeAsset(EnemyAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            asset.AssetId ??= string.Empty;
            asset.Usage = AssetUsage.Battle;
            asset.FileName ??= string.Empty;
            asset.SourcePath ??= string.Empty;
            asset.StoredPath ??= string.Empty;
            asset.PromptRecordPath ??= string.Empty;
            asset.Memo ??= string.Empty;
        }

        private static void ValidateEnemyId(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                throw new ArgumentException("EnemyId is required.", nameof(enemyId));
            }

            if (enemyId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("EnemyId contains invalid file name characters.", nameof(enemyId));
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
