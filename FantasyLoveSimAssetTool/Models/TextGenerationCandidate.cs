using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class TextGenerationCandidate : ObservableObject
    {
        private bool useExpressionSuggestion;

        public TextGenerationCandidate(string text, int minLength, int maxLength, string expressionId = "")
        {
            Text = text ?? string.Empty;
            ExpressionId = expressionId ?? string.Empty;
            useExpressionSuggestion = !string.IsNullOrWhiteSpace(ExpressionId);
            CharacterCount = Text.Length;
            ValidationMessage = CharacterCount < minLength
                ? $"短め（推奨 {minLength}～{maxLength}文字）"
                : CharacterCount > maxLength
                    ? $"長め（推奨 {minLength}～{maxLength}文字）"
                    : "適正";
            HasWarning = CharacterCount < minLength || CharacterCount > maxLength;
        }

        public string Text { get; }

        public string ExpressionId { get; }

        public bool HasExpressionSuggestion => !string.IsNullOrWhiteSpace(ExpressionId);

        public bool UseExpressionSuggestion
        {
            get => useExpressionSuggestion;
            set { if (useExpressionSuggestion != value) { useExpressionSuggestion = value; OnPropertyChanged(); } }
        }

        public int CharacterCount { get; }

        public string ValidationMessage { get; }

        public bool HasWarning { get; }
    }
}
