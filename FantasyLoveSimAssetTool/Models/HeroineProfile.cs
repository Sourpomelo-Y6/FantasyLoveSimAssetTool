using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Models
{
    public class HeroineProfile
    {
        public string HeroineId { get; set; }

        public string DisplayName { get; set; }

        public string Age { get; set; }

        public string Height { get; set; }

        public string Personality { get; set; }

        public string SpeakingStyle { get; set; }

        public string FirstPerson { get; set; }

        public string SecondPerson { get; set; }

        public string Likes { get; set; }

        public string Dislikes { get; set; }

        public string AppearancePrompt { get; set; }

        public string ActionReactionPolicy { get; set; }

        public string EndingPolicy { get; set; }

        public ObservableCollection<HeroineAsset> Assets { get; set; }

        public ObservableCollection<StillWorkItem> StillWorkItems { get; set; }

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
            Likes = string.Empty;
            Dislikes = string.Empty;
            AppearancePrompt = string.Empty;
            ActionReactionPolicy = string.Empty;
            EndingPolicy = string.Empty;
            Assets = new ObservableCollection<HeroineAsset>();
            StillWorkItems = new ObservableCollection<StillWorkItem>();
        }
    }
}
