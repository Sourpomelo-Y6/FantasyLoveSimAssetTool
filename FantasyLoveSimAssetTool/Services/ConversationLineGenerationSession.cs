using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationLineGenerationSession
    {
        private readonly ConversationEntry sourceEntry;
        private readonly ConversationLine sourceLine;

        public ConversationLineGenerationSession(ConversationEntry entry, ConversationLine line)
        {
            sourceEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            sourceLine = line ?? throw new ArgumentNullException(nameof(line));
        }

        public bool IsCurrent(ConversationEntry entry, ConversationLine line) =>
            ReferenceEquals(sourceEntry, entry) && ReferenceEquals(sourceLine, line);

        public bool TryAdopt(ConversationEntry entry, ConversationLine line, string text)
        {
            if (!IsCurrent(entry, line) || string.IsNullOrWhiteSpace(text)) return false;
            sourceLine.Text = text.Trim();
            return true;
        }

        public static ShortTextGenerationContext CreateContext(ConversationEntry entry, ConversationLine line)
        {
            if (entry == null || line == null) return new ShortTextGenerationContext();
            return new ShortTextGenerationContext
            {
                ConversationKind = entry.Kind.ToString(),
                ConversationEntryId = entry.Id ?? string.Empty,
                ConversationCategory = entry.Category ?? string.Empty,
                ConversationSpeaker = line.Speaker ?? string.Empty,
                PreviousConversationLines = BuildPreviousLines(entry, line),
                ConversationConditions = BuildConditionSummary(entry.Conditions)
            };
        }

        private static string BuildPreviousLines(ConversationEntry entry, ConversationLine selectedLine)
        {
            if (entry.Lines == null) return string.Empty;
            int selectedIndex = entry.Lines.IndexOf(selectedLine);
            if (selectedIndex <= 0) return string.Empty;
            return string.Join(" / ", entry.Lines
                .Skip(Math.Max(0, selectedIndex - 2))
                .Take(Math.Min(2, selectedIndex))
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.Text))
                .Select(item => $"{item.Speaker}: {item.Text}"));
        }

        private static string BuildConditionSummary(ConversationCondition conditions)
        {
            if (conditions == null) return string.Empty;
            var values = new List<string>();
            AddCondition(values, "場所", conditions.LocationId);
            AddCondition(values, "時間", conditions.TimeOfDay);
            AddCondition(values, "天候", conditions.Weather);
            AddCondition(values, "季節", conditions.Season);
            AddCondition(values, "衣装", conditions.CostumeId);
            AddCondition(values, "行動", conditions.ActionId);
            return string.Join("; ", values);
        }

        private static void AddCondition(ICollection<string> values, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) values.Add($"{label}={value.Trim()}");
        }
    }
}
