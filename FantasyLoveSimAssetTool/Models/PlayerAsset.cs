namespace FantasyLoveSimAssetTool.Models
{
    public class PlayerAsset
    {
        public string AssetId { get; set; }

        public AssetUsage Usage { get; set; }

        public AssetStatus Status { get; set; }

        public string FileName { get; set; }

        public string SourcePath { get; set; }

        public string StoredPath { get; set; }

        public string PromptRecordPath { get; set; }

        public string Memo { get; set; }

        public PlayerAsset()
        {
            AssetId = string.Empty;
            Usage = AssetUsage.Battle;
            Status = AssetStatus.Pending;
            FileName = string.Empty;
            SourcePath = string.Empty;
            StoredPath = string.Empty;
            PromptRecordPath = string.Empty;
            Memo = string.Empty;
        }
    }
}
