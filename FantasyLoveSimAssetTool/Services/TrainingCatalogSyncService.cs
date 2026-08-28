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

        public static string BuildExportJson(HeroineProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                source = "FantasyLoveSimAssetTool",
                items = (profile.TrainingCatalog?.Items ?? new ObservableCollection<TrainingCatalogItem>())
                    .Where(item => item != null)
                    .Select(item => new
                    {
                        trainingId = item.TrainingId,
                        displayName = item.DisplayName,
                        trainingCategoryId = item.TrainingCategoryId,
                        unlockedByDefault = item.UnlockedByDefault,
                        sortOrder = item.SortOrder,
                        occurrenceType = item.OccurrenceType,
                        visibleConditionRanks = item.VisibleConditionRanks,
                        executableConditionRanks = item.ExecutableConditionRanks,
                        requiredCompletedTrainingIds = item.RequiredCompletedTrainingIds,
                        requireAllCompletedTrainings = item.RequireAllCompletedTrainings,
                        hideUntilPrerequisitesMet = item.HideUntilPrerequisitesMet,
                        hideAfterCompletion = item.HideAfterCompletion,
                        unlockNodeIds = item.UnlockNodeIds,
                        unlockNodeNames = item.UnlockNodeNames
                    }).ToList()
            };
            return JsonSerializer.Serialize(exportModel, new JsonSerializerOptions { WriteIndented = true });
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
                target.IsToolCreated = false;
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
            return RefreshReferenceWarnings(settings, null);
        }

        public static int RefreshReferenceWarnings(
            TrainingCatalogSettings settings,
            IEnumerable<string> knownUnlockNodeIds)
        {
            if (settings?.Items == null) return 0;
            List<TrainingCatalogItem> items = settings.Items.Where(item => item != null).ToList();
            HashSet<string> knownIds = new HashSet<string>(
                items.Where(item => !string.IsNullOrWhiteSpace(item.TrainingId))
                    .Select(item => item.TrainingId),
                StringComparer.OrdinalIgnoreCase);
            HashSet<string> knownNodes = knownUnlockNodeIds == null ? null : new HashSet<string>(
                knownUnlockNodeIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()),
                StringComparer.OrdinalIgnoreCase);
            HashSet<string> duplicateIds = new HashSet<string>(items
                .Where(item => !string.IsNullOrWhiteSpace(item.TrainingId))
                .GroupBy(item => item.TrainingId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1).Select(group => group.Key), StringComparer.OrdinalIgnoreCase);
            int warningCount = 0;
            foreach (TrainingCatalogItem item in items)
            {
                item.RequiredCompletedTrainingIds ??= new List<string>();
                item.UnlockNodeIds ??= new List<string>();
                List<string> warnings = new List<string>();
                string itemId = (item.TrainingId ?? string.Empty).Trim();
                if (itemId.Length == 0) warnings.Add("TrainingId が空です");
                else if (duplicateIds.Contains(itemId)) warnings.Add("TrainingId が重複しています");
                List<string> missingIds = item.RequiredCompletedTrainingIds
                    .Where(id => !string.IsNullOrWhiteSpace(id) && !knownIds.Contains(id.Trim()))
                    .Select(id => id.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (missingIds.Count > 0) warnings.Add("未登録の前提訓練: " + string.Join(" / ", missingIds));
                if (item.RequiredCompletedTrainingIds.Any(id =>
                    string.Equals(id?.Trim(), itemId, StringComparison.OrdinalIgnoreCase)))
                    warnings.Add("自分自身を前提訓練に指定しています");
                if (knownNodes != null)
                {
                    List<string> missingNodes = item.UnlockNodeIds
                        .Where(id => !string.IsNullOrWhiteSpace(id) && !knownNodes.Contains(id.Trim()))
                        .Select(id => id.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    if (missingNodes.Count > 0) warnings.Add("未登録の解放ノード: " + string.Join(" / ", missingNodes));
                }
                item.ReferenceWarning = string.Join(" / ", warnings);
                warningCount += warnings.Count;
            }

            HashSet<string> cyclicIds = FindCyclicTrainingIds(items, knownIds);
            foreach (TrainingCatalogItem item in items.Where(item =>
                cyclicIds.Contains((item.TrainingId ?? string.Empty).Trim())))
            {
                item.ReferenceWarning = string.IsNullOrWhiteSpace(item.ReferenceWarning)
                    ? "前提訓練が循環しています"
                    : item.ReferenceWarning + " / 前提訓練が循環しています";
                warningCount++;
            }
            return warningCount;
        }

        private static HashSet<string> FindCyclicTrainingIds(
            IEnumerable<TrainingCatalogItem> items,
            HashSet<string> knownIds)
        {
            Dictionary<string, List<string>> graph = items
                .Where(item => !string.IsNullOrWhiteSpace(item.TrainingId))
                .GroupBy(item => item.TrainingId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key,
                    group => group.First().RequiredCompletedTrainingIds
                        .Where(id => !string.IsNullOrWhiteSpace(id) && knownIds.Contains(id.Trim()))
                        .Select(id => id.Trim()).ToList(), StringComparer.OrdinalIgnoreCase);
            HashSet<string> cyclic = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string start in graph.Keys)
            {
                FindCycles(start, graph, new List<string>(), new HashSet<string>(StringComparer.OrdinalIgnoreCase), cyclic);
            }
            return cyclic;
        }

        private static void FindCycles(
            string current,
            Dictionary<string, List<string>> graph,
            List<string> path,
            HashSet<string> visiting,
            HashSet<string> cyclic)
        {
            if (visiting.Contains(current))
            {
                int start = path.FindIndex(id => string.Equals(id, current, StringComparison.OrdinalIgnoreCase));
                if (start >= 0) foreach (string id in path.Skip(start)) cyclic.Add(id);
                return;
            }
            visiting.Add(current);
            path.Add(current);
            if (graph.TryGetValue(current, out List<string> dependencies))
                foreach (string dependency in dependencies) FindCycles(dependency, graph, path, visiting, cyclic);
            path.RemoveAt(path.Count - 1);
            visiting.Remove(current);
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

        public static TrainingCatalogItem AddToolItem(
            TrainingCatalogSettings settings,
            string trainingId,
            string displayName,
            string categoryId)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Items ??= new ObservableCollection<TrainingCatalogItem>();
            string normalizedId = (trainingId ?? string.Empty).Trim();
            if (!IsValidTrainingId(normalizedId))
                throw new InvalidOperationException("TrainingIdは半角英字で始まり、半角英数字とアンダースコアだけを使用してください。");
            if (settings.Items.Any(item => string.Equals(
                item?.TrainingId, normalizedId, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"TrainingId '{normalizedId}' はすでに登録されています。");

            var item = new TrainingCatalogItem
            {
                TrainingId = normalizedId,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? normalizedId : displayName.Trim(),
                TrainingCategoryId = (categoryId ?? string.Empty).Trim(),
                IsToolCreated = true,
                SortOrder = settings.Items.Count == 0 ? 0 : settings.Items.Max(value => value?.SortOrder ?? 0) + 1
            };
            settings.Items.Add(item);
            return item;
        }

        public static bool RemoveToolItem(TrainingCatalogSettings settings, TrainingCatalogItem item)
        {
            if (settings?.Items == null || item?.IsToolCreated != true) return false;
            return settings.Items.Remove(item);
        }

        private static bool IsValidTrainingId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !IsAsciiLetter(value[0])) return false;
            return value.All(character => IsAsciiLetter(character) ||
                (character >= '0' && character <= '9') || character == '_');
        }

        private static bool IsAsciiLetter(char value) =>
            (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');
    }
}
