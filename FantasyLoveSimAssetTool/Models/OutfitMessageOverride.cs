using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class OutfitMessageOverride : ObservableObject
    {
        private string outfitId;
        private string lockedMessage;
        private string lockedExpressionId;
        private string changedMessage;
        private string changedExpressionId;

        public string OutfitId
        {
            get => outfitId;
            set { if (outfitId != value) { outfitId = value; OnPropertyChanged(); } }
        }

        public string LockedMessage
        {
            get => lockedMessage;
            set { if (lockedMessage != value) { lockedMessage = value; OnPropertyChanged(); } }
        }

        public string LockedExpressionId
        {
            get => lockedExpressionId;
            set { if (lockedExpressionId != value) { lockedExpressionId = value; OnPropertyChanged(); } }
        }

        public string ChangedMessage
        {
            get => changedMessage;
            set { if (changedMessage != value) { changedMessage = value; OnPropertyChanged(); } }
        }

        public string ChangedExpressionId
        {
            get => changedExpressionId;
            set { if (changedExpressionId != value) { changedExpressionId = value; OnPropertyChanged(); } }
        }

        public OutfitMessageOverride()
        {
            outfitId = string.Empty;
            lockedMessage = string.Empty;
            lockedExpressionId = string.Empty;
            changedMessage = string.Empty;
            changedExpressionId = string.Empty;
        }
    }
}
