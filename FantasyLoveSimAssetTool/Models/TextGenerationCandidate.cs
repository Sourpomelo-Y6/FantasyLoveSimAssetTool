using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class TextGenerationCandidate : ObservableObject
    {
        public TextGenerationCandidate(string text)
        {
            Text = text ?? string.Empty;
        }

        public string Text { get; }
    }
}
