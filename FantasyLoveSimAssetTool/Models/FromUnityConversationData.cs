using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityConversationDataFile
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string Kind { get; set; }

        public string Source { get; set; }

        public List<FromUnityConversationItem> Items { get; set; }
    }

    public class FromUnityConversationItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public FromUnityConversationCondition Conditions { get; set; }

        public List<FromUnityConversationLine> Lines { get; set; }

        public List<string> ImageAssetIds { get; set; }

        public FromUnityConversationSourceMetadata SourceMetadata { get; set; }

        public int Priority { get; set; }

        public string Memo { get; set; }
    }

    public class FromUnityConversationCondition
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

    public class FromUnityConversationLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Expression { get; set; }
    }

    public class FromUnityConversationSourceMetadata
    {
        public List<FromUnityConversationChoice> Choices { get; set; }
    }

    public class FromUnityConversationChoice
    {
        public string ChoiceText { get; set; }

        public string ResponseText { get; set; }

        public int AffectionChange { get; set; }
    }
}
