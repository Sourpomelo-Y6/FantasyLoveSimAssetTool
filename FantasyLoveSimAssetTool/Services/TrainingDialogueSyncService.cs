using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class TrainingDialogueMergeResult
    {
        public int AddedEntryCount { get; internal set; }
        public int AddedMessageCount { get; internal set; }
        public int UpdatedVoiceIdCount { get; internal set; }
        public int SkippedCount { get; internal set; }
    }

    public static class TrainingDialogueSyncService
    {
        public static FromUnityTrainingDialogueDataFile DeserializeFromUnity(string json)
        {
            FromUnityTrainingDialogueDataFile data = JsonSerializer.Deserialize<FromUnityTrainingDialogueDataFile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data == null)
                throw new InvalidOperationException("training_dialogues_from_unity.json を読み込めませんでした。");
            return data;
        }

        public static TrainingDialogueMergeResult MergeFromUnity(TrainingDialogueSettings settings, string heroineId, FromUnityTrainingDialogueDataFile data)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.SchemaVersion != 1)
                throw new InvalidOperationException($"未対応の schemaVersion です: {data.SchemaVersion}");
            if (!string.IsNullOrWhiteSpace(data.HeroineId) &&
                !string.Equals(data.HeroineId, heroineId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"HeroineId が選択中のキャラクターと一致しません。JSON: {data.HeroineId} / Selected: {heroineId}");

            settings.Items ??= new ObservableCollection<TrainingDialogueEntry>();
            TrainingDialogueMergeResult result = new TrainingDialogueMergeResult();
            foreach (FromUnityTrainingDialogueItem item in data.Items ?? new List<FromUnityTrainingDialogueItem>())
            {
                string trainingId = (item?.TrainingId ?? string.Empty).Trim();
                string visualState = NormalizeVisualState(item?.VisualState);
                if (item == null || trainingId.Length == 0 || visualState.Length == 0)
                {
                    result.SkippedCount++;
                    continue;
                }

                TrainingDialogueEntry entry = settings.Items.FirstOrDefault(existing =>
                    existing != null &&
                    string.Equals((existing.TrainingId ?? string.Empty).Trim(), trainingId, StringComparison.Ordinal) &&
                    string.Equals(NormalizeVisualState(existing.VisualState), visualState, StringComparison.Ordinal));
                if (entry == null)
                {
                    entry = new TrainingDialogueEntry { TrainingId = trainingId, VisualState = visualState };
                    settings.Items.Add(entry);
                    result.AddedEntryCount++;
                }
                else
                {
                    entry.TrainingId = trainingId;
                    entry.VisualState = visualState;
                }

                entry.Messages ??= new ObservableCollection<TrainingDialogueMessage>();
                foreach (string sourceMessage in item.Messages ?? new List<string>())
                {
                    string message = (sourceMessage ?? string.Empty).Trim();
                    if (message.Length == 0)
                    {
                        result.SkippedCount++;
                        continue;
                    }
                    if (entry.Messages.Any(existing =>
                        existing != null &&
                        string.Equals(existing.Text?.Trim(), message, StringComparison.Ordinal)))
                    {
                        // 旧JSONの本文だけでは、既存のVoice IDを消さない。
                        result.SkippedCount++;
                        continue;
                    }
                    entry.Messages.Add(new TrainingDialogueMessage { Text = message });
                    result.AddedMessageCount++;
                }
                foreach (FromUnityTrainingDialogueVoiceItem sourceCandidate in
                    item.VoicedMessages ?? new List<FromUnityTrainingDialogueVoiceItem>())
                {
                    string message = (sourceCandidate?.Message ?? string.Empty).Trim();
                    string voiceId = (sourceCandidate?.VoiceId ?? string.Empty).Trim();
                    if (message.Length == 0)
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    TrainingDialogueMessage existing = entry.Messages.FirstOrDefault(value =>
                        value != null &&
                        string.Equals(value.Text?.Trim(), message, StringComparison.Ordinal));
                    if (existing == null)
                    {
                        entry.Messages.Add(new TrainingDialogueMessage
                        {
                            Text = message,
                            VoiceId = voiceId
                        });
                        result.AddedMessageCount++;
                    }
                    else if (!string.Equals(
                        (existing.VoiceId ?? string.Empty).Trim(),
                        voiceId,
                        StringComparison.Ordinal))
                    {
                        existing.VoiceId = voiceId;
                        result.UpdatedVoiceIdCount++;
                    }
                    else
                    {
                        result.SkippedCount++;
                    }
                }
            }
            return result;
        }

        public static string BuildExportJson(HeroineProfile profile, ExportReport report)
        {
            TrainingDialogueSettings settings = profile.TrainingDialogues ?? new TrainingDialogueSettings();
            List<TrainingDialogueEntry> items = (settings.Items ?? new ObservableCollection<TrainingDialogueEntry>()).Where(item => item != null).ToList();
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (TrainingDialogueEntry item in items)
            {
                string key = (item.TrainingId ?? string.Empty) + "\n" + (item.VisualState ?? string.Empty);
                if (string.IsNullOrWhiteSpace(item.VisualState)) report.Warnings.Add("訓練セリフにvisualStateが空の項目があります。");
                else if (!keys.Add(key)) report.Warnings.Add($"訓練セリフが重複しています: {item.TrainingId} / {item.VisualState}");
                if (item.Messages == null || !item.Messages.Any(message => message != null && !string.IsNullOrWhiteSpace(message.Text)))
                    report.Warnings.Add($"訓練セリフが空です: {item.TrainingId} / {item.VisualState}");
                foreach (IGrouping<string, TrainingDialogueMessage> duplicate in
                    (item.Messages ?? new ObservableCollection<TrainingDialogueMessage>())
                        .Where(message => message != null && !string.IsNullOrWhiteSpace(message.Text))
                        .GroupBy(message => message.Text.Trim(), StringComparer.Ordinal)
                        .Where(group => group.Select(message =>
                            (message.VoiceId ?? string.Empty).Trim())
                            .Distinct(StringComparer.Ordinal).Count() > 1))
                {
                    report.Warnings.Add(
                        $"同じ訓練セリフ本文に異なるVoice IDがあります: {item.TrainingId} / {item.VisualState} / {duplicate.Key}");
                }
            }
            object exportModel = new
            {
                schemaVersion = 1,
                heroineId = profile.HeroineId,
                items = items.Select(item => new
                {
                    trainingId = item.TrainingId,
                    visualState = item.VisualState,
                    messages = NormalizeMessagesForExport(item.Messages)
                        .Where(message => string.IsNullOrEmpty(message.VoiceId))
                        .Select(message => message.Text).ToList(),
                    voicedMessages = NormalizeMessagesForExport(item.Messages)
                        .Where(message => !string.IsNullOrEmpty(message.VoiceId))
                        .Select(message => new
                        {
                            message = message.Text,
                            voiceId = message.VoiceId
                        }).ToList()
                }).ToList()
            };
            return JsonSerializer.Serialize(exportModel, new JsonSerializerOptions { WriteIndented = true });
        }

        public static string NormalizeVisualState(string visualState)
        {
            switch ((visualState ?? string.Empty).Trim())
            {
                case "BeforeFirstStep": return "SelectedBeforeFirstStep";
                case "AfterFirstStep": return "SelectedAfterFirstStep";
                case "SelectedBeforeFirstStep": return "SelectedBeforeFirstStep";
                case "SelectedAfterFirstStep": return "SelectedAfterFirstStep";
                case "PlayerLpConsumed": return "PlayerLpConsumed";
                case "HeroineLpConsumed": return "HeroineLpConsumed";
                case "SimultaneousLpConsumed": return "SimultaneousLpConsumed";
                default: return string.Empty;
            }
        }

        private static List<TrainingDialogueMessage> NormalizeMessagesForExport(
            IEnumerable<TrainingDialogueMessage> source)
        {
            List<TrainingDialogueMessage> result = new List<TrainingDialogueMessage>();
            foreach (TrainingDialogueMessage message in source ??
                Enumerable.Empty<TrainingDialogueMessage>())
            {
                string text = (message?.Text ?? string.Empty).Trim();
                string voiceId = (message?.VoiceId ?? string.Empty).Trim();
                if (text.Length == 0)
                {
                    continue;
                }

                TrainingDialogueMessage existing = result.FirstOrDefault(value =>
                    string.Equals(value.Text, text, StringComparison.Ordinal));
                if (existing == null)
                {
                    result.Add(new TrainingDialogueMessage
                    {
                        Text = text,
                        VoiceId = voiceId
                    });
                }
                else if (string.IsNullOrEmpty(existing.VoiceId) &&
                    !string.IsNullOrEmpty(voiceId))
                {
                    existing.VoiceId = voiceId;
                }
            }
            return result;
        }
    }
}
