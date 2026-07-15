using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityHeroineProfileData
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string DisplayName { get; set; }

        public string HeroineFirstPerson { get; set; }

        public string PlayerSecondPerson { get; set; }

        public string InitialDialogueMessage { get; set; }

        public string NextActionPrompt { get; set; }

        public string MorningGreeting { get; set; }

        public string GoodNightGreeting { get; set; }

        public string GameStartFallbackMessage { get; set; }

        public string GameStartFollowUpMessage { get; set; }

        public List<OutfitMessageOverride> OutfitMessageOverrides { get; set; }

        public List<OutfitReactionMessageOverride> OutfitReactionMessageOverrides { get; set; }

        public List<HeroineBattleSkill> BattleSkills { get; set; }

        public string ConversationResourcePath { get; set; }
        public string GameEventResourcePath { get; set; }
        public string ActionResourcePath { get; set; }
        public string ScheduledEventResourcePath { get; set; }
        public string BattleResultEventResourcePath { get; set; }
        public string BattlePanelResultMessageResourcePath { get; set; }
        public string EndingResourcePath { get; set; }

        // 初期化しないことで、JSONで省略された値(null)と明示的な空値を区別する。
    }
}
