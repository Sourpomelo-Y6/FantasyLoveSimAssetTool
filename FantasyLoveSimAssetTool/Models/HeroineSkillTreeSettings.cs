using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class HeroineTrainingSkill : ObservableObject
    {
        private string applicationScope = "AllTrainings";
        private string applicationTargetId = string.Empty;

        public string SkillId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int PlayerHpCostReduction { get; set; }
        public int HeroineHpCostReduction { get; set; }
        public int AffectionRewardModifier { get; set; }
        public int ProficiencyRewardModifier { get; set; }
        public string ApplicationScope
        {
            get => applicationScope;
            set
            {
                if (applicationScope == value) return;
                applicationScope = value;
                if (value == "AllTrainings") ApplicationTargetId = string.Empty;
                OnPropertyChanged();
            }
        }

        public string ApplicationTargetId
        {
            get => applicationTargetId;
            set { if (applicationTargetId != value) { applicationTargetId = value; OnPropertyChanged(); } }
        }
    }

    public class HeroineSkillTreeCondition : ObservableObject
    {
        private string conditionType = "TrainingCount";
        private string scope = "Total";
        private string targetId = string.Empty;
        private int requiredValue;

        public string ConditionType
        {
            get => conditionType;
            set { if (conditionType != value) { conditionType = value; ApplyRequiredScope(); OnPropertyChanged(); } }
        }

        public string Scope
        {
            get => scope;
            set { if (scope != value) { scope = value; if (value == "Total") TargetId = string.Empty; OnPropertyChanged(); } }
        }

        public string TargetId
        {
            get => targetId;
            set { if (targetId != value) { targetId = value; OnPropertyChanged(); } }
        }

        public int RequiredValue
        {
            get => requiredValue;
            set { if (requiredValue != value) { requiredValue = value; OnPropertyChanged(); } }
        }

        private void ApplyRequiredScope()
        {
            if (conditionType == "TrainingProficiency") Scope = "Training";
            else if (conditionType == "Affection" || conditionType == "Day") Scope = "Total";
            else if (conditionType == "MonsterDefeatCount" && scope != "Total" && scope != "Enemy") Scope = "Total";
        }
    }

    public class HeroineSkillTreeNode
    {
        public string NodeId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string TrainingSkillId { get; set; } = string.Empty;
        public string GrantedHeroineSkillId { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int SkillPointCost { get; set; } = 1;
        public ObservableCollection<string> PrerequisiteNodeIds { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> UnlockedTrainingIds { get; set; } = new ObservableCollection<string>();
        public string UnlockEventId { get; set; } = string.Empty;
        public ObservableCollection<HeroineSkillTreeCondition> UnlockConditions { get; set; } =
            new ObservableCollection<HeroineSkillTreeCondition>();
        public float TreePositionX { get; set; }
        public float TreePositionY { get; set; }

        [JsonIgnore]
        public string PrerequisiteNodeIdsText
        {
            get { return string.Join(", ", PrerequisiteNodeIds ?? new ObservableCollection<string>()); }
            set { PrerequisiteNodeIds = SplitIds(value); }
        }

        [JsonIgnore]
        public string UnlockedTrainingIdsText
        {
            get { return string.Join(", ", UnlockedTrainingIds ?? new ObservableCollection<string>()); }
            set { UnlockedTrainingIds = SplitIds(value); }
        }

        [JsonIgnore]
        public string UnlockConditionsText
        {
            get
            {
                return string.Join("; ", (UnlockConditions ?? new ObservableCollection<HeroineSkillTreeCondition>())
                    .Select(x => $"{x.ConditionType}|{x.Scope}|{x.TargetId}|{x.RequiredValue}"));
            }
            set
            {
                UnlockConditions = new ObservableCollection<HeroineSkillTreeCondition>(
                    (value ?? string.Empty).Split(';').Select(ParseCondition).Where(x => x != null));
            }
        }

        private static ObservableCollection<string> SplitIds(string value)
        {
            return new ObservableCollection<string>((value ?? string.Empty).Split(',')
                .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct());
        }

        private static HeroineSkillTreeCondition ParseCondition(string value)
        {
            string[] parts = (value ?? string.Empty).Split('|');
            if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0])) return null;
            int requiredValue = 0;
            if (parts.Length > 3) int.TryParse(parts[3].Trim(), out requiredValue);
            return new HeroineSkillTreeCondition
            {
                ConditionType = parts[0].Trim(),
                Scope = parts.Length > 1 ? parts[1].Trim() : "Total",
                TargetId = parts.Length > 2 ? parts[2].Trim() : string.Empty,
                RequiredValue = requiredValue
            };
        }
    }

    public class HeroineSkillTreeSettings
    {
        public ObservableCollection<HeroineTrainingSkill> TrainingSkills { get; set; } =
            new ObservableCollection<HeroineTrainingSkill>();
        public ObservableCollection<HeroineSkillTreeNode> Nodes { get; set; } =
            new ObservableCollection<HeroineSkillTreeNode>();
    }

    public class HeroineSkillsDataFile
    {
        public int SchemaVersion { get; set; } = 1;
        public string HeroineId { get; set; } = string.Empty;
        public HeroineTrainingSkill[] TrainingSkills { get; set; }
        public HeroineSkillTreeNode[] Nodes { get; set; }
    }
}
