using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public int SortOrder { get; set; }
        public string OccurrenceType { get; set; } = "Repeatable";
        public List<string> VisibleConditionRanks { get; set; } = new List<string>();
        public List<string> ExecutableConditionRanks { get; set; } = new List<string>();
        public List<string> RequiredCompletedTrainingIds { get; set; } = new List<string>();
        public bool RequireAllCompletedTrainings { get; set; } = true;
        public bool HideUntilPrerequisitesMet { get; set; } = true;
        public bool HideAfterCompletion { get; set; }
        public List<string> UnlockNodeIds { get; set; } = new List<string>();
        public List<string> UnlockNodeNames { get; set; } = new List<string>();

        [JsonIgnore]
        public string ReferenceWarning { get; set; } = string.Empty;

        [JsonIgnore]
        public string UnlockSummary => UnlockedByDefault
            ? "初期解放"
            : UnlockNodeNames != null && UnlockNodeNames.Count > 0
                ? string.Join(" / ", UnlockNodeNames)
                : "解放ノード未設定";

        [JsonIgnore]
        public string ConditionBadgeSummary
        {
            get
            {
                List<string> badges = new List<string>();
                if (string.Equals(OccurrenceType, "OncePerSave", System.StringComparison.OrdinalIgnoreCase))
                    badges.Add("一回限定");
                if (VisibleConditionRanks?.Count > 0)
                    badges.Add(IsExcellentOnly(VisibleConditionRanks)
                        ? "絶好調限定"
                        : "表示条件あり");
                if (ExecutableConditionRanks?.Count > 0)
                    badges.Add(IsDisabledWhenPoor(ExecutableConditionRanks)
                        ? "不調時不可"
                        : "実行条件あり");
                if (RequiredCompletedTrainingIds?.Count > 0)
                    badges.Add("前提訓練あり");
                if (HideAfterCompletion)
                    badges.Add("完了後非表示");
                return badges.Count > 0 ? string.Join(" / ", badges) : "常時・反復可能";
            }
        }

        [JsonIgnore]
        public string PrerequisiteSummary => RequiredCompletedTrainingIds?.Count > 0
            ? string.Join(RequireAllCompletedTrainings ? " AND " : " OR ", RequiredCompletedTrainingIds)
            : "なし";

        [JsonIgnore]
        public string ConditionDetails =>
            $"出現: {FormatRanks(VisibleConditionRanks)}" +
            $"\n実行: {FormatRanks(ExecutableConditionRanks)}" +
            $"\n回数: {(string.Equals(OccurrenceType, "OncePerSave", System.StringComparison.OrdinalIgnoreCase) ? "セーブデータにつき一回" : "反復可能")}" +
            $"\n前提: {PrerequisiteSummary}" +
            $"\n表示規則: {(HideUntilPrerequisitesMet ? "前提未達時は非表示" : "前提未達時も表示")} / {(HideAfterCompletion ? "完了後は非表示" : "完了後も表示")}";

        private static bool IsExcellentOnly(IEnumerable<string> ranks)
        {
            List<string> values = ranks?.ToList() ?? new List<string>();
            return values.Count == 1 &&
                string.Equals(values[0], "Excellent", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDisabledWhenPoor(IEnumerable<string> ranks)
        {
            List<string> values = ranks?.ToList() ?? new List<string>();
            return values.Count > 0 &&
                !values.Any(value =>
                    string.Equals(value, "Poor", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "Awful", System.StringComparison.OrdinalIgnoreCase));
        }

        private static string FormatRanks(IEnumerable<string> ranks)
        {
            List<string> values = ranks?.ToList() ?? new List<string>();
            return values.Count == 0
                ? "すべての調子"
                : string.Join(" / ", values.Select(ToJapaneseRank));
        }

        private static string ToJapaneseRank(string rank)
        {
            if (string.Equals(rank, "Excellent", System.StringComparison.OrdinalIgnoreCase)) return "絶好調";
            if (string.Equals(rank, "Good", System.StringComparison.OrdinalIgnoreCase)) return "好調";
            if (string.Equals(rank, "Normal", System.StringComparison.OrdinalIgnoreCase)) return "普通";
            if (string.Equals(rank, "Poor", System.StringComparison.OrdinalIgnoreCase)) return "不調";
            if (string.Equals(rank, "Awful", System.StringComparison.OrdinalIgnoreCase)) return "絶不調";
            return rank ?? string.Empty;
        }
    }

    public class TrainingCatalogSettings
    {
        public ObservableCollection<TrainingCatalogItem> Items { get; set; } =
            new ObservableCollection<TrainingCatalogItem>();
    }
}
