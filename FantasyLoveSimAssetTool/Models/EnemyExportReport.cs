using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Models
{
    public class EnemyExportReport
    {
        public string ExportPath { get; set; }

        public int AcceptedAssetCount { get; set; }

        public int ExportedImageCount { get; set; }

        public int ExportedPromptCount { get; set; }

        public int SkippedImageCount { get; set; }

        public ObservableCollection<string> Warnings { get; set; }

        public EnemyExportReport()
        {
            ExportPath = string.Empty;
            Warnings = new ObservableCollection<string>();
        }
    }
}
