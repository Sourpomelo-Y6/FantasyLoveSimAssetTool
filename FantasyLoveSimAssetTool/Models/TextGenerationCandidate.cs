using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class TextGenerationCandidate : ObservableObject
    {
        public TextGenerationCandidate(string text, int minLength, int maxLength)
        {
            Text = text ?? string.Empty;
            CharacterCount = Text.Length;
            ValidationMessage = CharacterCount < minLength
                ? $"短め（推奨 {minLength}～{maxLength}文字）"
                : CharacterCount > maxLength
                    ? $"長め（推奨 {minLength}～{maxLength}文字）"
                    : "適正";
            HasWarning = CharacterCount < minLength || CharacterCount > maxLength;
        }

        public string Text { get; }

        public int CharacterCount { get; }

        public string ValidationMessage { get; }

        public bool HasWarning { get; }
    }
}
