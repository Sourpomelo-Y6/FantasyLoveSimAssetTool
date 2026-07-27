namespace FantasyLoveSimAssetTool.Models
{
    public class AudioLibraryItem
    {
        public string Category { get; set; } = string.Empty;
        public string LogicalId { get; set; } = string.Empty;
        public string HeroineId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ExpectedPath { get; set; } = string.Empty;
        public int ReferenceCount { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsExpected { get; set; }

        public string StatusSymbol => IsAvailable ? "○" : "×";
        public string StatusText => IsAvailable ? "導入済み" : "未配置";
        public string ReferenceText => ReferenceCount > 0 ? ReferenceCount.ToString() : "-";
    }
}
