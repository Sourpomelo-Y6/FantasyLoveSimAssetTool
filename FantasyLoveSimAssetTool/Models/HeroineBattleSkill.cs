using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    /// <summary>
    /// Unity の HeroineBattleSkillData と往復するための編集モデル。
    /// enum は Unity 側の名前を文字列で保持し、Tool と Unity の結合を弱くする。
    /// </summary>
    public class HeroineBattleSkill : ObservableObject
    {
        private string displayName = "ヒロインスキル";
        public string SkillId { get; set; } = "HeroineSkill";
        public string DisplayName
        {
            get => displayName;
            set { if (displayName != value) { displayName = value; OnPropertyChanged(); } }
        }
        public string EffectType { get; set; } = "Damage";
        public string Target { get; set; } = "Enemy";
        public int Cost { get; set; }
        public int Power { get; set; }
        public string AffectedStat { get; set; } = "Attack";
        public int StatusDurationTurns { get; set; } = 2;
        public int UseChancePercent { get; set; } = 35;
        public int Priority { get; set; }
        public int MaxUsesPerBattle { get; set; } = 1;
    }
}
