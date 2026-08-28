namespace FantasyLoveSimAssetTool.Models
{
    public class StillWorkItem
    {
        public string AssetId { get; set; }

        public StillStatus Status { get; set; }

        public string SpecificPrompt { get; set; }

        public string NegativePromptAddition { get; set; }

        public bool IsHidden { get; set; }

        public StillWorkItem()
        {
            AssetId = string.Empty;
            Status = StillStatus.NotGenerated;
            SpecificPrompt = string.Empty;
            NegativePromptAddition = string.Empty;
        }
    }
}
