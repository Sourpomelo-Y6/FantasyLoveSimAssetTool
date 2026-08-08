using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class HeadPartWorkspaceItem : ObservableObject
    {
        private string sourceImagePath = string.Empty;
        private string registeredImagePath = string.Empty;
        private string statusText = "未登録";
        private string expressionId = string.Empty;
        private string assetId = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string AssetId
        {
            get => assetId;
            set
            {
                if (assetId == value) return;
                assetId = value;
                OnPropertyChanged();
            }
        }

        public string LayerKind { get; set; } = string.Empty;

        public string ExpressionId
        {
            get => expressionId;
            set
            {
                if (expressionId == value) return;
                expressionId = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpression => LayerKind == "HeadExpression";

        public int DrawOrder { get; set; }

        public string SourceImagePath
        {
            get => sourceImagePath;
            set
            {
                if (sourceImagePath == value) return;
                sourceImagePath = value;
                OnPropertyChanged();
            }
        }

        public string RegisteredImagePath
        {
            get => registeredImagePath;
            set
            {
                if (registeredImagePath == value) return;
                registeredImagePath = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => statusText;
            set
            {
                if (statusText == value) return;
                statusText = value;
                OnPropertyChanged();
            }
        }
    }
}
