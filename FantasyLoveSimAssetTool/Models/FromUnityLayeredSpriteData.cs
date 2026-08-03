using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityLayeredSpriteDataFile
    {
        public int SchemaVersion { get; set; }
        public string HeroineId { get; set; }
        public string Source { get; set; }
        public string DefaultCostumeId { get; set; }
        public string DefaultExpressionId { get; set; }
        public List<FromUnityLayeredSpriteItem> Items { get; set; } = new List<FromUnityLayeredSpriteItem>();
    }

    public class FromUnityLayeredSpriteItem
    {
        public string AssetId { get; set; }
        public string LayerKind { get; set; }
        public string CostumeId { get; set; }
        public string ExpressionId { get; set; }
        public string DisplayName { get; set; }
        public int DrawOrder { get; set; }
        public string FileName { get; set; }
        public string UnityImagePath { get; set; }
    }
}
