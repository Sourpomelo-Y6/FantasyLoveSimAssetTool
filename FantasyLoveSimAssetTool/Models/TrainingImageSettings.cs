using System.Collections.ObjectModel;

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
}
