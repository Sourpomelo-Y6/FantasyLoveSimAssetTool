using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class StillDefinition : ObservableObject
    {
        private string assetId;
        private string displayName;
        private AssetUsage usage;
        private string fileName;
        private string specificPrompt;
        private string negativePromptAddition;
        private StillStatus status;

        public string AssetId
        {
            get { return assetId; }
            set
            {
                if (assetId == value) { return; }
                assetId = value;
                OnPropertyChanged(nameof(AssetId));
            }
        }

        public string DisplayName
        {
            get { return displayName; }
            set
            {
                if (displayName == value) { return; }
                displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public AssetUsage Usage
        {
            get { return usage; }
            set
            {
                if (usage == value) { return; }
                usage = value;
                OnPropertyChanged(nameof(Usage));
            }
        }

        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName == value) { return; }
                fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string SpecificPrompt
        {
            get { return specificPrompt; }
            set
            {
                if (specificPrompt == value) { return; }
                specificPrompt = value;
                OnPropertyChanged(nameof(SpecificPrompt));
            }
        }

        public string NegativePromptAddition
        {
            get { return negativePromptAddition; }
            set
            {
                if (negativePromptAddition == value) { return; }
                negativePromptAddition = value;
                OnPropertyChanged(nameof(NegativePromptAddition));
            }
        }

        public StillStatus Status
        {
            get { return status; }
            set
            {
                if (status == value) { return; }
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public StillDefinition()
        {
            assetId = string.Empty;
            displayName = string.Empty;
            fileName = string.Empty;
            specificPrompt = string.Empty;
            negativePromptAddition = string.Empty;
            status = StillStatus.NotGenerated;
        }
    }
}
