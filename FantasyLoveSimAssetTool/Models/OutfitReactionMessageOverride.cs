using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class OutfitReactionMessageOverride : ObservableObject
    {
        private string reactionType;
        private string message;

        public string ReactionType
        {
            get => reactionType;
            set { if (reactionType != value) { reactionType = value; OnPropertyChanged(); } }
        }

        public string Message
        {
            get => message;
            set { if (message != value) { message = value; OnPropertyChanged(); } }
        }

        public OutfitReactionMessageOverride()
        {
            reactionType = string.Empty;
            message = string.Empty;
        }
    }
}
