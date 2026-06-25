using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Models
{
    public class ExportReport
    {
        public string ExportPath { get; set; }

        public int AcceptedAssetCount { get; set; }

        public int ExportedImageCount { get; set; }

        public int ExportedPromptCount { get; set; }

        public int SkippedImageCount { get; set; }

        public int ConversationCount { get; set; }

        public int GameEventCount { get; set; }

        public int ScheduledEventCount { get; set; }

        public int ActionReactionCount { get; set; }

        public int EndingCount { get; set; }

        public int TotalConversationDataCount
        {
            get { return ConversationCount + GameEventCount + ScheduledEventCount + ActionReactionCount + EndingCount; }
        }

        public ObservableCollection<string> Warnings { get; set; }

        public ExportReport()
        {
            ExportPath = string.Empty;
            Warnings = new ObservableCollection<string>();
        }
    }
}
