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

            WriteDataFiles(profile, acceptedAssets, heroineExportDirectory);

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

        private void WriteDataFiles(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets, string heroineExportDirectory)
        {
            string dataDirectory = Path.Combine(heroineExportDirectory, "Data");
            File.WriteAllText(Path.Combine(dataDirectory, "heroine_profile_note.md"), BuildProfileNote(profile, acceptedAssets));
            File.WriteAllText(Path.Combine(dataDirectory, "heroine_profile_export.json"), BuildProfileExportJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "assets_export.json"), BuildAssetsExportJson(profile, acceptedAssets));
            File.WriteAllText(Path.Combine(dataDirectory, "conversations_export.json"), BuildConversationExportJson(profile, ConversationDataKind.Conversations));
            File.WriteAllText(Path.Combine(dataDirectory, "game_events_export.json"), BuildConversationExportJson(profile, ConversationDataKind.GameEvents));
            File.WriteAllText(Path.Combine(dataDirectory, "action_reactions_export.json"), BuildConversationExportJson(profile, ConversationDataKind.ActionReactions));
            File.WriteAllText(Path.Combine(dataDirectory, "endings_export.json"), BuildConversationExportJson(profile, ConversationDataKind.Endings));
            WriteDraftFile(Path.Combine(dataDirectory, "conversations_draft.md"), "会話案");
            WriteDraftFile(Path.Combine(dataDirectory, "game_events_draft.md"), "イベント案");
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
                likes = profile.Likes,
                dislikes = profile.Dislikes,
                appearancePrompt = profile.AppearancePrompt,
                stillCommonPositivePrompt = profile.StillCommonPositivePrompt,
                actionReactionPolicy = profile.ActionReactionPolicy,
                endingPolicy = profile.EndingPolicy
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

        private static void ValidateConversationEntries(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets, ExportReport report)
        {
            IReadOnlyList<ConversationEntry> entries = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .ToList();
            HashSet<string> acceptedAssetIds = new HashSet<string>(
                acceptedAssets.Select(asset => asset.AssetId).Where(assetId => !string.IsNullOrWhiteSpace(assetId)),
                StringComparer.OrdinalIgnoreCase);

            report.ConversationCount = entries.Count(entry => entry.Kind == ConversationDataKind.Conversations);
            report.GameEventCount = entries.Count(entry => entry.Kind == ConversationDataKind.GameEvents);
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

        private static void ValidateConversationEntry(ConversationEntry entry, HashSet<string> acceptedAssetIds, ExportReport report)
        {
            string label = BuildConversationWarningLabel(entry);
            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                report.Warnings.Add($"{label}: id が空です。");
            }

            if (entry.Priority < 0)
            {
                report.Warnings.Add($"{label}: priority が 0 未満です。");
            }

            if (entry.Conditions != null && entry.Conditions.MinAffection > entry.Conditions.MaxAffection)
            {
                report.Warnings.Add($"{label}: minAffection が maxAffection より大きいです。");
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
            }

            foreach (string assetId in SplitList(entry.ImageAssetIdsText))
            {
                if (!acceptedAssetIds.Contains(assetId))
                {
                    report.Warnings.Add($"{label}: imageAssetId `{assetId}` は Accepted 画像に存在しません。");
                }
            }
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
                        requiredItemId = entry.Conditions == null ? string.Empty : entry.Conditions.RequiredItemId,
                        once = entry.Conditions != null && entry.Conditions.Once,
                        requiredFlagIds = SplitList(entry.Conditions == null ? string.Empty : entry.Conditions.RequiredFlagIdsText)
                    },
                    lines = (entry.Lines ?? new System.Collections.ObjectModel.ObservableCollection<ConversationLine>())
                        .Select(line => new
                        {
                            speaker = line.Speaker,
                            text = line.Text,
                            expression = line.Expression
                        }).ToList(),
                    imageAssetIds = SplitList(entry.ImageAssetIdsText),
                    priority = entry.Priority,
                    memo = entry.Memo
                }).ToList()
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
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
