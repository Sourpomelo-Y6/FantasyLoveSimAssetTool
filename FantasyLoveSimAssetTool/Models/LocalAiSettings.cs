namespace FantasyLoveSimAssetTool.Models
{
    public class LocalAiSettings
    {
        public string ServerUrl { get; set; } = "http://127.0.0.1:8080";

        public string ModelId { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; } = 120;

        public double Temperature { get; set; } = 0.7;

        public int MaxTokens { get; set; } = 1024;
    }
}
