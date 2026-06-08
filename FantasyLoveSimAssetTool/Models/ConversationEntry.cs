using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace FantasyLoveSimAssetTool.Models
{
    public class ConversationEntry
    {
        public ConversationDataKind Kind { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public ConversationCondition Conditions { get; set; }

        public ObservableCollection<ConversationLine> Lines { get; set; }

        public string ImageAssetIdsText { get; set; }

        public int Priority { get; set; }

        public string Memo { get; set; }

        [JsonIgnore]
        public string FirstLinePreview
        {
            get
            {
                ConversationLine line = Lines == null ? null : Lines.FirstOrDefault();
                return line == null ? string.Empty : line.Text;
            }
        }

        public ConversationEntry()
        {
            Id = string.Empty;
            Title = string.Empty;
            Category = string.Empty;
            Conditions = new ConversationCondition();
            Lines = new ObservableCollection<ConversationLine>();
            ImageAssetIdsText = string.Empty;
            Priority = 100;
            Memo = string.Empty;
        }
    }
}
