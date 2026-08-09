using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityActionDataFile
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string Source { get; set; }

        public List<FromUnityActionDataItem> Items { get; set; }
    }

    public class FromUnityActionDataItem
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Category { get; set; }

        public string ExecutionType { get; set; }

        public string DisplayColumn { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsEnabled { get; set; }

        public string RequiredItemId { get; set; }

        public List<string> RequiredFlagIds { get; set; }

        public List<FromUnityActionLine> ResultLines { get; set; }

        public List<string> ImageAssetIds { get; set; }

        public int Priority { get; set; }

        public string Memo { get; set; }

        public List<FromUnityActionReaction> Reactions { get; set; }
    }

    public class MenuActionImportSummary
    {
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int UnchangedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Warnings { get; } = new List<string>();
    }

    public class FromUnityActionReaction
    {
        public string Id { get; set; }
        public List<FromUnityActionLine> ResultLines { get; set; }
        public List<string> ImageAssetIds { get; set; }
        public int Priority { get; set; }
        public FromUnityActionReactionCondition Conditions { get; set; }
    }

    public class FromUnityActionReactionCondition
    {
        public int MinAffection { get; set; }
        public int MaxAffection { get; set; }
        public List<string> TimeSlots { get; set; }
        public List<string> Weathers { get; set; }
        public List<string> Seasons { get; set; }
        public string CostumeId { get; set; }
        public bool Once { get; set; }
        public List<string> RequiredFlagIds { get; set; }
        public List<string> RequiredSkillIds { get; set; }
    }

    public class FromUnityActionLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Expression { get; set; }

        public string VoiceId { get; set; }
    }
}
