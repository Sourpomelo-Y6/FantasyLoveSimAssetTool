using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class TrainingDialogueGenerationContext
    {
        public string TrainingId { get; set; } = string.Empty;
        public string TrainingDisplayName { get; set; } = string.Empty;
        public string TrainingCategory { get; set; } = string.Empty;
        public string VisualState { get; set; } = string.Empty;
        public bool PlayerVisible { get; set; }
        public bool HeroineVisible { get; set; }
        public string AdditionalInstruction { get; set; } = string.Empty;
        public IReadOnlyCollection<string> ExistingMessages { get; set; } = new List<string>();
    }

    public sealed class TrainingDialogueGenerationResult
    {
        public IReadOnlyList<string> Candidates { get; set; } = new List<string>();
        public string Prompt { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public string ParseError { get; set; } = string.Empty;
    }
}
