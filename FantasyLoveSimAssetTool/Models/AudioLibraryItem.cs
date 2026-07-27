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
        public string ReferenceDetails { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsExpected { get; set; }

        public string StatusSymbol => IsAvailable ? "○" : "×";
        public string StatusText => IsAvailable ? "導入済み" : "未配置";
        public string ReferenceText => ReferenceCount > 0 ? ReferenceCount.ToString() : "-";
        public bool IsUnusedVoice => Category == "VOICE" && IsAvailable && ReferenceCount == 0;
        public string VoiceStatusSymbol => !IsAvailable ? "×" : IsUnusedVoice ? "△" : "○";
        public string VoiceStatusText => !IsAvailable ? "未配置" : IsUnusedVoice ? "未使用" : "使用中";
        public string VoiceStatusToolTip => !IsAvailable
            ? "Toolデータから参照されていますが、音声ファイルが見つかりません。"
            : IsUnusedVoice
                ? "Toolデータ内に参照元がありません。Unity側から直接参照されている可能性があるため、削除前に確認してください。"
                : string.IsNullOrWhiteSpace(ReferenceDetails)
                    ? "Toolデータから参照されています。"
                    : ReferenceDetails;
    }
}
