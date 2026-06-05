namespace FantasyLoveSimAssetTool.Models
{
    public class ComfySettings
    {
        public string EndpointUrl { get; set; }

        public string WorkflowTemplatePath { get; set; }

        public string PositivePromptPlaceholder { get; set; }

        public string NegativePromptPlaceholder { get; set; }

        public string OutputNodeId { get; set; }

        public ComfySettings()
        {
            EndpointUrl = "http://127.0.0.1:8188";
            WorkflowTemplatePath = "ComfySettings/workflow-template.json";
            PositivePromptPlaceholder = "{PositivePrompt}";
            NegativePromptPlaceholder = "{NegativePrompt}";
            OutputNodeId = string.Empty;
        }
    }
}
