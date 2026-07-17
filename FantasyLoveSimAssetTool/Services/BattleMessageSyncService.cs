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

        public static BattleResultEventsDataFile DeserializeResultEvents(string json) =>
            Validate(JsonSerializer.Deserialize<BattleResultEventsDataFile>(json, Options));
        public static BattlePanelResultMessagesDataFile DeserializePanelMessages(string json) =>
            Validate(JsonSerializer.Deserialize<BattlePanelResultMessagesDataFile>(json, Options));

        public static void ApplyResultEvents(HeroineProfile profile, BattleResultEventsDataFile data)
        {
            ValidateHeroine(profile, data?.HeroineId);
            profile.BattleMessages ??= new BattleMessageSettings();
            if (data.Items != null) profile.BattleMessages.ResultEvents = NormalizeEvents(data.Items);
        }

        public static void ApplyPanelMessages(HeroineProfile profile, BattlePanelResultMessagesDataFile data)
        {
            ValidateHeroine(profile, data?.HeroineId);
            profile.BattleMessages ??= new BattleMessageSettings();
            if (data.Items != null) profile.BattleMessages.PanelMessages = NormalizePanelMessages(data.Items);
        }

        public static void Normalize(HeroineProfile profile)
        {
            profile.BattleMessages ??= new BattleMessageSettings();
            profile.BattleMessages.ResultEvents = NormalizeEvents(profile.BattleMessages.ResultEvents);
            profile.BattleMessages.PanelMessages = NormalizePanelMessages(profile.BattleMessages.PanelMessages);
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
                item.StillId = item.StillId?.Trim() ?? string.Empty;
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
                result.Add(item);
            }
            return new ObservableCollection<BattlePanelResultMessageEntry>(result);
        }

        private static string JoinIds(string value) => string.Join(", ", (value ?? string.Empty).Split(',')
            .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase));
        private static string BuildId(string type, string context) => string.IsNullOrWhiteSpace(context) ? type : type + "_" + context;
        private static void ValidateHeroine(HeroineProfile profile, string heroineId)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!string.IsNullOrWhiteSpace(heroineId) && !string.Equals(profile.HeroineId, heroineId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("HeroineId が選択中のキャラクターと一致しません。");
        }
        private static T Validate<T>(T data) where T : class
        {
            if (data == null) throw new InvalidOperationException("戦闘メッセージJSONを読み込めませんでした。");
            int version = data is BattleResultEventsDataFile events ? events.SchemaVersion : ((BattlePanelResultMessagesDataFile)(object)data).SchemaVersion;
            if (version != 1) throw new InvalidOperationException($"未対応の schemaVersion です: {version}");
            return data;
        }
    }
}
