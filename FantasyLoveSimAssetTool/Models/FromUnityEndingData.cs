using System.Collections.Generic;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityEndingDataFile
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string Kind { get; set; }

        public string Source { get; set; }

        public List<FromUnityEndingItem> Items { get; set; }

        public List<FromUnityEndingItem> Endings { get; set; }
    }

    public class FromUnityEndingItem
    {
        public string Id { get; set; }

        public string EndingId { get; set; }

        public string Title { get; set; }

        public string DisplayName { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string EndingType { get; set; }

        public FromUnityEndingCondition Conditions { get; set; }

        public int RequiredAffection { get; set; }

        public List<string> RequiredShownEventIds { get; set; }

        public List<string> RequiredFlagIds { get; set; }

        public List<FromUnityEndingLine> Lines { get; set; }

        public string Message { get; set; }

        public FromUnityEndingSourceMetadata SourceMetadata { get; set; }

        public List<string> ImageAssetIds { get; set; }

        public string StillId { get; set; }

        public string StillAssetId { get; set; }

        public string StillSpriteName { get; set; }

        public JsonElement StillSprite { get; set; }

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

        public string CostumeId { get; set; }

        public string RequiredItemId { get; set; }

        public bool Once { get; set; }

        public List<string> RequiredFlagIds { get; set; }
    }

    public class FromUnityEndingLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Message { get; set; }

        public string Expression { get; set; }
    }

    public class FromUnityEndingSourceMetadata
    {
        public string Message { get; set; }
    }
}
