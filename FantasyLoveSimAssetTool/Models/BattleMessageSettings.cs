using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace FantasyLoveSimAssetTool.Models
{
    public class BattleResultEventEntry
    {
        public string EventId { get; set; } = string.Empty;
        public string ResultType { get; set; } = "SoloVictory";
        public string BattleContextId { get; set; } = string.Empty;
        public string SpeakerType { get; set; } = "Heroine";
        public string SpeakerName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string VoiceId { get; set; }
        public string StillId { get; set; } = string.Empty;
        public string VisualMode { get; set; } = "Auto";
        public string ExpressionId { get; set; } = string.Empty;
        public int AffectionChange { get; set; }
        public string[] UnlockedOutfitIds { get; set; } = new string[0];

        [JsonIgnore]
        public string UnlockedOutfitIdsText
        {
            get { return string.Join(", ", UnlockedOutfitIds ?? new string[0]); }
            set
            {
                UnlockedOutfitIds = (value ?? string.Empty).Split(',').Select(x => x.Trim())
                    .Where(x => x.Length > 0).Distinct().ToArray();
            }
        }
    }

    public class BattlePanelResultMessageEntry
    {
        public string MessageId { get; set; } = string.Empty;
        public string ResultType { get; set; } = "Default";
        public string Message { get; set; } = string.Empty;
        public string VoiceId { get; set; }
    }

    public class SoloReturnReactionEntry
    {
        public string ReactionId { get; set; } = string.Empty;
        public string ResultType { get; set; } = "SoloVictory";
        public string BattleContextId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string VoiceId { get; set; }
        public string StillId { get; set; } = string.Empty;
        public string VisualMode { get; set; } = "Auto";
        public string ExpressionId { get; set; } = string.Empty;
    }

    public class BattleMessageSettings
    {
        public ObservableCollection<BattleResultEventEntry> ResultEvents { get; set; } =
            new ObservableCollection<BattleResultEventEntry>();
        public ObservableCollection<BattlePanelResultMessageEntry> PanelMessages { get; set; } =
            new ObservableCollection<BattlePanelResultMessageEntry>();
        public ObservableCollection<SoloReturnReactionEntry> SoloReturnReactions { get; set; } =
            new ObservableCollection<SoloReturnReactionEntry>();
    }

    public class BattleResultEventsDataFile
    {
        public int SchemaVersion { get; set; } = 1;
        public string HeroineId { get; set; } = string.Empty;
        public BattleResultEventEntry[] Items { get; set; }
    }

    public class BattlePanelResultMessagesDataFile
    {
        public int SchemaVersion { get; set; } = 1;
        public string HeroineId { get; set; } = string.Empty;
        public BattlePanelResultMessageEntry[] Items { get; set; }
    }

    public class SoloReturnReactionsDataFile
    {
        public int SchemaVersion { get; set; } = 1;
        public string HeroineId { get; set; } = string.Empty;
        public SoloReturnReactionEntry[] Items { get; set; }
    }
}
