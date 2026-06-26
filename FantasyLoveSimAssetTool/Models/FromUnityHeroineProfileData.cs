using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityHeroineProfileData
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string DisplayName { get; set; }

        public string InitialDialogueMessage { get; set; }

        public string NextActionPrompt { get; set; }

        public string MorningGreeting { get; set; }

        public string GoodNightGreeting { get; set; }

        public string GameStartFallbackMessage { get; set; }

        public string GameStartFollowUpMessage { get; set; }

        public List<OutfitMessageOverride> OutfitMessageOverrides { get; set; }

        public List<OutfitReactionMessageOverride> OutfitReactionMessageOverrides { get; set; }

        public FromUnityHeroineProfileData()
        {
            HeroineId = string.Empty;
            DisplayName = string.Empty;
            InitialDialogueMessage = string.Empty;
            NextActionPrompt = string.Empty;
            MorningGreeting = string.Empty;
            GoodNightGreeting = string.Empty;
            GameStartFallbackMessage = string.Empty;
            GameStartFollowUpMessage = string.Empty;
            OutfitMessageOverrides = new List<OutfitMessageOverride>();
            OutfitReactionMessageOverrides = new List<OutfitReactionMessageOverride>();
        }
    }
}
