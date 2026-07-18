namespace FantasyLoveSimAssetTool.Models
{
    public class ConversationCondition
    {
        public string LocationId { get; set; }

        public int MinAffection { get; set; }

        public int MaxAffection { get; set; }

        public string Weather { get; set; }

        public string Season { get; set; }

        public string TimeOfDay { get; set; }

        public string ActionId { get; set; }

        public string CostumeId { get; set; }

        public string RequiredItemId { get; set; }

        public bool Once { get; set; }

        public string RequiredFlagIdsText { get; set; }

        public string RequiredSkillIdsText { get; set; }

        public bool RequiredSkillIdsSpecified { get; set; }

        public ConversationCondition()
        {
            LocationId = string.Empty;
            MaxAffection = 9999;
            Weather = string.Empty;
            Season = string.Empty;
            TimeOfDay = string.Empty;
            ActionId = string.Empty;
            CostumeId = string.Empty;
            RequiredItemId = string.Empty;
            RequiredFlagIdsText = string.Empty;
            RequiredSkillIdsText = string.Empty;
        }
    }
}
