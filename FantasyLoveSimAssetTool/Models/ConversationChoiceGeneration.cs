using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class ConversationChoiceGenerationContext
    {
        public string CharacterPrompt { get; set; } = string.Empty;
        public string ConversationKind { get; set; } = string.Empty;
        public string ConversationEntryId { get; set; } = string.Empty;
        public string ConversationCategory { get; set; } = string.Empty;
        public string PreviousLine { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public IReadOnlyCollection<string> ExistingChoices { get; set; } = new List<string>();
    }

    public sealed class ConversationChoiceGenerationResult
    {
        public IReadOnlyList<string> Candidates { get; set; } = new List<string>();
        public string Prompt { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public string ParseError { get; set; } = string.Empty;
    }
}
