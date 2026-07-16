using FantasyLoveSimAssetTool.Common;
using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Models
{
    public class HeroineProfile : ObservableObject
    {
        private string appearancePrompt;
        private string stillCommonPositivePrompt;

        public string HeroineId { get; set; }

        public string DisplayName { get; set; }

        public string Age { get; set; }

        public string Height { get; set; }

        public string Personality { get; set; }

        public string SpeakingStyle { get; set; }

        public string FirstPerson { get; set; }

        public string SecondPerson { get; set; }

        public string InitialDialogueMessage { get; set; }

        public string NextActionPrompt { get; set; }

        public string MorningGreeting { get; set; }

        public string GoodNightGreeting { get; set; }

        public string GameStartFallbackMessage { get; set; }

        public string GameStartFollowUpMessage { get; set; }

        public string Likes { get; set; }

        public string Dislikes { get; set; }

        public string AppearancePrompt
        {
            get { return appearancePrompt; }
            set
            {
                if (appearancePrompt == value) { return; }
                appearancePrompt = value;
                OnPropertyChanged(nameof(AppearancePrompt));
            }
        }

        public string StillCommonPositivePrompt
        {
            get { return stillCommonPositivePrompt; }
            set
            {
                if (stillCommonPositivePrompt == value) { return; }
                stillCommonPositivePrompt = value;
                OnPropertyChanged(nameof(StillCommonPositivePrompt));
            }
        }

        public string ActionReactionPolicy { get; set; }

        public string EndingPolicy { get; set; }

        public ObservableCollection<OutfitMessageOverride> OutfitMessageOverrides { get; set; }

        public ObservableCollection<OutfitReactionMessageOverride> OutfitReactionMessageOverrides { get; set; }

        public ObservableCollection<HeroineBattleSkill> BattleSkills { get; set; }

        public TrainingImageSettings TrainingImages { get; set; }

        public TrainingDialogueSettings TrainingDialogues { get; set; }

        // 古い profile.json と「空配列を明示」の区別を維持するための互換フラグ。
        public bool BattleSkillsSpecified { get; set; }

        public string ConversationResourcePath { get; set; }

        public string GameEventResourcePath { get; set; }

        public string ActionResourcePath { get; set; }

        public string ScheduledEventResourcePath { get; set; }

        public string BattleResultEventResourcePath { get; set; }

        public string BattlePanelResultMessageResourcePath { get; set; }

        public string EndingResourcePath { get; set; }

        public ObservableCollection<HeroineAsset> Assets { get; set; }

        public ObservableCollection<StillWorkItem> StillWorkItems { get; set; }

        public ObservableCollection<ConversationEntry> ConversationEntries { get; set; }

        public HeroineProfile()
        {
            HeroineId = string.Empty;
            DisplayName = string.Empty;
            Age = string.Empty;
            Height = string.Empty;
            Personality = string.Empty;
            SpeakingStyle = string.Empty;
            FirstPerson = string.Empty;
            SecondPerson = string.Empty;
            InitialDialogueMessage = string.Empty;
            NextActionPrompt = string.Empty;
            MorningGreeting = string.Empty;
            GoodNightGreeting = string.Empty;
            GameStartFallbackMessage = string.Empty;
            GameStartFollowUpMessage = string.Empty;
            Likes = string.Empty;
            Dislikes = string.Empty;
            appearancePrompt = string.Empty;
            stillCommonPositivePrompt = "clean lines,highly detailed,masterpiece,8k,best quality,very aesthetic,absurdres,newest";
            ActionReactionPolicy = string.Empty;
            EndingPolicy = string.Empty;
            OutfitMessageOverrides = new ObservableCollection<OutfitMessageOverride>();
            OutfitReactionMessageOverrides = new ObservableCollection<OutfitReactionMessageOverride>();
            BattleSkills = new ObservableCollection<HeroineBattleSkill>();
            TrainingImages = new TrainingImageSettings();
            TrainingDialogues = new TrainingDialogueSettings();
            ConversationResourcePath = string.Empty;
            GameEventResourcePath = string.Empty;
            ActionResourcePath = string.Empty;
            ScheduledEventResourcePath = string.Empty;
            BattleResultEventResourcePath = string.Empty;
            BattlePanelResultMessageResourcePath = string.Empty;
            EndingResourcePath = string.Empty;
            Assets = new ObservableCollection<HeroineAsset>();
            StillWorkItems = new ObservableCollection<StillWorkItem>();
            ConversationEntries = new ObservableCollection<ConversationEntry>();
        }
    }
}
