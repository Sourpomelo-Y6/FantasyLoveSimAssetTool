using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class ConversationSituationPrompt
    {
        public string SituationId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        public List<string> RequiredElements { get; set; } = new List<string>();
        public List<string> AvoidElements { get; set; } = new List<string>();
        public ConversationSituationConditionSuggestion SuggestedConditions { get; set; }
    }

    public sealed class ConversationSituationConditionSuggestion
    {
        public string LocationId { get; set; } = string.Empty;
        public string Weather { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public string TimeOfDay { get; set; } = string.Empty;
        public int? MinAffection { get; set; }
        public int? MaxAffection { get; set; }
        public int? Priority { get; set; }
        public bool? Once { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public sealed class ConversationCharacterPrompt
    {
        public string HeroineId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string FirstPerson { get; set; } = string.Empty;
        public string SecondPerson { get; set; } = string.Empty;
        public List<string> VoiceRules { get; set; } = new List<string>();
        public List<string> RelationshipRules { get; set; } = new List<string>();
        public List<string> EmotionRules { get; set; } = new List<string>();
        public List<string> AvoidExpressions { get; set; } = new List<string>();
        public List<string> ReferenceLines { get; set; } = new List<string>();
    }
}
