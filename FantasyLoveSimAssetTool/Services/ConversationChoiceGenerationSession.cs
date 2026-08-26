using FantasyLoveSimAssetTool.Models;
using System;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationChoiceGenerationSession
    {
        private readonly ConversationEntry sourceEntry;

        public ConversationChoiceGenerationSession(ConversationEntry entry, ConversationChoice choice)
        {
            sourceEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            if (choice == null) throw new ArgumentNullException(nameof(choice));
        }

        public bool IsCurrent(ConversationEntry entry, ConversationChoice choice) =>
            ReferenceEquals(sourceEntry, entry) && choice != null &&
            sourceEntry.Choices != null && sourceEntry.Choices.Contains(choice);

        public bool TryAdopt(ConversationEntry entry, ConversationChoice choice, string text)
        {
            if (!IsCurrent(entry, choice) || string.IsNullOrWhiteSpace(text)) return false;
            choice.ChoiceText = text.Trim();
            return true;
        }
    }
}
