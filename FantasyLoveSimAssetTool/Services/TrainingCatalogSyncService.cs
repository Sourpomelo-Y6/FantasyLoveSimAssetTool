using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class TrainingCatalogMergeResult
    {
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public int WarningCount { get; set; }
    }

    public static class TrainingCatalogSyncService
    {
        private static readonly HashSet<string> ValidOccurrenceTypes =
            new HashSet<string>(new[] { "Repeatable", "OncePerSave" }, StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> ValidConditionRanks =
            new HashSet<string>(new[] { "Excellent", "Good", "Normal", "Poor", "Awful" }, StringComparer.OrdinalIgnoreCase);

        public static FromUnityTrainingCatalogDataFile DeserializeFromUnity(string json)
        {
            FromUnityTrainingCatalogDataFile data =
                JsonSerializer.Deserialize<FromUnityTrainingCatalogDataFile>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data == null)
                throw new InvalidOperationException("training_catalog_from_unity.json を読み込めませんでした。");
            if (data.SchemaVersion != 1)
                throw new InvalidOperationException($"未対応の schemaVersion です: {data.SchemaVersion}");
            return data;
        }

        public static TrainingCatalogMergeResult MergeFromUnity(
            TrainingCatalogSettings settings,
            string expectedHeroineId,
            FromUnityTrainingCatalogDataFile data)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (!string.IsNullOrWhiteSpace(data.HeroineId) &&
                !string.Equals(data.HeroineId, expectedHeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"HeroineId が選択中のキャラクターと一致しません。JSON: {data.HeroineId} / Selected: {expectedHeroineId}");
            }

            settings.Items ??= new ObservableCollection<TrainingCatalogItem>();
            TrainingCatalogMergeResult result = new TrainingCatalogMergeResult();
            HashSet<string> importedIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (FromUnityTrainingCatalogItem source in
                data.Items ?? new List<FromUnityTrainingCatalogItem>())
            {
                string trainingId = (source?.TrainingId ?? string.Empty).Trim();
                if (trainingId.Length == 0 || !importedIds.Add(trainingId))
                {
                    result.SkippedCount++;
                    continue;
                }

                TrainingCatalogItem target = settings.Items.FirstOrDefault(item =>
                    item != null && string.Equals(item.TrainingId, trainingId, StringComparison.Ordinal));
                if (target == null)
                {
                    target = new TrainingCatalogItem { TrainingId = trainingId };
                    settings.Items.Add(target);
                    result.AddedCount++;
                }
                else
                {
                    result.UpdatedCount++;
                }

                target.DisplayName = string.IsNullOrWhiteSpace(source.DisplayName)
                    ? trainingId
                    : source.DisplayName.Trim();
                target.TrainingCategoryId = (source.TrainingCategoryId ?? string.Empty).Trim();
                target.UnlockedByDefault = source.UnlockedByDefault;
                target.UnlockNodeIds = CleanIds(source.UnlockNodeIds);
                target.UnlockNodeNames = CleanIds(source.UnlockNodeNames);

                // 旧JSONに存在しない条件項目は、Toolに保存済みの値を維持する。
                if (source.SortOrder.HasValue) target.SortOrder = source.SortOrder.Value;
                if (source.OccurrenceType != null)
                {
                    string occurrence = source.OccurrenceType.Trim();
                    if (ValidOccurrenceTypes.Contains(occurrence))
                        target.OccurrenceType = ValidOccurrenceTypes.First(value =>
                            string.Equals(value, occurrence, StringComparison.OrdinalIgnoreCase));
                    else
                        result.WarningCount++;
                }
                if (source.VisibleConditionRanks != null)
                    target.VisibleConditionRanks = CleanRanks(source.VisibleConditionRanks, result);
                if (source.ExecutableConditionRanks != null)
                    target.ExecutableConditionRanks = CleanRanks(source.ExecutableConditionRanks, result);
                if (source.RequiredCompletedTrainingIds != null)
                    target.RequiredCompletedTrainingIds = CleanIds(source.RequiredCompletedTrainingIds);
                if (source.RequireAllCompletedTrainings.HasValue)
                    target.RequireAllCompletedTrainings = source.RequireAllCompletedTrainings.Value;
                if (source.HideUntilPrerequisitesMet.HasValue)
                    target.HideUntilPrerequisitesMet = source.HideUntilPrerequisitesMet.Value;
                if (source.HideAfterCompletion.HasValue)
                    target.HideAfterCompletion = source.HideAfterCompletion.Value;
            }

            result.WarningCount += RefreshReferenceWarnings(settings);
            return result;
        }

        public static int RefreshReferenceWarnings(TrainingCatalogSettings settings)
        {
            if (settings?.Items == null) return 0;
            HashSet<string> knownIds = new HashSet<string>(
                settings.Items.Where(item => item != null && !string.IsNullOrWhiteSpace(item.TrainingId))
                    .Select(item => item.TrainingId),
                StringComparer.Ordinal);
            int warningCount = 0;
            foreach (TrainingCatalogItem item in settings.Items.Where(item => item != null))
            {
                item.RequiredCompletedTrainingIds ??= new List<string>();
                List<string> missingIds = item.RequiredCompletedTrainingIds
                    .Where(id => !knownIds.Contains(id))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                item.ReferenceWarning = missingIds.Count > 0
                    ? "未登録の前提訓練: " + string.Join(" / ", missingIds)
                    : string.Empty;
                warningCount += missingIds.Count;
            }
            return warningCount;
        }

        private static List<string> CleanIds(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static List<string> CleanRanks(
            IEnumerable<string> values,
            TrainingCatalogMergeResult result)
        {
            List<string> cleaned = new List<string>();
            foreach (string value in CleanIds(values))
            {
                string canonical = ValidConditionRanks.FirstOrDefault(rank =>
                    string.Equals(rank, value, StringComparison.OrdinalIgnoreCase));
                if (canonical == null)
                {
                    result.WarningCount++;
                    continue;
                }
                if (!cleaned.Contains(canonical)) cleaned.Add(canonical);
            }
            return cleaned;
        }
    }
}
