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

        public string RequiredItemId { get; set; }

        public List<string> RequiredFlagIds { get; set; }

        public List<FromUnityActionLine> ResultLines { get; set; }

        public List<string> ImageAssetIds { get; set; }

        public int Priority { get; set; }

        public string Memo { get; set; }
    }

    public class FromUnityActionLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Expression { get; set; }
    }
}
