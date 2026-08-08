namespace FantasyLoveSimAssetTool.Models
{
    public class MenuActionDefinition
    {
        public string ActionId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int DisplayColumn { get; set; }

        public string ExecutionType { get; set; } = "SimpleAction";

        public bool IsEnabled { get; set; } = true;

        public bool IsRequired { get; set; } = true;
    }
}
