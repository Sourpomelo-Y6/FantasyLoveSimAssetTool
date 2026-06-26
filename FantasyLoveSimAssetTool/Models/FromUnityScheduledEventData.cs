using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class FromUnityScheduledEventDataFile
    {
        public int SchemaVersion { get; set; }

        public string HeroineId { get; set; }

        public string Kind { get; set; }

        public string Source { get; set; }

        public List<FromUnityScheduledEventItem> Items { get; set; }

        public List<FromUnityScheduledEventItem> ScheduledEvents { get; set; }
    }

    public class FromUnityScheduledEventItem
    {
        public string Id { get; set; }

        public string ScheduledEventId { get; set; }

        public string ScheduleType { get; set; }

        public string Title { get; set; }

        public string DisplayName { get; set; }

        public string Category { get; set; }

        public FromUnityScheduledEventCondition Conditions { get; set; }

        public string ActionId { get; set; }

        public string TriggerTimeSlot { get; set; }

        public string OutfitPromptMode { get; set; }

        public string EventSpeakerType { get; set; }

        public string PreparationMessage { get; set; }

        public string EventMessage { get; set; }

        public List<FromUnityScheduledEventLine> Lines { get; set; }

        public List<string> ImageAssetIds { get; set; }

        public string StillId { get; set; }

        public string StillAssetId { get; set; }

        public int AffectionChange { get; set; }

        public int Priority { get; set; }

        public string Memo { get; set; }
    }

    public class FromUnityScheduledEventCondition
    {
        public string ScheduleType { get; set; }

        public string ActionId { get; set; }

        public string TriggerTimeSlot { get; set; }

        public string LocationId { get; set; }

        public string Weather { get; set; }

        public string Season { get; set; }

        public string TimeOfDay { get; set; }

        public int MinAffection { get; set; }

        public int MaxAffection { get; set; }

        public string RequiredItemId { get; set; }

        public bool Once { get; set; }

        public List<string> RequiredFlagIds { get; set; }
    }

    public class FromUnityScheduledEventLine
    {
        public string Speaker { get; set; }

        public string Text { get; set; }

        public string Expression { get; set; }
    }
}
