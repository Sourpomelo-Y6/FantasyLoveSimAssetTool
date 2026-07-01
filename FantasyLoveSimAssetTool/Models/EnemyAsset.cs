namespace FantasyLoveSimAssetTool.Models
{
    public class EnemyAsset
    {
        public string AssetId { get; set; }

        public AssetUsage Usage { get; set; }

        public AssetStatus Status { get; set; }

        public string FileName { get; set; }

        public string SourcePath { get; set; }

        public string StoredPath { get; set; }

        public string PromptRecordPath { get; set; }

        public string Memo { get; set; }

        public EnemyAsset()
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
