using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Models
{
    public enum ExportValidationSeverity { Information, Warning, Error }

    public sealed class ExportValidationIssue
    {
        public ExportValidationSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public ProductionStatusTargetKind TargetKind { get; set; }
        public string TargetId { get; set; } = string.Empty;
        public int TargetTabIndex { get; set; } = 15;
        public ConversationDataKind ConversationKind { get; set; }
    }

    public sealed class ExportValidationResult
    {
        public IReadOnlyList<ExportValidationIssue> Issues { get; set; } = new List<ExportValidationIssue>();
        public int ErrorCount => Issues.Count(x => x.Severity == ExportValidationSeverity.Error);
        public int WarningCount => Issues.Count(x => x.Severity == ExportValidationSeverity.Warning);
        public bool CanExport => ErrorCount == 0;
    }
}
