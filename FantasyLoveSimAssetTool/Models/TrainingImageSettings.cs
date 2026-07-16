using System.Collections.ObjectModel;
using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class TrainingImageDefaults
    {
        public string BeforeFirstStepImageAssetId { get; set; } = string.Empty;
        public string AfterFirstStepImageAssetId { get; set; } = string.Empty;
        public string PlayerLpConsumedImageAssetId { get; set; } = string.Empty;
        public string HeroineLpConsumedImageAssetId { get; set; } = string.Empty;
        public string SimultaneousLpConsumedImageAssetId { get; set; } = string.Empty;
    }

    public class TrainingImageEntry
    {
        public string TrainingId { get; set; } = string.Empty;
        public string BeforeFirstStepImageAssetId { get; set; } = string.Empty;
        public string AfterFirstStepImageAssetId { get; set; } = string.Empty;
        public string PlayerLpConsumedImageAssetId { get; set; } = string.Empty;
        public string HeroineLpConsumedImageAssetId { get; set; } = string.Empty;
        public string SimultaneousLpConsumedImageAssetId { get; set; } = string.Empty;
        public string Memo { get; set; } = string.Empty;
    }

    public class TrainingImageSettings
    {
        public TrainingImageDefaults Defaults { get; set; } = new TrainingImageDefaults();
        public ObservableCollection<TrainingImageEntry> Items { get; set; } =
            new ObservableCollection<TrainingImageEntry>();
    }

    public class TrainingDialogueMessage : ObservableObject
    {
        private string text = string.Empty;

        public string Text
        {
            get { return text; }
            set
            {
                if (text == value) { return; }
                text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    public class TrainingDialogueEntry
    {
        public string TrainingId { get; set; } = string.Empty;
        public string VisualState { get; set; } = string.Empty;
        public ObservableCollection<TrainingDialogueMessage> Messages { get; set; } =
            new ObservableCollection<TrainingDialogueMessage>();
    }

    public class TrainingDialogueSettings
    {
        public ObservableCollection<TrainingDialogueEntry> Items { get; set; } =
            new ObservableCollection<TrainingDialogueEntry>();
    }
}
