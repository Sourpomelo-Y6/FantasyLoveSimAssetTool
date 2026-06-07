namespace FantasyLoveSimAssetTool.Models
{
    public class ComfyPromptQueueResult
    {
        public string PromptId { get; set; }

        public string ClientId { get; set; }

        public ComfyPromptQueueResult()
        {
            PromptId = string.Empty;
            ClientId = string.Empty;
        }
    }
}
