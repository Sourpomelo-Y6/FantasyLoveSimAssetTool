using FantasyLoveSimAssetTool.Common;
using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Models
{
    public class PlayerProfile : ObservableObject
    {
        private string appearancePrompt;
        private string battleCommonPositivePrompt;
        private string negativePrompt;

        public int SchemaVersion { get; set; }

        public string PlayerId { get; set; }

        public string DisplayName { get; set; }

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

        public string BattleCommonPositivePrompt
        {
            get { return battleCommonPositivePrompt; }
            set
            {
                if (battleCommonPositivePrompt == value) { return; }
                battleCommonPositivePrompt = value;
                OnPropertyChanged(nameof(BattleCommonPositivePrompt));
            }
        }

        public string NegativePrompt
        {
            get { return negativePrompt; }
            set
            {
                if (negativePrompt == value) { return; }
                negativePrompt = value;
                OnPropertyChanged(nameof(NegativePrompt));
            }
        }

        public string Memo { get; set; }

        public ObservableCollection<PlayerAsset> Assets { get; set; }

        public PlayerProfile()
        {
            SchemaVersion = 1;
            PlayerId = "Player";
            DisplayName = "Player";
            AppearancePrompt = string.Empty;
            BattleCommonPositivePrompt = "clean lines, highly detailed, masterpiece, 8k, best quality, very aesthetic, absurdres, newest";
            NegativePrompt = "lowres, bad anatomy, bad face, error, extra digit, fewer digits, worst quality, low quality, normal quality, jpeg artifacts, signature, watermark, username, blurry";
            Memo = string.Empty;
            Assets = new ObservableCollection<PlayerAsset>();
        }
    }
}
