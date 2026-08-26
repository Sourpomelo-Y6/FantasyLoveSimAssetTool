using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class ConversationChoice : ObservableObject
    {
        private string choiceText = string.Empty;
        private string responseText = string.Empty;
        private int? affectionChange = 0;

        public string ChoiceText
        {
            get => choiceText;
            set
            {
                string normalized = value ?? string.Empty;
                if (choiceText == normalized) return;
                choiceText = normalized;
                OnPropertyChanged();
            }
        }

        public string ResponseText
        {
            get => responseText;
            set
            {
                string normalized = value ?? string.Empty;
                if (responseText == normalized) return;
                responseText = normalized;
                OnPropertyChanged();
            }
        }

        public int? AffectionChange
        {
            get => affectionChange;
            set
            {
                if (affectionChange == value) return;
                affectionChange = value;
                OnPropertyChanged();
            }
        }
    }
}
