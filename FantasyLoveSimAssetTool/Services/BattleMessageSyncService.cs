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

        private static ObservableCollection<BattleResultEventEntry> NormalizeEvents(IEnumerable<BattleResultEventEntry> source)
        {
            List<BattleResultEventEntry> result = new List<BattleResultEventEntry>();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (BattleResultEventEntry item in source ?? Enumerable.Empty<BattleResultEventEntry>())
            {
                if (item == null) continue;
                item.ResultType = string.IsNullOrWhiteSpace(item.ResultType) ? "SoloVictory" : item.ResultType.Trim();
                item.BattleContextId = item.BattleContextId?.Trim() ?? string.Empty;
                item.EventId = string.IsNullOrWhiteSpace(item.EventId)
                    ? BuildId(item.ResultType, item.BattleContextId)
                    : item.EventId.Trim();
                if (!ids.Add(item.EventId)) continue;
                item.Message ??= string.Empty;
                item.StillId = item.StillId?.Trim() ?? string.Empty;
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
