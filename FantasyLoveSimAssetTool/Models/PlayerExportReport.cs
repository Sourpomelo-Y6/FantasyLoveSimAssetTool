using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class PlayerExportReport
    {
        public string ExportPath { get; set; }

        public int AcceptedAssetCount { get; set; }

        public int ExportedImageCount { get; set; }

        public int SkippedImageCount { get; set; }

        public int ExportedPromptCount { get; set; }

        public List<string> Warnings { get; set; }

        public PlayerExportReport()
        {
            ExportPath = string.Empty;
            Warnings = new List<string>();
        }
    }
}
