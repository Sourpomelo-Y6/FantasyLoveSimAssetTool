namespace FantasyLoveSimAssetTool.Models
{
    public class ConversationChoice
    {
        public string ChoiceText { get; set; }

        public string ResponseText { get; set; }

        public int? AffectionChange { get; set; }

        public ConversationChoice()
        {
            ChoiceText = string.Empty;
            ResponseText = string.Empty;
            AffectionChange = 0;
        }
    }
}
