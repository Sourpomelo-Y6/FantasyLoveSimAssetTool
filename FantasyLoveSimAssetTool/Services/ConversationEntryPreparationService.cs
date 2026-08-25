using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class ConversationEntryPreparationService
    {
        public static ConversationEntry Create(
            ConversationSituationPrompt situation,
            IEnumerable<ConversationEntry> existingEntries)
        {
            if (situation == null || string.IsNullOrWhiteSpace(situation.SituationId) ||
                string.IsNullOrWhiteSpace(situation.DisplayName))
                throw new InvalidOperationException("状況テンプレートを選択してください。");
            string category = NormalizeIdPart(situation.Category);
            HashSet<string> existingIds = new HashSet<string>(
                (existingEntries ?? Array.Empty<ConversationEntry>())
                    .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                    .Select(entry => entry.Id), StringComparer.OrdinalIgnoreCase);
            int number = 1;
            string id;
            do id = $"Conv_{category}_{number++:D2}";
            while (existingIds.Contains(id));

            var entry = new ConversationEntry
            {
                Kind = ConversationDataKind.Conversations,
                Id = id,
                Title = situation.DisplayName,
                Category = string.IsNullOrWhiteSpace(situation.Category) ? "General" : situation.Category.Trim(),
                Priority = 100
            };
            entry.Conditions.MaxAffection = 9999;
            entry.Lines.Add(new ConversationLine());
            return entry;
        }

        private static string NormalizeIdPart(string value)
        {
            char[] characters = (string.IsNullOrWhiteSpace(value) ? "General" : value.Trim())
                .Where(character => char.IsLetterOrDigit(character) || character == '_')
                .ToArray();
            return characters.Length == 0 ? "General" : new string(characters);
        }
    }
}
