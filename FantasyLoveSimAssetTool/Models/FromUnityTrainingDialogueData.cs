using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityTrainingDialogueDataFile
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string Source { get; set; }

        public List<FromUnityTrainingDialogueItem> Items { get; set; }
    }

    public class FromUnityTrainingDialogueItem
    {
        public string TrainingId { get; set; }

        public string VisualState { get; set; }

        public List<string> Messages { get; set; }
    }
}
