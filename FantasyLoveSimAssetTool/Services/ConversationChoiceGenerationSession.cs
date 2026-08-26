using FantasyLoveSimAssetTool.Models;
using System;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationChoiceGenerationSession
    {
        private readonly ConversationEntry sourceEntry;
        private readonly ConversationChoice sourceChoice;

        public ConversationChoiceGenerationSession(ConversationEntry entry, ConversationChoice choice)
        {
            sourceEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            sourceChoice = choice ?? throw new ArgumentNullException(nameof(choice));
        }

        public bool IsCurrent(ConversationEntry entry, ConversationChoice choice) =>
            ReferenceEquals(sourceEntry, entry) && ReferenceEquals(sourceChoice, choice);

        public bool TryAdopt(ConversationEntry entry, ConversationChoice choice, string text)
        {
            if (!IsCurrent(entry, choice) || string.IsNullOrWhiteSpace(text)) return false;
            sourceChoice.ChoiceText = text.Trim();
            return true;
        }
    }
}
