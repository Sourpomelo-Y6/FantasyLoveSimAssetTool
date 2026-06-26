using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityGameEventDataFile
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string Kind { get; set; }

        public string Source { get; set; }

        public List<FromUnityGameEventItem> Items { get; set; }
    }

    public class FromUnityGameEventItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public FromUnityGameEventCondition Conditions { get; set; }

        public List<FromUnityGameEventLine> Lines { get; set; }

        public List<string> ImageAssetIds { get; set; }

        public FromUnityGameEventSourceMetadata SourceMetadata { get; set; }

        public int Priority { get; set; }

        public string Memo { get; set; }
    }

    public class FromUnityGameEventCondition
    {
        public string LocationId { get; set; }

        public int MinAffection { get; set; }

        public int MaxAffection { get; set; }

        public string Weather { get; set; }

        public string Season { get; set; }

        public string TimeOfDay { get; set; }

        public string ActionId { get; set; }

        public string CostumeId { get; set; }

        public string RequiredItemId { get; set; }

        public bool Once { get; set; }

        public List<string> RequiredFlagIds { get; set; }
    }

    public class FromUnityGameEventLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Expression { get; set; }
    }

    public class FromUnityGameEventSourceMetadata
    {
        public List<FromUnityGameEventChoice> Choices { get; set; }
    }

    public class FromUnityGameEventChoice
    {
        public string ChoiceText { get; set; }

        public string ResponseText { get; set; }

        public int AffectionChange { get; set; }
    }
}
