namespace FantasyLoveSimAssetTool.Models
{
    public class LayerPreviewItem
    {
        public string AssetId { get; set; }

        public string DisplayName { get; set; }

        public string LayerKind { get; set; }

        public int DrawOrder { get; set; }

        public string ImagePath { get; set; }

        public LayerPreviewItem()
        {
            AssetId = string.Empty;
            DisplayName = string.Empty;
            LayerKind = string.Empty;
            ImagePath = string.Empty;
        }
    }
}
