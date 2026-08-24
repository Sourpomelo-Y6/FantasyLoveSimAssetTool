namespace FantasyLoveSimAssetTool.Models
{
    public sealed class ShortTextGenerationTarget
    {
        public ShortTextGenerationTarget(string id, string displayName, string purpose, int minLength, int maxLength,
            bool includeActionPolicy = false, string requiredContext = "None")
        {
            Id = id;
            DisplayName = displayName;
            Purpose = purpose;
            MinLength = minLength;
            MaxLength = maxLength;
            IncludeActionPolicy = includeActionPolicy;
            RequiredContext = requiredContext;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Purpose { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public bool IncludeActionPolicy { get; }
        public string RequiredContext { get; }
    }
}
