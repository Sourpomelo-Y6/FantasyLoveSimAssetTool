using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasyLoveSimAssetTool.Services
{
    public class ExportService
    {
        private readonly CharacterProjectService characterProjectService;
        private readonly ImageInspectionService imageInspectionService;
        private readonly StillDefinitionService stillDefinitionService;
        private readonly DefinitionCatalogService definitionCatalogService;

        public string ExportDirectory
        {
            get { return Path.Combine(characterProjectService.WorkspaceRoot, "Export"); }
        }

        public ExportService(CharacterProjectService characterProjectService)
            : this(characterProjectService, new ImageInspectionService())
        {
        }

        public ExportService(CharacterProjectService characterProjectService, ImageInspectionService imageInspectionService)
        {
            this.characterProjectService = characterProjectService ?? throw new ArgumentNullException(nameof(characterProjectService));
            this.imageInspectionService = imageInspectionService ?? throw new ArgumentNullException(nameof(imageInspectionService));
            stillDefinitionService = new StillDefinitionService(this.characterProjectService.WorkspaceRoot);
            definitionCatalogService = new DefinitionCatalogService(this.characterProjectService.WorkspaceRoot);
        }

        public ExportReport ExportHeroine(HeroineProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (string.IsNullOrWhiteSpace(profile.HeroineId))
            {
                throw new ArgumentException("HeroineId is required.", nameof(profile));
            }

            string heroineExportDirectory = Path.Combine(ExportDirectory, profile.HeroineId);
            EnsureExportDirectories(heroineExportDirectory);

            IReadOnlyList<HeroineAsset> acceptedAssets = (profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(asset => asset.Status == AssetStatus.Accepted)
                .ToList();

            ExportReport report = new ExportReport
            {
                ExportPath = heroineExportDirectory,
                AcceptedAssetCount = acceptedAssets.Count
            };
            ValidateConversationEntries(profile, acceptedAssets, report);
            foreach (string warning in BattleMessageSyncService.Validate(
                profile,
                stillDefinitionService.GetDefaultDefinitions().Where(x => x != null).Select(x => x.AssetId),
                GetConversationCostumeIds()))
            {
                report.Warnings.Add(warning);
            }

            foreach (HeroineAsset asset in acceptedAssets)
            {
                if (ExportImage(profile, asset, heroineExportDirectory, report))
                {
                    report.ExportedImageCount++;
                }
                else
                {
                    report.SkippedImageCount++;
                }

                if (ExportPrompt(profile, asset, heroineExportDirectory, report))
                {
                    report.ExportedPromptCount++;
                }
            }

            WriteDataFiles(profile, acceptedAssets, heroineExportDirectory, report);

            return report;
        }

        private void EnsureExportDirectories(string heroineExportDirectory)
        {
            Directory.CreateDirectory(heroineExportDirectory);
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Sprites"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Event"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Actions"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Ending"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Battle"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Training"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Data"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Prompts"));
        }

        private bool ExportImage(HeroineProfile profile, HeroineAsset asset, string heroineExportDirectory, ExportReport report)
        {
            if (string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                report.Warnings.Add($"{asset.AssetId}: StoredPath が空のため画像を export できません。");
                return false;
            }

            string sourcePath = Path.Combine(characterProjectService.GetCharacterDirectory(profile.HeroineId), asset.StoredPath);
            if (!File.Exists(sourcePath))
            {
                report.Warnings.Add($"{asset.AssetId}: 画像ファイルが見つかりません: {sourcePath}");
                return false;
            }

            AddImageInspectionWarnings(asset, sourcePath, report);

            string fileName = GetExportFileName(asset, sourcePath);
            string destinationDirectory = Path.Combine(heroineExportDirectory, "Images", asset.Usage.ToString());
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(sourcePath, Path.Combine(destinationDirectory, fileName), true);
            return true;
        }

        private void AddImageInspectionWarnings(HeroineAsset asset, string sourcePath, ExportReport report)
        {
            try
            {
                ImageInspectionResult result = imageInspectionService.Inspect(sourcePath, asset.Usage);
                foreach (string warning in result.Warnings)
                {
                    report.Warnings.Add($"{asset.AssetId}: {warning}");
                }
            }
            catch (Exception ex)
            {
                report.Warnings.Add($"{asset.AssetId}: 画像検査に失敗しました: {ex.Message}");
            }
        }

        private bool ExportPrompt(HeroineProfile profile, HeroineAsset asset, string heroineExportDirectory, ExportReport report)
        {
            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                report.Warnings.Add($"{asset.AssetId}: PromptRecordPath が空のため prompt JSON を export できません。");
                return false;
            }

            string sourcePath = Path.Combine(characterProjectService.GetCharacterDirectory(profile.HeroineId), asset.PromptRecordPath);
            if (!File.Exists(sourcePath))
            {
                report.Warnings.Add($"{asset.AssetId}: prompt JSON が見つかりません: {sourcePath}");
                return false;
            }

            string destinationPath = Path.Combine(heroineExportDirectory, "Prompts", Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destinationPath, true);
            return true;
        }

        private void WriteDataFiles(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets, string heroineExportDirectory, ExportReport report)
        {
            string dataDirectory = Path.Combine(heroineExportDirectory, "Data");
            File.WriteAllText(Path.Combine(dataDirectory, "heroine_profile_note.md"), BuildProfileNote(profile, acceptedAssets));
            File.WriteAllText(Path.Combine(dataDirectory, "heroine_profile_export.json"), BuildProfileExportJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "assets_export.json"), BuildAssetsExportJson(profile, acceptedAssets));
            File.WriteAllText(Path.Combine(dataDirectory, "sprite_layers_export.json"), BuildSpriteLayersExportJson(profile, acceptedAssets, report));
            File.WriteAllText(Path.Combine(dataDirectory, "training_images_export.json"), BuildTrainingImagesExportJson(profile, acceptedAssets, report));
            File.WriteAllText(Path.Combine(dataDirectory, "training_dialogues_export.json"), TrainingDialogueSyncService.BuildExportJson(profile, report));
            File.WriteAllText(Path.Combine(dataDirectory, "heroine_skills_export.json"), HeroineSkillTreeSyncService.BuildExportJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "battle_result_events_export.json"), BattleMessageSyncService.BuildResultEventsJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "battle_panel_result_messages_export.json"), BattleMessageSyncService.BuildPanelMessagesJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "conversations_export.json"), BuildConversationExportJson(profile, ConversationDataKind.Conversations));
            File.WriteAllText(Path.Combine(dataDirectory, "game_events_export.json"), BuildConversationExportJson(profile, ConversationDataKind.GameEvents));
            File.WriteAllText(Path.Combine(dataDirectory, "scheduled_events_export.json"), BuildScheduledEventsExportJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "action_reactions_export.json"), BuildConversationExportJson(profile, ConversationDataKind.ActionReactions));
            File.WriteAllText(Path.Combine(dataDirectory, "endings_export.json"), BuildConversationExportJson(profile, ConversationDataKind.Endings));
            WriteDraftFile(Path.Combine(dataDirectory, "conversations_draft.md"), "会話案");
            WriteDraftFile(Path.Combine(dataDirectory, "game_events_draft.md"), "イベント案");
            WriteDraftFile(Path.Combine(dataDirectory, "scheduled_events_draft.md"), "予定イベント案");
            WriteDraftFile(Path.Combine(dataDirectory, "action_reactions_draft.md"), "行動反応案");
            WriteDraftFile(Path.Combine(dataDirectory, "endings_draft.md"), "エンディング案");
        }

        private static string BuildProfileNote(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Heroine Profile Note");
            builder.AppendLine();
            AppendValue(builder, "HeroineId", profile.HeroineId);
            AppendValue(builder, "表示名", profile.DisplayName);
            AppendValue(builder, "年齢", profile.Age);
            AppendValue(builder, "身長", profile.Height);
            builder.AppendLine();
            AppendSection(builder, "性格", profile.Personality);
            AppendSection(builder, "口調", profile.SpeakingStyle);
            AppendValue(builder, "一人称", profile.FirstPerson);
            AppendValue(builder, "二人称", profile.SecondPerson);
            builder.AppendLine();
            AppendSection(builder, "好きなもの", profile.Likes);
            AppendSection(builder, "苦手なもの", profile.Dislikes);
            AppendSection(builder, "容姿プロンプト", profile.AppearancePrompt);
            AppendSection(builder, "行動反応方針", profile.ActionReactionPolicy);
            AppendSection(builder, "エンディング方針", profile.EndingPolicy);

            builder.AppendLine("## Accepted Assets");
            builder.AppendLine();
            if (acceptedAssets.Count == 0)
            {
                builder.AppendLine("- なし");
            }
            else
            {
                foreach (HeroineAsset asset in acceptedAssets)
                {
                    builder.AppendLine($"- `{asset.AssetId}` / `{asset.Usage}` / `{asset.FileName}`");
                }
            }

            return builder.ToString();
        }

        private static string BuildProfileExportJson(HeroineProfile profile)
        {
            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                displayName = profile.DisplayName,
                age = profile.Age,
                height = profile.Height,
                personality = profile.Personality,
                speakingStyle = profile.SpeakingStyle,
                firstPerson = profile.FirstPerson,
                secondPerson = profile.SecondPerson,
                initialDialogueMessage = profile.InitialDialogueMessage,
                nextActionPrompt = profile.NextActionPrompt,
                morningGreeting = profile.MorningGreeting,
                goodNightGreeting = profile.GoodNightGreeting,
                gameStartFallbackMessage = profile.GameStartFallbackMessage,
                gameStartFollowUpMessage = profile.GameStartFollowUpMessage,
                likes = profile.Likes,
                dislikes = profile.Dislikes,
                appearancePrompt = profile.AppearancePrompt,
                stillCommonPositivePrompt = profile.StillCommonPositivePrompt,
                actionReactionPolicy = profile.ActionReactionPolicy,
                endingPolicy = profile.EndingPolicy,
                outfitMessageOverrides = (profile.OutfitMessageOverrides ?? new System.Collections.ObjectModel.ObservableCollection<OutfitMessageOverride>())
                    .Select(item => new
                    {
                        outfitId = item.OutfitId,
                        lockedMessage = item.LockedMessage,
                        changedMessage = item.ChangedMessage
                    }).ToList(),
                outfitReactionMessageOverrides = (profile.OutfitReactionMessageOverrides ?? new System.Collections.ObjectModel.ObservableCollection<OutfitReactionMessageOverride>())
                    .Select(item => new
                    {
                        reactionType = item.ReactionType,
                        message = item.Message
                    }).ToList(),
                battleSkills = BattleSkillSyncService.CreateExportValues(profile)?.Select(item => new
                    {
                        skillId = item.SkillId,
                        displayName = item.DisplayName,
                        effectType = item.EffectType,
                        target = item.Target,
                        cost = item.Cost,
                        power = item.Power,
                        affectedStat = item.AffectedStat,
                        statusDurationTurns = item.StatusDurationTurns,
                        useChancePercent = item.UseChancePercent,
                        priority = item.Priority,
                        maxUsesPerBattle = item.MaxUsesPerBattle
                    }).ToList(),
                conversationResourcePath = profile.ConversationResourcePath,
                gameEventResourcePath = profile.GameEventResourcePath,
                actionResourcePath = profile.ActionResourcePath,
                scheduledEventResourcePath = profile.ScheduledEventResourcePath,
                battleResultEventResourcePath = profile.BattleResultEventResourcePath,
                battlePanelResultMessageResourcePath = profile.BattlePanelResultMessageResourcePath,
                endingResourcePath = profile.EndingResourcePath
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static string BuildAssetsExportJson(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets)
        {
            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                unityImageRoot = $"Assets/Images/Heroines/{profile.HeroineId}",
                assets = acceptedAssets.Select(asset => new
                {
                    assetId = asset.AssetId,
                    usage = asset.Usage,
                    status = asset.Status,
                    fileName = GetExportFileName(asset, string.Empty),
                    memo = asset.Memo,
                    exportImagePath = ToExportRelativePath("Images", asset.Usage.ToString(), GetExportFileName(asset, string.Empty)),
                    exportPromptPath = string.IsNullOrWhiteSpace(asset.PromptRecordPath)
                        ? string.Empty
                        : ToExportRelativePath("Prompts", Path.GetFileName(asset.PromptRecordPath)),
                    unityImagePath = ToExportRelativePath(
                        "Assets",
                        "Images",
                        "Heroines",
                        profile.HeroineId,
                        asset.Usage.ToString(),
                        GetExportFileName(asset, string.Empty))
                }).ToList()
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static string BuildTrainingImagesExportJson(
            HeroineProfile profile,
            IReadOnlyList<HeroineAsset> acceptedAssets,
            ExportReport report)
        {
            TrainingImageSettings settings = profile.TrainingImages ?? new TrainingImageSettings();
            TrainingImageDefaults defaults = settings.Defaults ?? new TrainingImageDefaults();
            List<TrainingImageEntry> items = (settings.Items ?? new System.Collections.ObjectModel.ObservableCollection<TrainingImageEntry>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.TrainingId))
                .ToList();
            ValidateTrainingImageReferences(defaults, items, acceptedAssets, report);

            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                defaults = new
                {
                    beforeFirstStepImageAssetId = defaults.BeforeFirstStepImageAssetId,
                    afterFirstStepImageAssetId = defaults.AfterFirstStepImageAssetId,
                    playerLpConsumedImageAssetId = defaults.PlayerLpConsumedImageAssetId,
                    heroineLpConsumedImageAssetId = defaults.HeroineLpConsumedImageAssetId,
                    simultaneousLpConsumedImageAssetId = defaults.SimultaneousLpConsumedImageAssetId
                },
                items = items.Select(item => new
                {
                    trainingId = item.TrainingId,
                    beforeFirstStepImageAssetId = item.BeforeFirstStepImageAssetId,
                    afterFirstStepImageAssetId = item.AfterFirstStepImageAssetId,
                    playerLpConsumedImageAssetId = item.PlayerLpConsumedImageAssetId,
                    heroineLpConsumedImageAssetId = item.HeroineLpConsumedImageAssetId,
                    simultaneousLpConsumedImageAssetId = item.SimultaneousLpConsumedImageAssetId,
                    memo = item.Memo
                }).ToList()
            };
            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static void ValidateTrainingImageReferences(
            TrainingImageDefaults defaults,
            IReadOnlyList<TrainingImageEntry> items,
            IReadOnlyList<HeroineAsset> acceptedAssets,
            ExportReport report)
        {
            Dictionary<string, HeroineAsset> assets = acceptedAssets
                .Where(asset => asset != null && !string.IsNullOrWhiteSpace(asset.AssetId))
                .GroupBy(asset => asset.AssetId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            List<string> references = new List<string>
            {
                defaults.BeforeFirstStepImageAssetId,
                defaults.AfterFirstStepImageAssetId,
                defaults.PlayerLpConsumedImageAssetId,
                defaults.HeroineLpConsumedImageAssetId,
                defaults.SimultaneousLpConsumedImageAssetId
            };
            foreach (TrainingImageEntry item in items)
            {
                references.Add(item.BeforeFirstStepImageAssetId);
                references.Add(item.AfterFirstStepImageAssetId);
                references.Add(item.PlayerLpConsumedImageAssetId);
                references.Add(item.HeroineLpConsumedImageAssetId);
                references.Add(item.SimultaneousLpConsumedImageAssetId);
            }

            foreach (string assetId in references.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!assets.TryGetValue(assetId, out HeroineAsset asset))
                {
                    report.Warnings.Add($"{assetId}: 訓練画像の対応先がAccepted画像にありません。");
                }
                else if (asset.Usage != AssetUsage.Training)
                {
                    report.Warnings.Add($"{assetId}: 訓練画像の用途はTrainingが必要です。現在: {asset.Usage}");
                }
            }
        }

        private string BuildSpriteLayersExportJson(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets, ExportReport report)
        {
            IReadOnlyList<LayerAssetDefinition> layerDefinitions = stillDefinitionService.GetLayerAssetDefinitions();
            Dictionary<string, HeroineAsset> acceptedAssetById = acceptedAssets
                .Where(asset => !string.IsNullOrWhiteSpace(asset.AssetId))
                .GroupBy(asset => asset.AssetId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            List<object> exportedLayers = new List<object>();

            ValidateSpriteLayerDefinitions(profile, layerDefinitions, acceptedAssetById, report);

            foreach (LayerAssetDefinition layer in layerDefinitions)
            {
                if (!acceptedAssetById.TryGetValue(layer.AssetId, out HeroineAsset asset))
                {
                    report.Warnings.Add($"{layer.AssetId}: レイヤー素材が Accepted 画像として登録されていません。");
                    continue;
                }

                if (asset.Usage != AssetUsage.Sprites)
                {
                    report.Warnings.Add($"{layer.AssetId}: レイヤー素材の用途は Sprites が想定です。現在: {asset.Usage}");
                }

                string fileName = GetExportFileName(asset, string.Empty);
                exportedLayers.Add(new
                {
                    assetId = layer.AssetId,
                    layerKind = layer.LayerKind,
                    costumeId = layer.CostumeId,
                    expressionId = layer.ExpressionId,
                    displayName = layer.DisplayName,
                    drawOrder = layer.DrawOrder,
                    fileName,
                    exportImagePath = ToExportRelativePath("Images", asset.Usage.ToString(), fileName),
                    unityImagePath = ToExportRelativePath(
                        "Assets",
                        "Images",
                        "Heroines",
                        profile.HeroineId,
                        asset.Usage.ToString(),
                        fileName)
                });
            }

            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                unityImageRoot = $"Assets/Images/Heroines/{profile.HeroineId}",
                layers = exportedLayers
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private void ValidateSpriteLayerDefinitions(
            HeroineProfile profile,
            IReadOnlyList<LayerAssetDefinition> layerDefinitions,
            Dictionary<string, HeroineAsset> acceptedAssetById,
            ExportReport report)
        {
            if (layerDefinitions == null || layerDefinitions.Count == 0)
            {
                return;
            }

            ValidateLayerDuplicateKeys(layerDefinitions, report);
            ValidateLayerRequiredFields(layerDefinitions, report);
            ValidateRequiredAcceptedLayers(layerDefinitions, acceptedAssetById, report);
            ValidateLayerImages(profile, layerDefinitions, acceptedAssetById, report);
        }

        private static void ValidateLayerDuplicateKeys(IReadOnlyList<LayerAssetDefinition> layerDefinitions, ExportReport report)
        {
            foreach (IGrouping<string, LayerAssetDefinition> group in layerDefinitions
                .Where(layer => layer != null && !string.IsNullOrWhiteSpace(layer.AssetId))
                .GroupBy(layer => layer.AssetId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1))
            {
                report.Warnings.Add($"sprite_layers_export: assetId `{group.Key}` のレイヤー定義が重複しています。");
            }

            foreach (IGrouping<string, LayerAssetDefinition> group in layerDefinitions
                .Where(layer => layer != null && !string.IsNullOrWhiteSpace(layer.LayerKind))
                .GroupBy(BuildLayerDisplayKey, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1))
            {
                report.Warnings.Add($"sprite_layers_export: layerKind + costumeId + expressionId `{group.Key}` のレイヤー定義が重複しています。");
            }
        }

        private static void ValidateLayerRequiredFields(IReadOnlyList<LayerAssetDefinition> layerDefinitions, ExportReport report)
        {
            foreach (LayerAssetDefinition layer in layerDefinitions.Where(layer => layer != null))
            {
                string label = BuildLayerWarningLabel(layer);
                string layerKind = layer.LayerKind?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(layerKind))
                {
                    report.Warnings.Add($"{label}: layerKind が空です。");
                }
                else if (!IsKnownLayerKind(layerKind))
                {
                    report.Warnings.Add($"{label}: layerKind `{layerKind}` は候補外です。");
                }

                if (string.Equals(layerKind, "Costume", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(layer.CostumeId))
                {
                    report.Warnings.Add($"{label}: layerKind が Costume なのに costumeId が空です。");
                }

                if (string.Equals(layerKind, "Expression", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(layer.ExpressionId))
                {
                    report.Warnings.Add($"{label}: layerKind が Expression なのに expressionId が空です。");
                }
            }
        }

        private static void ValidateRequiredAcceptedLayers(
            IReadOnlyList<LayerAssetDefinition> layerDefinitions,
            Dictionary<string, HeroineAsset> acceptedAssetById,
            ExportReport report)
        {
            if (!HasAcceptedLayer(layerDefinitions, acceptedAssetById, layer => IsLayerKind(layer, "BaseBody")))
            {
                report.Warnings.Add("sprite_layers_export: Accepted 済みの BaseBody レイヤーがありません。");
            }

            if (!HasAcceptedLayer(layerDefinitions, acceptedAssetById, layer =>
                    IsLayerKind(layer, "Costume")
                    && string.Equals(layer.CostumeId?.Trim(), "Default", StringComparison.OrdinalIgnoreCase)))
            {
                report.Warnings.Add("sprite_layers_export: Accepted 済みの Default 衣装レイヤーがありません。");
            }

            if (!HasAcceptedLayer(layerDefinitions, acceptedAssetById, layer =>
                    IsLayerKind(layer, "Expression")
                    && string.Equals(layer.ExpressionId?.Trim(), "Neutral", StringComparison.OrdinalIgnoreCase)))
            {
                report.Warnings.Add("sprite_layers_export: Accepted 済みの Neutral 表情レイヤーがありません。");
            }
        }

        private void ValidateLayerImages(
            HeroineProfile profile,
            IReadOnlyList<LayerAssetDefinition> layerDefinitions,
            Dictionary<string, HeroineAsset> acceptedAssetById,
            ExportReport report)
        {
            Dictionary<string, ImageInspectionResult> inspectionByAssetId = new Dictionary<string, ImageInspectionResult>(StringComparer.OrdinalIgnoreCase);
            foreach (LayerAssetDefinition layer in layerDefinitions.Where(layer => layer != null && !string.IsNullOrWhiteSpace(layer.AssetId)))
            {
                if (!acceptedAssetById.TryGetValue(layer.AssetId, out HeroineAsset asset))
                {
                    continue;
                }

                string imagePath = BuildStoredImagePath(profile, asset);
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                {
                    continue;
                }

                try
                {
                    ImageInspectionResult result = imageInspectionService.Inspect(imagePath, asset.Usage);
                    inspectionByAssetId[layer.AssetId] = result;
                    if (!string.Equals(result.FileFormat, "PNG", StringComparison.OrdinalIgnoreCase))
                    {
                        report.Warnings.Add($"{layer.AssetId}: レイヤー画像は透過 PNG が想定です。現在: {result.FileFormat}");
                    }

                    if (!result.HasTransparentPixels)
                    {
                        report.Warnings.Add($"{layer.AssetId}: レイヤー画像に透過ピクセルが見つかりません。");
                    }
                }
                catch (Exception ex)
                {
                    report.Warnings.Add($"{layer.AssetId}: レイヤー画像検査に失敗しました: {ex.Message}");
                }
            }

            ImageInspectionResult baseBodyResult = FindBaseBodyInspection(layerDefinitions, inspectionByAssetId);
            if (baseBodyResult == null)
            {
                return;
            }

            foreach (LayerAssetDefinition layer in layerDefinitions.Where(layer => layer != null && !IsLayerKind(layer, "BaseBody")))
            {
                if (string.IsNullOrWhiteSpace(layer.AssetId)
                    || !inspectionByAssetId.TryGetValue(layer.AssetId, out ImageInspectionResult result))
                {
                    continue;
                }

                if (result.PixelWidth != baseBodyResult.PixelWidth || result.PixelHeight != baseBodyResult.PixelHeight)
                {
                    report.Warnings.Add($"{layer.AssetId}: レイヤー画像のキャンバスサイズが BaseBody と一致しません。BaseBody {baseBodyResult.PixelWidth}x{baseBodyResult.PixelHeight} / 現在 {result.PixelWidth}x{result.PixelHeight}");
                }

                if (!HasSameAspectRatio(result, baseBodyResult))
                {
                    report.Warnings.Add($"{layer.AssetId}: レイヤー画像の縦横比が BaseBody と一致しません。");
                }
            }
        }

        private string BuildStoredImagePath(HeroineProfile profile, HeroineAsset asset)
        {
            if (profile == null || asset == null || string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                return string.Empty;
            }

            return Path.Combine(characterProjectService.GetCharacterDirectory(profile.HeroineId), asset.StoredPath);
        }

        private static ImageInspectionResult FindBaseBodyInspection(
            IReadOnlyList<LayerAssetDefinition> layerDefinitions,
            Dictionary<string, ImageInspectionResult> inspectionByAssetId)
        {
            LayerAssetDefinition baseBodyLayer = layerDefinitions
                .Where(layer => layer != null && IsLayerKind(layer, "BaseBody") && !string.IsNullOrWhiteSpace(layer.AssetId))
                .OrderBy(layer => layer.DrawOrder)
                .FirstOrDefault(layer => inspectionByAssetId.ContainsKey(layer.AssetId));

            if (baseBodyLayer == null)
            {
                return null;
            }

            return inspectionByAssetId[baseBodyLayer.AssetId];
        }

        private static bool HasAcceptedLayer(
            IReadOnlyList<LayerAssetDefinition> layerDefinitions,
            Dictionary<string, HeroineAsset> acceptedAssetById,
            Func<LayerAssetDefinition, bool> predicate)
        {
            return layerDefinitions
                .Where(layer => layer != null && !string.IsNullOrWhiteSpace(layer.AssetId))
                .Any(layer => predicate(layer) && acceptedAssetById.ContainsKey(layer.AssetId));
        }

        private static string BuildLayerDisplayKey(LayerAssetDefinition layer)
        {
            return string.Join(
                "|",
                layer.LayerKind?.Trim() ?? string.Empty,
                layer.CostumeId?.Trim() ?? string.Empty,
                layer.ExpressionId?.Trim() ?? string.Empty);
        }

        private static string BuildLayerWarningLabel(LayerAssetDefinition layer)
        {
            string assetId = string.IsNullOrWhiteSpace(layer.AssetId) ? "(assetId未設定)" : layer.AssetId.Trim();
            return $"sprite_layers_export `{assetId}`";
        }

        private static bool IsLayerKind(LayerAssetDefinition layer, string layerKind)
        {
            return layer != null
                && string.Equals(layer.LayerKind?.Trim(), layerKind, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsKnownLayerKind(string layerKind)
        {
            return string.Equals(layerKind, "BaseBody", StringComparison.OrdinalIgnoreCase)
                || string.Equals(layerKind, "Costume", StringComparison.OrdinalIgnoreCase)
                || string.Equals(layerKind, "Expression", StringComparison.OrdinalIgnoreCase)
                || string.Equals(layerKind, "Accessory", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSameAspectRatio(ImageInspectionResult first, ImageInspectionResult second)
        {
            if (first.PixelHeight == 0 || second.PixelHeight == 0)
            {
                return true;
            }

            double firstRatio = (double)first.PixelWidth / first.PixelHeight;
            double secondRatio = (double)second.PixelWidth / second.PixelHeight;
            return Math.Abs(firstRatio - secondRatio) < 0.001;
        }

        private void ValidateConversationEntries(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets, ExportReport report)
        {
            IReadOnlyList<ConversationEntry> entries = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .ToList();
            HashSet<string> acceptedAssetIds = new HashSet<string>(
                acceptedAssets.Select(asset => asset.AssetId).Where(assetId => !string.IsNullOrWhiteSpace(assetId)),
                StringComparer.OrdinalIgnoreCase);

            report.ConversationCount = entries.Count(entry => entry.Kind == ConversationDataKind.Conversations);
            report.GameEventCount = entries.Count(entry => entry.Kind == ConversationDataKind.GameEvents);
            report.ScheduledEventCount = entries.Count(entry => entry.Kind == ConversationDataKind.ScheduledEvents);
            report.ActionReactionCount = entries.Count(entry => entry.Kind == ConversationDataKind.ActionReactions);
            report.EndingCount = entries.Count(entry => entry.Kind == ConversationDataKind.Endings);

            foreach (IGrouping<ConversationDataKind, ConversationEntry> group in entries.GroupBy(entry => entry.Kind))
            {
                foreach (IGrouping<string, ConversationEntry> duplicateGroup in group
                    .Where(entry => !string.IsNullOrWhiteSpace(entry.Id))
                    .GroupBy(entry => entry.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Where(idGroup => idGroup.Count() > 1))
                {
                    report.Warnings.Add($"{GetConversationKindLabel(group.Key)}: id `{duplicateGroup.Key}` が重複しています。");
                }
            }

            foreach (ConversationEntry entry in entries)
            {
                ValidateConversationEntry(entry, acceptedAssetIds, report);
            }
        }

        private void ValidateConversationEntry(ConversationEntry entry, HashSet<string> acceptedAssetIds, ExportReport report)
        {
            string label = BuildConversationWarningLabel(entry);
            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                report.Warnings.Add($"{label}: id が空です。");
            }

            if (string.IsNullOrWhiteSpace(entry.Title))
            {
                report.Warnings.Add($"{label}: title が空です。");
            }

            if (string.IsNullOrWhiteSpace(entry.Category))
            {
                report.Warnings.Add($"{label}: category が空です。");
            }

            if (entry.Priority < 0)
            {
                report.Warnings.Add($"{label}: priority が 0 未満です。");
            }

            if (entry.Conditions != null && entry.Conditions.MinAffection > entry.Conditions.MaxAffection)
            {
                report.Warnings.Add($"{label}: minAffection が maxAffection より大きいです。");
            }

            if (entry.Conditions != null)
            {
                if (entry.Kind == ConversationDataKind.GameEvents
                    && entry.Conditions.Once
                    && string.IsNullOrWhiteSpace(entry.Conditions.RequiredFlagIdsText))
                {
                    report.Warnings.Add($"{label}: once が true のイベントは requiredFlagIds を指定してください。");
                }

                if (entry.Kind == ConversationDataKind.ScheduledEvents)
                {
                    ValidateCatalogValue(label, "scheduleType", entry.Category, ConversationValueCatalog.ScheduledEventTypes, report);
                    ValidateCatalogValue(label, "actionId", entry.Conditions.ActionId, ConversationValueCatalog.ScheduledEventActions, report);
                    ValidateCatalogValue(label, "triggerTimeSlot", entry.Conditions.TimeOfDay, ConversationValueCatalog.ScheduledTimeSlots, report);
                }
                else
                {
                    ValidateCatalogValue(label, "actionId", entry.Conditions.ActionId, ConversationValueCatalog.Actions, report);
                }

                ValidateCatalogValue(label, "locationId", entry.Conditions.LocationId, ConversationValueCatalog.Locations, report);
                ValidateCatalogValue(label, "weather", entry.Conditions.Weather, ConversationValueCatalog.Weather, report);
                ValidateCatalogValue(label, "season", entry.Conditions.Season, ConversationValueCatalog.Seasons, report);
                ValidateCatalogValue(label, "costumeId", entry.Conditions.CostumeId, GetConversationCostumeIds(), report);
                if (entry.Kind != ConversationDataKind.ScheduledEvents)
                {
                    ValidateCatalogValue(label, "timeOfDay", entry.Conditions.TimeOfDay, ConversationValueCatalog.TimeOfDay, report);
                }
            }

            IReadOnlyList<ConversationLine> lines = (entry.Lines ?? new System.Collections.ObjectModel.ObservableCollection<ConversationLine>()).ToList();
            if (lines.Count == 0)
            {
                report.Warnings.Add($"{label}: 台詞行が空です。");
            }

            for (int index = 0; index < lines.Count; index++)
            {
                ConversationLine line = lines[index];
                if (string.IsNullOrWhiteSpace(line.Speaker))
                {
                    report.Warnings.Add($"{label}: {index + 1} 行目の speaker が空です。");
                }

                if (string.IsNullOrWhiteSpace(line.Text))
                {
                    report.Warnings.Add($"{label}: {index + 1} 行目の text が空です。");
                }

                ValidateCatalogValue(label, $"{index + 1} 行目の expression", line.Expression, ConversationValueCatalog.Expressions, report);
            }

            foreach (string assetId in SplitList(entry.ImageAssetIdsText))
            {
                if (!acceptedAssetIds.Contains(assetId))
                {
                    report.Warnings.Add($"{label}: imageAssetId `{assetId}` は Accepted 画像に存在しません。");
                }
            }

            IReadOnlyList<ConversationChoice> choices = (entry.Choices ?? new System.Collections.ObjectModel.ObservableCollection<ConversationChoice>()).ToList();
            for (int index = 0; index < choices.Count; index++)
            {
                ConversationChoice choice = choices[index];
                if (string.IsNullOrWhiteSpace(choice.ChoiceText))
                {
                    report.Warnings.Add($"{label}: {index + 1} 番目の choiceText が空です。");
                }

                if (string.IsNullOrWhiteSpace(choice.ResponseText))
                {
                    report.Warnings.Add($"{label}: {index + 1} 番目の responseText が空です。");
                }
            }
        }

        private static void ValidateCatalogValue(string label, string fieldName, string value, IReadOnlyCollection<string> allowedValues, ExportReport report)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!allowedValues.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                report.Warnings.Add($"{label}: {fieldName} `{value}` は候補外です。");
            }
        }

        private IReadOnlyCollection<string> GetConversationCostumeIds()
        {
            List<string> values = new List<string>(ConversationValueCatalog.Costumes);
            try
            {
                CostumeDefinitionFile file = definitionCatalogService.LoadCostumeDefinitionFile();
                values.AddRange((file.Costumes ?? new List<CostumeDefinition>())
                    .Where(costume => costume != null && !string.IsNullOrWhiteSpace(costume.CostumeId))
                    .Select(costume => costume.CostumeId.Trim()));
            }
            catch
            {
                // Export validation should continue even if optional definition files are unavailable.
            }

            return values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildConversationWarningLabel(ConversationEntry entry)
        {
            string id = string.IsNullOrWhiteSpace(entry.Id) ? "(id未設定)" : entry.Id;
            return $"{GetConversationKindLabel(entry.Kind)} `{id}`";
        }

        private static string GetConversationKindLabel(ConversationDataKind kind)
        {
            switch (kind)
            {
                case ConversationDataKind.GameEvents:
                    return "イベント";
                case ConversationDataKind.ScheduledEvents:
                    return "予定イベント";
                case ConversationDataKind.ActionReactions:
                    return "行動反応";
                case ConversationDataKind.Endings:
                    return "エンディング";
                default:
                    return "会話";
            }
        }

        private static string BuildConversationExportJson(HeroineProfile profile, ConversationDataKind kind)
        {
            IReadOnlyList<ConversationEntry> entries = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(entry => entry.Kind == kind)
                .ToList();

            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                kind = kind.ToString(),
                items = entries.Select(entry => new
                {
                    id = entry.Id,
                    title = entry.Title,
                    category = entry.Category,
                    conditions = new
                    {
                        locationId = entry.Conditions == null ? string.Empty : entry.Conditions.LocationId,
                        minAffection = entry.Conditions == null ? 0 : entry.Conditions.MinAffection,
                        maxAffection = entry.Conditions == null ? 100 : entry.Conditions.MaxAffection,
                        weather = entry.Conditions == null ? string.Empty : entry.Conditions.Weather,
                        season = entry.Conditions == null ? string.Empty : entry.Conditions.Season,
                        timeOfDay = entry.Conditions == null ? string.Empty : entry.Conditions.TimeOfDay,
                        actionId = entry.Conditions == null ? string.Empty : entry.Conditions.ActionId,
                        costumeId = entry.Conditions == null ? string.Empty : entry.Conditions.CostumeId,
                        requiredItemId = entry.Conditions == null ? string.Empty : entry.Conditions.RequiredItemId,
                        once = entry.Conditions != null && entry.Conditions.Once,
                        requiredFlagIds = SplitList(entry.Conditions == null ? string.Empty : entry.Conditions.RequiredFlagIdsText),
                        requiredSkillIds = RequiredSkillIdSyncService.CreateExportValues(entry.Conditions)
                    },
                    lines = (entry.Lines ?? new System.Collections.ObjectModel.ObservableCollection<ConversationLine>())
                        .Select(line => new
                        {
                            speaker = line.Speaker,
                            text = line.Text,
                            expression = line.Expression
                        }).ToList(),
                    choices = (entry.Choices ?? new System.Collections.ObjectModel.ObservableCollection<ConversationChoice>())
                        .Select(choice => new
                        {
                            choiceText = choice.ChoiceText,
                            responseText = choice.ResponseText,
                            affectionChange = choice.AffectionChange ?? 0
                        }).ToList(),
                    imageAssetIds = SplitList(entry.ImageAssetIdsText),
                    priority = entry.Priority,
                    memo = entry.Memo
                }).ToList()
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static string BuildScheduledEventsExportJson(HeroineProfile profile)
        {
            IReadOnlyList<ConversationEntry> entries = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(entry => entry.Kind == ConversationDataKind.ScheduledEvents)
                .ToList();

            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                kind = ConversationDataKind.ScheduledEvents.ToString(),
                items = entries.Select(entry =>
                {
                    IReadOnlyList<ConversationLine> lines = (entry.Lines ?? new System.Collections.ObjectModel.ObservableCollection<ConversationLine>()).ToList();
                    string preparationMessage = lines.Count > 0 ? lines[0].Text ?? string.Empty : string.Empty;
                    string eventMessage = lines.Count > 1
                        ? string.Join(Environment.NewLine, lines.Skip(1).Select(line => line.Text ?? string.Empty))
                        : string.Empty;
                    return new
                    {
                        id = entry.Id,
                        title = entry.Title,
                        category = entry.Category,
                        conditions = new
                        {
                            scheduleType = string.IsNullOrWhiteSpace(entry.Category) ? string.Empty : entry.Category,
                            actionId = entry.Conditions == null ? string.Empty : entry.Conditions.ActionId,
                            triggerTimeSlot = entry.Conditions == null ? string.Empty : entry.Conditions.TimeOfDay,
                            costumeId = entry.Conditions == null ? string.Empty : entry.Conditions.CostumeId,
                            outfitPromptMode = "Conditional",
                            eventSpeakerType = "Heroine",
                            affectionChange = 1
                        },
                        preparationMessage,
                        eventMessage,
                        lines = lines
                            .Select(line => new
                            {
                                speaker = line.Speaker,
                                text = line.Text,
                                expression = line.Expression
                            }).ToList(),
                        imageAssetIds = SplitList(entry.ImageAssetIdsText),
                        priority = entry.Priority,
                        memo = entry.Memo
                    };
                }).ToList()
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        private static string ToExportRelativePath(params string[] segments)
        {
            return string.Join("/", segments.Where(segment => !string.IsNullOrWhiteSpace(segment)));
        }

        private static IReadOnlyList<string> SplitList(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            return text
                .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct()
                .ToList();
        }

        private static string GetExportFileName(HeroineAsset asset, string sourcePath)
        {
            if (!string.IsNullOrWhiteSpace(asset.FileName))
            {
                return asset.FileName;
            }

            if (!string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                return Path.GetFileName(asset.StoredPath);
            }

            return string.IsNullOrWhiteSpace(sourcePath) ? string.Empty : Path.GetFileName(sourcePath);
        }

        private static void WriteDraftFile(string path, string title)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# " + title);
            builder.AppendLine();
            builder.AppendLine("## Draft");
            builder.AppendLine();
            File.WriteAllText(path, builder.ToString());
        }

        private static void AppendValue(StringBuilder builder, string label, string value)
        {
            builder.AppendLine($"- {label}: {value ?? string.Empty}");
        }

        private static void AppendSection(StringBuilder builder, string title, string value)
        {
            builder.AppendLine("## " + title);
            builder.AppendLine();
            builder.AppendLine(value ?? string.Empty);
            builder.AppendLine();
        }
    }
}
