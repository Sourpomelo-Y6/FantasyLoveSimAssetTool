using FantasyLoveSimAssetTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class TextGenerationCandidate : ObservableObject
    {
        private bool useExpressionSuggestion;

        private readonly List<string> validationMessages = new List<string>();

        public TextGenerationCandidate(string text, int minLength, int maxLength, string expressionId = "",
            IEnumerable<string> warnings = null)
        {
            Text = text ?? string.Empty;
            ExpressionId = expressionId ?? string.Empty;
            useExpressionSuggestion = !string.IsNullOrWhiteSpace(ExpressionId);
            CharacterCount = Text.Length;
            if (CharacterCount < minLength) validationMessages.Add($"短め（推奨 {minLength}～{maxLength}文字）");
            else if (CharacterCount > maxLength) validationMessages.Add($"長め（推奨 {minLength}～{maxLength}文字）");
            foreach (string warning in warnings ?? Enumerable.Empty<string>()) AddWarning(warning);
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

        public string ValidationMessage => validationMessages.Count == 0 ? "適正" : string.Join(" / ", validationMessages);

        public bool HasWarning => validationMessages.Count > 0;

        public void AddWarning(string warning)
        {
            if (string.IsNullOrWhiteSpace(warning) || validationMessages.Contains(warning, StringComparer.Ordinal)) return;
            validationMessages.Add(warning);
            OnPropertyChanged(nameof(ValidationMessage));
            OnPropertyChanged(nameof(HasWarning));
        }
    }
}
