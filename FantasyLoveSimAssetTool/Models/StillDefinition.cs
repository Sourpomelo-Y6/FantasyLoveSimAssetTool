namespace FantasyLoveSimAssetTool.Models
{
    public class StillDefinition
    {
        public string AssetId { get; set; }

        public string DisplayName { get; set; }

        public AssetUsage Usage { get; set; }

        public string FileName { get; set; }

        public string SpecificPrompt { get; set; }

        public StillStatus Status { get; set; }

        public StillDefinition()
        {
            AssetId = string.Empty;
            DisplayName = string.Empty;
            FileName = string.Empty;
            SpecificPrompt = string.Empty;
            Status = StillStatus.NotGenerated;
        }
    }
}
