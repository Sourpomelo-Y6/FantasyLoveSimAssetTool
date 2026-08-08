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

        public HeroineProfile CreateCharacter(
            string heroineId,
            string displayName,
            bool overwriteExisting = false)
        {
            ValidateHeroineId(heroineId);

            string normalizedHeroineId = heroineId.Trim();
            if (HasExistingCharacterData(normalizedHeroineId) && !overwriteExisting)
            {
                throw new InvalidOperationException(
                    "同じ HeroineId のキャラクターデータが既に存在します。");
            }

            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = normalizedHeroineId,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? normalizedHeroineId : displayName.Trim(),
                StillCommonPositivePrompt = "clean lines,highly detailed,masterpiece,8k,best quality,very aesthetic,absurdres,newest"
            };
            profile.BattleSkillsSpecified = true;
            ApplyDefaultResourcePaths(profile);

            EnsureCharacterDirectories(profile.HeroineId);
            SaveProfile(profile);
            return profile;
        }

        public bool HasExistingCharacterData(string heroineId)
        {
            ValidateHeroineId(heroineId);
            return Directory.Exists(GetCharacterDirectory(heroineId.Trim()));
        }

        public void SaveProfile(HeroineProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            ValidateHeroineId(profile.HeroineId);
            profile.AppearancePrompt ??= string.Empty;
            profile.StillCommonPositivePrompt ??= string.Empty;
            NormalizeProfileCompatibilityFields(profile);
            profile.OutfitMessageOverrides ??= new ObservableCollection<OutfitMessageOverride>();
            profile.OutfitReactionMessageOverrides ??= new ObservableCollection<OutfitReactionMessageOverride>();
            profile.BattleSkills ??= new ObservableCollection<HeroineBattleSkill>();
            NormalizeTrainingImages(profile);
            NormalizeTrainingDialogues(profile);
            NormalizeTrainingCatalog(profile);
            profile.HeroineSkillTree = HeroineSkillTreeSyncService.Normalize(profile.HeroineSkillTree);
            BattleMessageSyncService.Normalize(profile);
            if (profile.BattleSkills.Count > 0)
            {
                profile.BattleSkillsSpecified = true;
            }
            profile.Assets ??= new ObservableCollection<HeroineAsset>();
            profile.StillWorkItems ??= new ObservableCollection<StillWorkItem>();
            profile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            NormalizeConversationEntries(profile.ConversationEntries);
            MenuActionDefinitionService.Normalize(profile);
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
            profile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            MenuActionDefinitionService.Normalize(profile);
            profile.OutfitMessageOverrides ??= new ObservableCollection<OutfitMessageOverride>();
            profile.OutfitReactionMessageOverrides ??= new ObservableCollection<OutfitReactionMessageOverride>();
            profile.BattleSkills ??= new ObservableCollection<HeroineBattleSkill>();
            NormalizeTrainingImages(profile);
            NormalizeTrainingDialogues(profile);
            NormalizeTrainingCatalog(profile);
            profile.HeroineSkillTree = HeroineSkillTreeSyncService.Normalize(profile.HeroineSkillTree);
            BattleMessageSyncService.Normalize(profile);
            profile.AppearancePrompt ??= string.Empty;
            profile.StillCommonPositivePrompt ??= string.Empty;
            NormalizeProfileCompatibilityFields(profile);
            if (string.IsNullOrWhiteSpace(profile.ConversationResourcePath)
                && string.IsNullOrWhiteSpace(profile.GameEventResourcePath)
                && string.IsNullOrWhiteSpace(profile.ActionResourcePath)
                && string.IsNullOrWhiteSpace(profile.ScheduledEventResourcePath)
                && string.IsNullOrWhiteSpace(profile.BattleResultEventResourcePath)
                && string.IsNullOrWhiteSpace(profile.BattlePanelResultMessageResourcePath)
                && string.IsNullOrWhiteSpace(profile.EndingResourcePath))
            {
                ApplyDefaultResourcePaths(profile);
            }
            NormalizeConversationEntries(profile.ConversationEntries);

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
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Battle"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Training"));
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

        private static void NormalizeConversationEntries(ObservableCollection<ConversationEntry> entries)
        {
            foreach (ConversationEntry entry in entries)
            {
                entry.Id ??= string.Empty;
                entry.Title ??= string.Empty;
                entry.Category ??= string.Empty;
                entry.Conditions ??= new ConversationCondition();
                entry.Lines ??= new ObservableCollection<ConversationLine>();
                entry.Choices ??= new ObservableCollection<ConversationChoice>();
                entry.ImageAssetIdsText ??= string.Empty;
                entry.EndingVisualMode = string.IsNullOrWhiteSpace(entry.EndingVisualMode)
                    ? "Auto"
                    : entry.EndingVisualMode;
                entry.Memo ??= string.Empty;
                entry.Conditions.LocationId ??= string.Empty;
                entry.Conditions.Weather ??= string.Empty;
                entry.Conditions.Season ??= string.Empty;
                entry.Conditions.TimeOfDay ??= string.Empty;
                entry.Conditions.ActionId ??= string.Empty;
                entry.Conditions.RequiredItemId ??= string.Empty;
                entry.Conditions.RequiredFlagIdsText ??= string.Empty;
                entry.Conditions.RequiredSkillIdsText ??= string.Empty;
                if (!string.IsNullOrWhiteSpace(entry.Conditions.RequiredSkillIdsText))
                {
                    entry.Conditions.RequiredSkillIdsSpecified = true;
                }

                foreach (ConversationLine line in entry.Lines)
                {
                    line.Speaker ??= string.Empty;
                    line.Text ??= string.Empty;
                    line.Expression ??= string.Empty;
                }

                foreach (ConversationChoice choice in entry.Choices)
                {
                    choice.ChoiceText ??= string.Empty;
                    choice.ResponseText ??= string.Empty;
                }
            }
        }

        private static void NormalizeProfileCompatibilityFields(HeroineProfile profile)
        {
            profile.InitialDialogueMessage ??= string.Empty;
            profile.NextActionPrompt ??= string.Empty;
            profile.MorningGreeting ??= string.Empty;
            profile.GoodNightGreeting ??= string.Empty;
            profile.GameStartFallbackMessage ??= string.Empty;
            profile.GameStartFollowUpMessage ??= string.Empty;
            profile.ConversationResourcePath ??= string.Empty;
            profile.GameEventResourcePath ??= string.Empty;
            profile.ActionResourcePath ??= string.Empty;
            profile.ScheduledEventResourcePath ??= string.Empty;
            profile.BattleResultEventResourcePath ??= string.Empty;
            profile.BattlePanelResultMessageResourcePath ??= string.Empty;
            profile.EndingResourcePath ??= string.Empty;
        }

        private static void NormalizeTrainingImages(HeroineProfile profile)
        {
            profile.TrainingImages ??= new TrainingImageSettings();
            profile.TrainingImages.Defaults ??= new TrainingImageDefaults();
            profile.TrainingImages.Items ??= new ObservableCollection<TrainingImageEntry>();
            TrainingImageDefaults defaults = profile.TrainingImages.Defaults;
            defaults.BeforeFirstStepImageAssetId ??= string.Empty;
            defaults.AfterFirstStepImageAssetId ??= string.Empty;
            defaults.PlayerLpConsumedImageAssetId ??= string.Empty;
            defaults.HeroineLpConsumedImageAssetId ??= string.Empty;
            defaults.SimultaneousLpConsumedImageAssetId ??= string.Empty;
            foreach (TrainingImageEntry item in profile.TrainingImages.Items)
            {
                item.TrainingId ??= string.Empty;
                item.BeforeFirstStepImageAssetId ??= string.Empty;
                item.AfterFirstStepImageAssetId ??= string.Empty;
                item.PlayerLpConsumedImageAssetId ??= string.Empty;
                item.HeroineLpConsumedImageAssetId ??= string.Empty;
                item.SimultaneousLpConsumedImageAssetId ??= string.Empty;
                item.Memo ??= string.Empty;
            }
        }

        private static void NormalizeTrainingDialogues(HeroineProfile profile)
        {
            profile.TrainingDialogues ??= new TrainingDialogueSettings();
            profile.TrainingDialogues.Items ??= new ObservableCollection<TrainingDialogueEntry>();
            foreach (TrainingDialogueEntry entry in profile.TrainingDialogues.Items)
            {
                if (entry != null)
                {
                    entry.TrainingId ??= string.Empty;
                    entry.VisualState ??= string.Empty;
                    entry.Messages ??= new ObservableCollection<TrainingDialogueMessage>();
                    foreach (TrainingDialogueMessage message in entry.Messages)
                    {
                        if (message != null)
                        {
                            message.Text ??= string.Empty;
                            message.VoiceId ??= string.Empty;
                        }
                    }
                }
            }
        }

        private static void NormalizeTrainingCatalog(HeroineProfile profile)
        {
            profile.TrainingCatalog ??= new TrainingCatalogSettings();
            profile.TrainingCatalog.Items ??= new ObservableCollection<TrainingCatalogItem>();
            foreach (TrainingCatalogItem item in profile.TrainingCatalog.Items)
            {
                if (item == null) continue;
                item.TrainingId ??= string.Empty;
                item.DisplayName ??= string.Empty;
                item.TrainingCategoryId ??= string.Empty;
                item.OccurrenceType ??= "Repeatable";
                item.VisibleConditionRanks ??= new List<string>();
                item.ExecutableConditionRanks ??= new List<string>();
                item.RequiredCompletedTrainingIds ??= new List<string>();
                item.UnlockNodeIds ??= new List<string>();
                item.UnlockNodeNames ??= new List<string>();
            }
            TrainingCatalogSyncService.RefreshReferenceWarnings(profile.TrainingCatalog);
        }

        private static void ApplyDefaultResourcePaths(HeroineProfile profile)
        {
            string root = $"Heroines/{profile.HeroineId}";
            profile.ConversationResourcePath = $"{root}/Conversations";
            profile.GameEventResourcePath = $"{root}/GameEvents";
            profile.ActionResourcePath = $"{root}/Actions";
            profile.ScheduledEventResourcePath = $"{root}/ScheduledEvents";
            profile.BattleResultEventResourcePath = $"{root}/BattleResultEvents";
            profile.BattlePanelResultMessageResourcePath = $"{root}/BattlePanelResultMessages";
            profile.EndingResourcePath = $"{root}/Endings";
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
