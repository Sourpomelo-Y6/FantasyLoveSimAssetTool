namespace FantasyLoveSimAssetTool.Models
{
    public class OutfitMessageOverride
    {
        public string OutfitId { get; set; }

        public string LockedMessage { get; set; }

        public string ChangedMessage { get; set; }

        public OutfitMessageOverride()
        {
            OutfitId = string.Empty;
            LockedMessage = string.Empty;
            ChangedMessage = string.Empty;
        }
    }
}
