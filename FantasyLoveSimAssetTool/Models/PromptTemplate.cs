namespace FantasyLoveSimAssetTool.Models
{
    public class PromptTemplate
    {
        public string TemplateId { get; set; }

        public string DisplayName { get; set; }

        public AssetUsage Usage { get; set; }

        public string TemplateText { get; set; }

        public PromptTemplate()
        {
            TemplateId = string.Empty;
            DisplayName = string.Empty;
            TemplateText = string.Empty;
        }
    }
}
