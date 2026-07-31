using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityTrainingCatalogDataFile
    {
        public int SchemaVersion { get; set; }
        public string HeroineId { get; set; }
        public string Source { get; set; }
        public List<FromUnityTrainingCatalogItem> Items { get; set; }
    }

    public class FromUnityTrainingCatalogItem
    {
        public string TrainingId { get; set; }
        public string DisplayName { get; set; }
        public string TrainingCategoryId { get; set; }
        public bool UnlockedByDefault { get; set; }
        public int? SortOrder { get; set; }
        public string OccurrenceType { get; set; }
        public List<string> VisibleConditionRanks { get; set; }
        public List<string> ExecutableConditionRanks { get; set; }
        public List<string> RequiredCompletedTrainingIds { get; set; }
        public bool? RequireAllCompletedTrainings { get; set; }
        public bool? HideUntilPrerequisitesMet { get; set; }
        public bool? HideAfterCompletion { get; set; }
        public List<string> UnlockNodeIds { get; set; }
        public List<string> UnlockNodeNames { get; set; }
    }
}
