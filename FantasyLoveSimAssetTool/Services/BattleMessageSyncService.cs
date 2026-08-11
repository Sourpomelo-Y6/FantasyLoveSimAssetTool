using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public static class BattleMessageSyncService
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string BuildResultEventsJson(HeroineProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            Normalize(profile);
            return JsonSerializer.Serialize(new BattleResultEventsDataFile
            {
                HeroineId = profile.HeroineId,
                Items = profile.BattleMessages.ResultEvents.ToArray()
            }, Options);
        }

        public static string BuildPanelMessagesJson(HeroineProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            Normalize(profile);
            return JsonSerializer.Serialize(new BattlePanelResultMessagesDataFile
            {
                HeroineId = profile.HeroineId,
                Items = profile.BattleMessages.PanelMessages.ToArray()
            }, Options);
        }

        public static string BuildSoloReturnReactionsJson(HeroineProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            Normalize(profile);
            return JsonSerializer.Serialize(new SoloReturnReactionsDataFile
            {
                HeroineId = profile.HeroineId,
                Items = profile.BattleMessages.SoloReturnReactions.ToArray()
            }, Options);
        }

        public static BattleResultEventsDataFile DeserializeResultEvents(string json) =>
            Validate(JsonSerializer.Deserialize<BattleResultEventsDataFile>(json, Options));
        public static BattlePanelResultMessagesDataFile DeserializePanelMessages(string json) =>
            Validate(JsonSerializer.Deserialize<BattlePanelResultMessagesDataFile>(json, Options));
        public static SoloReturnReactionsDataFile DeserializeSoloReturnReactions(string json) =>
            Validate(JsonSerializer.Deserialize<SoloReturnReactionsDataFile>(json, Options));

        public static void ApplyResultEvents(HeroineProfile profile, BattleResultEventsDataFile data)
        {
            ValidateHeroine(profile, data?.HeroineId);
            profile.BattleMessages ??= new BattleMessageSettings();
            if (data.Items != null)
            {
                PreserveMissingResultVoiceIds(
                    profile.BattleMessages.ResultEvents,
                    data.Items);
                profile.BattleMessages.ResultEvents = NormalizeEvents(data.Items);
            }
        }

        public static void ApplyPanelMessages(HeroineProfile profile, BattlePanelResultMessagesDataFile data)
        {
            ValidateHeroine(profile, data?.HeroineId);
            profile.BattleMessages ??= new BattleMessageSettings();
            if (data.Items != null)
            {
                PreserveMissingPanelVoiceIds(
                    profile.BattleMessages.PanelMessages,
                    data.Items);
                profile.BattleMessages.PanelMessages = NormalizePanelMessages(data.Items);
            }
        }

        public static void ApplySoloReturnReactions(HeroineProfile profile, SoloReturnReactionsDataFile data)
        {
            ValidateHeroine(profile, data?.HeroineId);
            profile.BattleMessages ??= new BattleMessageSettings();
            // Items省略は旧データとして扱い、現在の設定を維持する。
            if (data.Items != null)
            {
                PreserveMissingSoloReturnVoiceIds(
                    profile.BattleMessages.SoloReturnReactions,
                    data.Items);
                profile.BattleMessages.SoloReturnReactions = NormalizeSoloReturnReactions(data.Items);
            }
        }

        public static void Normalize(HeroineProfile profile)
        {
            profile.BattleMessages ??= new BattleMessageSettings();
            profile.BattleMessages.ResultEvents = NormalizeEvents(profile.BattleMessages.ResultEvents);
            profile.BattleMessages.PanelMessages = NormalizePanelMessages(profile.BattleMessages.PanelMessages);
            profile.BattleMessages.SoloReturnReactions = NormalizeSoloReturnReactions(
                profile.BattleMessages.SoloReturnReactions);
        }

        public static BattleMessageChangeSummary AnalyzeChanges(
            IEnumerable<BattleResultEventEntry> beforeEvents,
            IEnumerable<BattleResultEventEntry> afterEvents,
            IEnumerable<BattlePanelResultMessageEntry> beforePanelMessages,
            IEnumerable<BattlePanelResultMessageEntry> afterPanelMessages)
        {
            BattleMessageChangeSummary summary = new BattleMessageChangeSummary();
            Dictionary<string, BattleResultEventEntry> beforeResultMap = ToMap(beforeEvents, x => x.EventId);
            Dictionary<string, BattleResultEventEntry> afterResultMap = ToMap(afterEvents, x => x.EventId);
            summary.ResultAdded = afterResultMap.Keys.Count(x => !beforeResultMap.ContainsKey(x));
            summary.ResultDeleted = beforeResultMap.Keys.Count(x => !afterResultMap.ContainsKey(x));
            foreach (string id in beforeResultMap.Keys.Where(afterResultMap.ContainsKey))
            {
                BattleResultEventEntry before = beforeResultMap[id];
                BattleResultEventEntry after = afterResultMap[id];
                if (ResultEventEquals(before, after)) summary.ResultUnchanged++;
                else summary.ResultUpdated++;
                if (!Same(before.SpeakerType, after.SpeakerType) || !Same(before.SpeakerName, after.SpeakerName)) summary.SpeakerChanged++;
                if (!Same(before.ExpressionId, after.ExpressionId)) summary.ExpressionChanged++;
                if (!Same(before.VisualMode, after.VisualMode)) summary.VisualModeChanged++;
            }

            Dictionary<string, BattlePanelResultMessageEntry> beforePanelMap = ToMap(beforePanelMessages, x => x.MessageId);
            Dictionary<string, BattlePanelResultMessageEntry> afterPanelMap = ToMap(afterPanelMessages, x => x.MessageId);
            summary.PanelAdded = afterPanelMap.Keys.Count(x => !beforePanelMap.ContainsKey(x));
            summary.PanelDeleted = beforePanelMap.Keys.Count(x => !afterPanelMap.ContainsKey(x));
            foreach (string id in beforePanelMap.Keys.Where(afterPanelMap.ContainsKey))
            {
                BattlePanelResultMessageEntry before = beforePanelMap[id];
                BattlePanelResultMessageEntry after = afterPanelMap[id];
                if (Same(before.ResultType, after.ResultType) &&
                    Same(before.Message, after.Message) &&
                    Same(before.VoiceId, after.VoiceId)) summary.PanelUnchanged++;
                else summary.PanelUpdated++;
            }

            return summary;
        }

        public static IReadOnlyList<string> Validate(
            HeroineProfile profile,
            IEnumerable<string> knownStillIds,
            IEnumerable<string> knownOutfitIds,
            IEnumerable<string> knownExpressionIds)
        {
            List<string> messages = new List<string>();
            BattleMessageSettings settings = profile?.BattleMessages ?? new BattleMessageSettings();
            HashSet<string> resultTypes = new HashSet<string>(new[]
            { "SoloVictory", "DuoVictory", "SoloDefeat", "DuoDefeat", "SoloEscape", "DuoEscape" }, StringComparer.Ordinal);
            HashSet<string> panelTypes = new HashSet<string>(new[] { "Victory", "Defeat", "Escape", "Default" }, StringComparer.Ordinal);
            HashSet<string> stillIds = new HashSet<string>(knownStillIds ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            HashSet<string> outfitIds = new HashSet<string>(knownOutfitIds ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            HashSet<string> expressionIds = new HashSet<string>(knownExpressionIds ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            HashSet<string> speakerTypes = new HashSet<string>(new[] { "Heroine", "System", "Schedule", "Outfit", "Player" }, StringComparer.Ordinal);
            HashSet<string> visualModes = new HashSet<string>(new[] { "Auto", "StillOnly", "StillWithPortrait", "PortraitOnly" }, StringComparer.Ordinal);
            List<BattleResultEventEntry> events = (settings.ResultEvents ?? new ObservableCollection<BattleResultEventEntry>()).Where(x => x != null).ToList();
            foreach (IGrouping<string, BattleResultEventEntry> group in events.Where(x => !string.IsNullOrWhiteSpace(x.EventId)).GroupBy(x => x.EventId.Trim(), StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
                messages.Add($"[Error] 戦闘結果: EventId `{group.Key}` が重複しています。");
            foreach (IGrouping<string, BattleResultEventEntry> group in events.GroupBy(x => (x.ResultType?.Trim() ?? "") + "|" + (x.BattleContextId?.Trim() ?? ""), StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
                messages.Add($"[Error] 戦闘結果: resultType + battleContextId `{group.Key}` が重複しています。");
            foreach (BattleResultEventEntry item in events)
            {
                string label = string.IsNullOrWhiteSpace(item.EventId) ? "EventId未設定" : item.EventId.Trim();
                if (string.IsNullOrWhiteSpace(item.EventId)) messages.Add("[Error] 戦闘結果: EventId が空です。");
                if (!resultTypes.Contains(item.ResultType?.Trim() ?? "")) messages.Add($"[Error] 戦闘結果 `{label}`: resultType `{item.ResultType}` は候補外です。");
                if (!speakerTypes.Contains(item.SpeakerType?.Trim() ?? "")) messages.Add($"[Error] 戦闘結果 `{label}`: speakerType `{item.SpeakerType}` は候補外です。");
                if (!visualModes.Contains(item.VisualMode?.Trim() ?? "")) messages.Add($"[Error] 戦闘結果 `{label}`: visualMode `{item.VisualMode}` は候補外です。");
                if (string.IsNullOrWhiteSpace(item.Message)) messages.Add($"[Error] 戦闘結果 `{label}`: message が空です。");
                if (!string.IsNullOrWhiteSpace(item.StillId) && !stillIds.Contains(item.StillId.Trim())) messages.Add($"[Warning] 戦闘結果 `{label}`: stillId `{item.StillId}` は登録済み候補にありません。");
                if (!string.IsNullOrWhiteSpace(item.ExpressionId) && !expressionIds.Contains(item.ExpressionId.Trim())) messages.Add($"[Warning] 戦闘結果 `{label}`: expressionId `{item.ExpressionId}` は登録済み候補にありません。");
                foreach (string outfitId in item.UnlockedOutfitIds ?? new string[0])
                    if (!string.IsNullOrWhiteSpace(outfitId) && !outfitIds.Contains(outfitId.Trim())) messages.Add($"[Warning] 戦闘結果 `{label}`: outfitId `{outfitId}` は登録済み候補にありません。");
            }
            List<BattlePanelResultMessageEntry> panels = (settings.PanelMessages ?? new ObservableCollection<BattlePanelResultMessageEntry>()).Where(x => x != null).ToList();
            foreach (IGrouping<string, BattlePanelResultMessageEntry> group in panels.GroupBy(x => x.ResultType?.Trim() ?? "", StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
                messages.Add($"[Error] 戦闘パネル: resultType `{group.Key}` が重複しています。");
            foreach (BattlePanelResultMessageEntry item in panels)
            {
                string label = string.IsNullOrWhiteSpace(item.MessageId) ? "MessageId未設定" : item.MessageId.Trim();
                if (string.IsNullOrWhiteSpace(item.MessageId)) messages.Add("[Error] 戦闘パネル: MessageId が空です。");
                if (!panelTypes.Contains(item.ResultType?.Trim() ?? "")) messages.Add($"[Error] 戦闘パネル `{label}`: resultType `{item.ResultType}` は候補外です。");
                if (string.IsNullOrWhiteSpace(item.Message)) messages.Add($"[Error] 戦闘パネル `{label}`: message が空です。");
            }
            List<SoloReturnReactionEntry> returns = (settings.SoloReturnReactions ??
                new ObservableCollection<SoloReturnReactionEntry>()).Where(x => x != null).ToList();
            foreach (IGrouping<string, SoloReturnReactionEntry> group in returns.GroupBy(
                x => (x.ResultType?.Trim() ?? "") + "|" + (x.BattleContextId?.Trim() ?? ""),
                StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
                messages.Add($"[Error] 帰還後反応: resultType + battleContextId `{group.Key}` が重複しています。");
            foreach (SoloReturnReactionEntry item in returns)
            {
                string label = string.IsNullOrWhiteSpace(item.ReactionId) ? "ReactionId未設定" : item.ReactionId.Trim();
                if (string.IsNullOrWhiteSpace(item.ReactionId)) messages.Add("[Error] 帰還後反応: ReactionId が空です。");
                if (!new[] { "SoloVictory", "SoloDefeat", "SoloEscape" }.Contains(item.ResultType?.Trim() ?? ""))
                    messages.Add($"[Error] 帰還後反応 `{label}`: resultType `{item.ResultType}` は候補外です。");
                if (string.IsNullOrWhiteSpace(item.Message)) messages.Add($"[Error] 帰還後反応 `{label}`: message が空です。");
                if (!visualModes.Contains(item.VisualMode?.Trim() ?? "")) messages.Add($"[Error] 帰還後反応 `{label}`: visualMode `{item.VisualMode}` は候補外です。");
                if (!string.IsNullOrWhiteSpace(item.StillId) && !stillIds.Contains(item.StillId.Trim())) messages.Add($"[Warning] 帰還後反応 `{label}`: stillId `{item.StillId}` は登録済み候補にありません。");
                if (!string.IsNullOrWhiteSpace(item.ExpressionId) && !expressionIds.Contains(item.ExpressionId.Trim())) messages.Add($"[Warning] 帰還後反応 `{label}`: expressionId `{item.ExpressionId}` は登録済み候補にありません。");
            }
            return messages;
        }

        private static ObservableCollection<BattleResultEventEntry> NormalizeEvents(IEnumerable<BattleResultEventEntry> source)
        {
            List<BattleResultEventEntry> result = new List<BattleResultEventEntry>();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (BattleResultEventEntry item in source ?? Enumerable.Empty<BattleResultEventEntry>())
            {
                if (item == null) continue;
                item.ResultType = string.IsNullOrWhiteSpace(item.ResultType) ? "SoloVictory" : item.ResultType.Trim();
                item.BattleContextId = item.BattleContextId?.Trim() ?? string.Empty;
                item.SpeakerType = string.IsNullOrWhiteSpace(item.SpeakerType) ? "Heroine" : item.SpeakerType.Trim();
                item.SpeakerName = item.SpeakerName?.Trim() ?? string.Empty;
                item.EventId = string.IsNullOrWhiteSpace(item.EventId)
                    ? BuildId(item.ResultType, item.BattleContextId)
                    : item.EventId.Trim();
                if (!ids.Add(item.EventId)) continue;
                item.Message ??= string.Empty;
                item.VoiceId = item.VoiceId?.Trim() ?? string.Empty;
                item.StillId = item.StillId?.Trim() ?? string.Empty;
                item.VisualMode = string.IsNullOrWhiteSpace(item.VisualMode) ? "Auto" : item.VisualMode.Trim();
                item.ExpressionId = item.ExpressionId?.Trim() ?? string.Empty;
                item.UnlockedOutfitIdsText = JoinIds(item.UnlockedOutfitIdsText);
                result.Add(item);
            }
            return new ObservableCollection<BattleResultEventEntry>(result);
        }

        private static ObservableCollection<BattlePanelResultMessageEntry> NormalizePanelMessages(IEnumerable<BattlePanelResultMessageEntry> source)
        {
            List<BattlePanelResultMessageEntry> result = new List<BattlePanelResultMessageEntry>();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (BattlePanelResultMessageEntry item in source ?? Enumerable.Empty<BattlePanelResultMessageEntry>())
            {
                if (item == null) continue;
                item.ResultType = string.IsNullOrWhiteSpace(item.ResultType) ? "Default" : item.ResultType.Trim();
                item.MessageId = string.IsNullOrWhiteSpace(item.MessageId) ? item.ResultType : item.MessageId.Trim();
                if (!ids.Add(item.MessageId)) continue;
                item.Message ??= string.Empty;
                item.VoiceId = item.VoiceId?.Trim() ?? string.Empty;
                result.Add(item);
            }
            return new ObservableCollection<BattlePanelResultMessageEntry>(result);
        }

        private static ObservableCollection<SoloReturnReactionEntry> NormalizeSoloReturnReactions(
            IEnumerable<SoloReturnReactionEntry> source)
        {
            List<SoloReturnReactionEntry> result = new List<SoloReturnReactionEntry>();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (SoloReturnReactionEntry item in source ?? Enumerable.Empty<SoloReturnReactionEntry>())
            {
                if (item == null) continue;
                item.ResultType = string.IsNullOrWhiteSpace(item.ResultType) ? "SoloVictory" : item.ResultType.Trim();
                item.BattleContextId = item.BattleContextId?.Trim() ?? string.Empty;
                item.ReactionId = string.IsNullOrWhiteSpace(item.ReactionId)
                    ? BuildId(item.ResultType, item.BattleContextId)
                    : item.ReactionId.Trim();
                if (!ids.Add(item.ReactionId)) continue;
                item.Message ??= string.Empty;
                item.VoiceId = item.VoiceId?.Trim() ?? string.Empty;
                item.StillId = item.StillId?.Trim() ?? string.Empty;
                item.VisualMode = string.IsNullOrWhiteSpace(item.VisualMode) ? "Auto" : item.VisualMode.Trim();
                item.ExpressionId = item.ExpressionId?.Trim() ?? string.Empty;
                result.Add(item);
            }
            return new ObservableCollection<SoloReturnReactionEntry>(result);
        }

        private static string JoinIds(string value) => string.Join(", ", (value ?? string.Empty).Split(',')
            .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase));
        private static string BuildId(string type, string context) => string.IsNullOrWhiteSpace(context) ? type : type + "_" + context;
        private static Dictionary<string, T> ToMap<T>(IEnumerable<T> source, Func<T, string> getId) where T : class =>
            (source ?? Enumerable.Empty<T>()).Where(x => x != null && !string.IsNullOrWhiteSpace(getId(x)))
                .GroupBy(x => getId(x).Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Last(), StringComparer.OrdinalIgnoreCase);
        private static bool Same(string left, string right) => string.Equals(left ?? string.Empty, right ?? string.Empty, StringComparison.Ordinal);
        private static bool ResultEventEquals(BattleResultEventEntry left, BattleResultEventEntry right) =>
            Same(left.ResultType, right.ResultType) && Same(left.BattleContextId, right.BattleContextId) &&
            Same(left.SpeakerType, right.SpeakerType) && Same(left.SpeakerName, right.SpeakerName) &&
            Same(left.Message, right.Message) && Same(left.VoiceId, right.VoiceId) &&
            Same(left.StillId, right.StillId) &&
            Same(left.VisualMode, right.VisualMode) && Same(left.ExpressionId, right.ExpressionId) &&
            left.AffectionChange == right.AffectionChange &&
            (left.UnlockedOutfitIds ?? Array.Empty<string>()).SequenceEqual(right.UnlockedOutfitIds ?? Array.Empty<string>(), StringComparer.Ordinal);

        private static void PreserveMissingResultVoiceIds(
            IEnumerable<BattleResultEventEntry> existing,
            IEnumerable<BattleResultEventEntry> incoming)
        {
            Dictionary<string, BattleResultEventEntry> existingById =
                ToMap(existing, item => item.EventId);
            foreach (BattleResultEventEntry item in incoming ??
                Enumerable.Empty<BattleResultEventEntry>())
            {
                if (item != null &&
                    item.VoiceId == null &&
                    !string.IsNullOrWhiteSpace(item.EventId) &&
                    existingById.TryGetValue(
                        item.EventId.Trim(),
                        out BattleResultEventEntry previous))
                {
                    item.VoiceId = previous.VoiceId ?? string.Empty;
                }
            }
        }

        private static void PreserveMissingPanelVoiceIds(
            IEnumerable<BattlePanelResultMessageEntry> existing,
            IEnumerable<BattlePanelResultMessageEntry> incoming)
        {
            Dictionary<string, BattlePanelResultMessageEntry> existingById =
                ToMap(existing, item => item.MessageId);
            foreach (BattlePanelResultMessageEntry item in incoming ??
                Enumerable.Empty<BattlePanelResultMessageEntry>())
            {
                if (item != null &&
                    item.VoiceId == null &&
                    !string.IsNullOrWhiteSpace(item.MessageId) &&
                    existingById.TryGetValue(
                        item.MessageId.Trim(),
                        out BattlePanelResultMessageEntry previous))
                {
                    item.VoiceId = previous.VoiceId ?? string.Empty;
                }
            }
        }
        private static void PreserveMissingSoloReturnVoiceIds(
            IEnumerable<SoloReturnReactionEntry> existing,
            IEnumerable<SoloReturnReactionEntry> incoming)
        {
            Dictionary<string, SoloReturnReactionEntry> existingById = ToMap(existing, item => item.ReactionId);
            foreach (SoloReturnReactionEntry item in incoming ?? Enumerable.Empty<SoloReturnReactionEntry>())
            {
                if (item != null && item.VoiceId == null && !string.IsNullOrWhiteSpace(item.ReactionId) &&
                    existingById.TryGetValue(item.ReactionId.Trim(), out SoloReturnReactionEntry previous))
                    item.VoiceId = previous.VoiceId ?? string.Empty;
            }
        }
        private static void ValidateHeroine(HeroineProfile profile, string heroineId)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!string.IsNullOrWhiteSpace(heroineId) && !string.Equals(profile.HeroineId, heroineId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("HeroineId が選択中のキャラクターと一致しません。");
        }
        private static T Validate<T>(T data) where T : class
        {
            if (data == null) throw new InvalidOperationException("戦闘メッセージJSONを読み込めませんでした。");
            int version = data is BattleResultEventsDataFile events ? events.SchemaVersion :
                data is BattlePanelResultMessagesDataFile panels ? panels.SchemaVersion :
                ((SoloReturnReactionsDataFile)(object)data).SchemaVersion;
            if (version != 1) throw new InvalidOperationException($"未対応の schemaVersion です: {version}");
            return data;
        }
    }

    public sealed class BattleMessageChangeSummary
    {
        public int ResultAdded { get; set; }
        public int ResultUpdated { get; set; }
        public int ResultDeleted { get; set; }
        public int ResultUnchanged { get; set; }
        public int PanelAdded { get; set; }
        public int PanelUpdated { get; set; }
        public int PanelDeleted { get; set; }
        public int PanelUnchanged { get; set; }
        public int SpeakerChanged { get; set; }
        public int ExpressionChanged { get; set; }
        public int VisualModeChanged { get; set; }
    }
}
