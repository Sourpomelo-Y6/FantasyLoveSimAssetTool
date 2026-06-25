using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityEndingDataFile
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string Kind { get; set; }

        public string Source { get; set; }

        public List<FromUnityEndingItem> Items { get; set; }
    }

    public class FromUnityEndingItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public FromUnityEndingCondition Conditions { get; set; }

        public List<FromUnityEndingLine> Lines { get; set; }

        public string Message { get; set; }

        public List<string> ImageAssetIds { get; set; }

        public int Priority { get; set; }

        public string Memo { get; set; }
    }

    public class FromUnityEndingCondition
    {
        public string LocationId { get; set; }

        public int MinAffection { get; set; }

        public int MaxAffection { get; set; }

        public string Weather { get; set; }

        public string Season { get; set; }

        public string TimeOfDay { get; set; }

        public string ActionId { get; set; }

        public string RequiredItemId { get; set; }

        public bool Once { get; set; }

        public List<string> RequiredFlagIds { get; set; }
    }

    public class FromUnityEndingLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Expression { get; set; }
    }
}
