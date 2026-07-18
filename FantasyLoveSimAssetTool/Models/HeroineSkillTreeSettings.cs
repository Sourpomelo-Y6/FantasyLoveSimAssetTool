using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace FantasyLoveSimAssetTool.Models
{
    public class HeroineTrainingSkill
    {
        public string SkillId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int PlayerHpCostReduction { get; set; }
        public int HeroineHpCostReduction { get; set; }
        public int AffectionRewardModifier { get; set; }
        public int ProficiencyRewardModifier { get; set; }
        public string ApplicationScope { get; set; } = "AllTrainings";
        public string ApplicationTargetId { get; set; } = string.Empty;
    }

    public class HeroineSkillTreeCondition
    {
        public string ConditionType { get; set; } = "TrainingCount";
        public string Scope { get; set; } = "Total";
        public string TargetId { get; set; } = string.Empty;
        public int RequiredValue { get; set; }
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
