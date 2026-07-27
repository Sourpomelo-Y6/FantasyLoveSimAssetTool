using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class ConversationLine : ObservableObject
    {
        private string speaker;
        private string text;
        private string expression;
        private string voiceId;

        public string Speaker
        {
            get { return speaker; }
            set { if (speaker != value) { speaker = value; OnPropertyChanged(nameof(Speaker)); } }
        }

        public string Text
        {
            get { return text; }
            set { if (text != value) { text = value; OnPropertyChanged(nameof(Text)); } }
        }

        public string Expression
        {
            get { return expression; }
            set { if (expression != value) { expression = value; OnPropertyChanged(nameof(Expression)); } }
        }

        public string VoiceId
        {
            get { return voiceId; }
            set { if (voiceId != value) { voiceId = value; OnPropertyChanged(nameof(VoiceId)); } }
        }

        public ConversationLine()
        {
            Speaker = "Heroine";
            Text = string.Empty;
            Expression = string.Empty;
            VoiceId = string.Empty;
        }
    }
}
