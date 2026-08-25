using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Services
{
    public static class ConversationSituationConditionService
    {
        public static bool Apply(ConversationEntry entry, ConversationSituationPrompt situation)
        {
            ConversationSituationConditionSuggestion suggestion = situation?.SuggestedConditions;
            if (entry == null || suggestion == null) return false;
            Validate(suggestion);
            entry.Conditions ??= new ConversationCondition();
            if (!string.IsNullOrWhiteSpace(suggestion.LocationId)) entry.Conditions.LocationId = suggestion.LocationId.Trim();
            if (!string.IsNullOrWhiteSpace(suggestion.Weather)) entry.Conditions.Weather = suggestion.Weather.Trim();
            if (!string.IsNullOrWhiteSpace(suggestion.Season)) entry.Conditions.Season = suggestion.Season.Trim();
            if (!string.IsNullOrWhiteSpace(suggestion.TimeOfDay)) entry.Conditions.TimeOfDay = suggestion.TimeOfDay.Trim();
            if (suggestion.MinAffection.HasValue) entry.Conditions.MinAffection = suggestion.MinAffection.Value;
            if (suggestion.MaxAffection.HasValue) entry.Conditions.MaxAffection = suggestion.MaxAffection.Value;
            if (suggestion.Priority.HasValue) entry.Priority = suggestion.Priority.Value;
            if (suggestion.Once.HasValue) entry.Conditions.Once = suggestion.Once.Value;
            return true;
        }

        public static string BuildSummary(ConversationSituationPrompt situation)
        {
            ConversationSituationConditionSuggestion value = situation?.SuggestedConditions;
            if (value == null) return "このテンプレートには推奨条件がありません。必要な条件を手動で設定してください。";
            var parts = new List<string>();
            Add(parts, "場所", value.LocationId);
            Add(parts, "天候", value.Weather);
            Add(parts, "季節", value.Season);
            Add(parts, "時間", value.TimeOfDay);
            if (value.MinAffection.HasValue || value.MaxAffection.HasValue)
                parts.Add($"好感度 {value.MinAffection?.ToString() ?? "現在値"}～{value.MaxAffection?.ToString() ?? "現在値"}");
            if (value.Priority.HasValue) parts.Add("優先度 " + value.Priority.Value);
            if (value.Once.HasValue) parts.Add(value.Once.Value ? "一度だけ" : "繰り返し可");
            string summary = parts.Count == 0 ? "設定値なし" : string.Join(" / ", parts);
            return string.IsNullOrWhiteSpace(value.Note) ? summary : summary + Environment.NewLine + value.Note.Trim();
        }

        private static void Validate(ConversationSituationConditionSuggestion value)
        {
            if (value.MinAffection < 0 || value.MinAffection > 9999 ||
                value.MaxAffection < 0 || value.MaxAffection > 9999 ||
                (value.MinAffection.HasValue && value.MaxAffection.HasValue && value.MinAffection > value.MaxAffection))
                throw new InvalidOperationException("状況テンプレートの推奨好感度範囲が不正です。");
            if (value.Priority < 0) throw new InvalidOperationException("状況テンプレートの推奨優先度が不正です。");
        }

        private static void Add(List<string> parts, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) parts.Add(label + " " + value.Trim());
        }
    }
}
