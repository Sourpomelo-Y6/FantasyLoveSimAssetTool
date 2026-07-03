using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class PlayerProjectService
    {
        private const string PlayerProfileFileName = "player.json";
        private readonly JsonSerializerOptions jsonOptions;

        public string WorkspaceRoot { get; }

        public string PlayerDirectory
        {
            get { return Path.Combine(WorkspaceRoot, "Player"); }
        }

        public PlayerProjectService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public PlayerProjectService(string workspaceRoot)
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

        public PlayerProfile LoadOrCreateProfile()
        {
            EnsurePlayerDirectories();
            string path = GetProfilePath();
            if (!File.Exists(path))
            {
                PlayerProfile playerProfile = new PlayerProfile();
                SaveProfile(playerProfile);
                return playerProfile;
            }

            string json = File.ReadAllText(path);
            PlayerProfile loadedProfile = JsonSerializer.Deserialize<PlayerProfile>(json, jsonOptions);
            PlayerProfile profile = loadedProfile ?? new PlayerProfile();
            NormalizeProfile(profile);
            return profile;
        }

        public void SaveProfile(PlayerProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            NormalizeProfile(profile);
            EnsurePlayerDirectories();
            string json = JsonSerializer.Serialize(profile, jsonOptions);
            File.WriteAllText(GetProfilePath(), json);
        }

        public PlayerAsset AddImageAsset(
            PlayerProfile profile,
            string sourceImagePath,
            string assetId,
            AssetStatus status,
            bool overwriteExisting = false)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            ValidateAssetId(assetId);
            profile.Assets ??= new ObservableCollection<PlayerAsset>();

            if (string.IsNullOrWhiteSpace(sourceImagePath))
            {
                throw new ArgumentException("Source image path is required.", nameof(sourceImagePath));
            }

            if (!File.Exists(sourceImagePath))
            {
                throw new FileNotFoundException("Source image file was not found.", sourceImagePath);
            }

            string normalizedAssetId = assetId.Trim();
            PlayerAsset existingAsset = profile.Assets.FirstOrDefault(asset => asset.AssetId == normalizedAssetId);
            if (existingAsset != null && !overwriteExisting)
            {
                throw new InvalidOperationException("AssetId already exists in player profile.");
            }

            EnsurePlayerDirectories();

            string extension = Path.GetExtension(sourceImagePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = normalizedAssetId + extension;
            string imageDirectory = GetImageUsageDirectory(AssetUsage.Battle);
            string storedPath = Path.Combine(imageDirectory, fileName);
            if (File.Exists(storedPath) && !overwriteExisting)
            {
                throw new IOException("Destination image file already exists.");
            }

            string relativeStoredPath = Path.Combine("Images", AssetUsage.Battle.ToString(), fileName);
            string oldStoredPath = existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.StoredPath)
                ? Path.Combine(PlayerDirectory, existingAsset.StoredPath)
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
            PlayerAsset assetRecord = existingAsset ?? new PlayerAsset();
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

        public bool UnregisterImageAsset(PlayerProfile profile, PlayerAsset asset)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            profile.Assets ??= new ObservableCollection<PlayerAsset>();
            PlayerAsset existingAsset = profile.Assets.FirstOrDefault(item => item.AssetId == asset.AssetId);
            if (existingAsset == null)
            {
                return false;
            }

            profile.Assets.Remove(existingAsset);
            SaveProfile(profile);
            return true;
        }

        public void EnsurePlayerDirectories()
        {
            Directory.CreateDirectory(PlayerDirectory);
            Directory.CreateDirectory(Path.Combine(PlayerDirectory, "Images"));
            Directory.CreateDirectory(Path.Combine(PlayerDirectory, "Images", AssetUsage.Battle.ToString()));
            Directory.CreateDirectory(Path.Combine(PlayerDirectory, "Prompts"));
        }

        public string GetProfilePath()
        {
            return Path.Combine(PlayerDirectory, PlayerProfileFileName);
        }

        public string GetImageUsageDirectory(AssetUsage usage)
        {
            if (usage != AssetUsage.Battle)
            {
                throw new ArgumentException("Player assets currently support only Battle usage.", nameof(usage));
            }

            return Path.Combine(PlayerDirectory, "Images", usage.ToString());
        }

        private static void NormalizeProfile(PlayerProfile profile)
        {
            profile.SchemaVersion = profile.SchemaVersion <= 0 ? 1 : profile.SchemaVersion;
            profile.PlayerId = string.IsNullOrWhiteSpace(profile.PlayerId) ? "Player" : profile.PlayerId;
            profile.DisplayName ??= string.Empty;
            profile.AppearancePrompt ??= string.Empty;
            profile.BattleCommonPositivePrompt ??= string.Empty;
            profile.NegativePrompt ??= string.Empty;
            profile.Memo ??= string.Empty;
            profile.Assets ??= new ObservableCollection<PlayerAsset>();

            foreach (PlayerAsset asset in profile.Assets)
            {
                NormalizeAsset(asset);
            }
        }

        private static void NormalizeAsset(PlayerAsset asset)
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
