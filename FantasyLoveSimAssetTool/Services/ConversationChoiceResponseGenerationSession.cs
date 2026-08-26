using FantasyLoveSimAssetTool.Models;
using System;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationChoiceResponseGenerationSession
    {
        private readonly ConversationEntry sourceEntry;
        private readonly ConversationChoice sourceChoice;
        private readonly string sourceChoiceText;

        public ConversationChoiceResponseGenerationSession(ConversationEntry entry, ConversationChoice choice)
        {
            sourceEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            sourceChoice = choice ?? throw new ArgumentNullException(nameof(choice));
            sourceChoiceText = choice.ChoiceText ?? string.Empty;
        }

        public bool IsCurrent(ConversationEntry entry, ConversationChoice choice) =>
            ReferenceEquals(sourceEntry, entry) && ReferenceEquals(sourceChoice, choice) &&
            string.Equals(sourceChoiceText, choice?.ChoiceText ?? string.Empty, StringComparison.Ordinal);

        public bool TryAdopt(ConversationEntry entry, ConversationChoice choice, string text)
        {
            if (!IsCurrent(entry, choice) || string.IsNullOrWhiteSpace(text)) return false;
            sourceChoice.ResponseText = text.Trim();
            return true;
        }
    }
}
