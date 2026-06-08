namespace FantasyLoveSimAssetTool.Models
{
    public class ConversationLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Expression { get; set; }

        public ConversationLine()
        {
            Speaker = "Heroine";
            Text = string.Empty;
            Expression = string.Empty;
        }
    }
}
