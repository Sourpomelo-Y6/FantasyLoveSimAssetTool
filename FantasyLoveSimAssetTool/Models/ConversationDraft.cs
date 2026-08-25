using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class ConversationDraftLine
    {
        public string Speaker { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string ExpressionId { get; set; } = string.Empty;
    }

    public sealed class ConversationDraftGenerationContext
    {
        public string AdditionalPrompt { get; set; } = string.Empty;
        public string ConversationKind { get; set; } = string.Empty;
        public string ConversationEntryId { get; set; } = string.Empty;
        public string ConversationCategory { get; set; } = string.Empty;
        public string ConditionSummary { get; set; } = string.Empty;
        public IReadOnlyCollection<string> ExpressionIds { get; set; } = new List<string>();
    }

    public sealed class ConversationDraftGenerationResult
    {
        public IReadOnlyList<ConversationDraftLine> Lines { get; set; } = new List<ConversationDraftLine>();
        public string Prompt { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public string ParseError { get; set; } = string.Empty;
    }
}
