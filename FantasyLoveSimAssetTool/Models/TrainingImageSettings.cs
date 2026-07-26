using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
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
        private string voiceId = string.Empty;

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

        public string VoiceId
        {
            get { return voiceId; }
            set
            {
                if (voiceId == value) { return; }
                voiceId = value;
                OnPropertyChanged(nameof(VoiceId));
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

    public class TrainingCatalogItem
    {
        public string TrainingId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string TrainingCategoryId { get; set; } = string.Empty;
        public bool UnlockedByDefault { get; set; }
        public List<string> UnlockNodeIds { get; set; } = new List<string>();
        public List<string> UnlockNodeNames { get; set; } = new List<string>();

        [JsonIgnore]
        public string UnlockSummary => UnlockedByDefault
            ? "初期解放"
            : UnlockNodeNames != null && UnlockNodeNames.Count > 0
                ? string.Join(" / ", UnlockNodeNames)
                : "解放ノード未設定";
    }

    public class TrainingCatalogSettings
    {
        public ObservableCollection<TrainingCatalogItem> Items { get; set; } =
            new ObservableCollection<TrainingCatalogItem>();
    }
}
