namespace FantasyLoveSimAssetTool.Models
{
    public sealed class ShortTextGenerationContext
    {
        public string OutfitId { get; set; } = string.Empty;

        public string ReactionType { get; set; } = string.Empty;

        public string TaskContext { get; set; } = string.Empty;

        public string ConversationKind { get; set; } = string.Empty;

        public string ConversationEntryId { get; set; } = string.Empty;

        public string ConversationCategory { get; set; } = string.Empty;

        public string ConversationSpeaker { get; set; } = string.Empty;

        public string PreviousConversationLines { get; set; } = string.Empty;

        public string ConversationConditions { get; set; } = string.Empty;

        public string ConversationAdditionalPrompt { get; set; } = string.Empty;
    }
}
