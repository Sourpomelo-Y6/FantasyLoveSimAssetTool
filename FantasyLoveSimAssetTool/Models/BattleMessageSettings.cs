using FantasyLoveSimAssetTool.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace FantasyLoveSimAssetTool.Models
{
    public class BattleResultEventEntry : ObservableObject
    {
        private string eventId = string.Empty;
        private string resultType = "SoloVictory";
        private string battleContextId = string.Empty;
        private string speakerType = "Heroine";
        private string speakerName = string.Empty;
        private string message = string.Empty;
        private string voiceId;
        private string stillId = string.Empty;
        private string visualMode = "Auto";
        private string expressionId = string.Empty;
        private int affectionChange;
        private string[] unlockedOutfitIds = new string[0];

        public string EventId { get => eventId; set => Set(ref eventId, value); }
        public string ResultType { get => resultType; set => Set(ref resultType, value); }
        public string BattleContextId { get => battleContextId; set => Set(ref battleContextId, value); }
        public string SpeakerType { get => speakerType; set => Set(ref speakerType, value); }
        public string SpeakerName { get => speakerName; set => Set(ref speakerName, value); }
        public string Message { get => message; set => Set(ref message, value); }
        public string VoiceId { get => voiceId; set => Set(ref voiceId, value); }
        public string StillId { get => stillId; set => Set(ref stillId, value); }
        public string VisualMode { get => visualMode; set => Set(ref visualMode, value); }
        public string ExpressionId { get => expressionId; set => Set(ref expressionId, value); }
        public int AffectionChange { get => affectionChange; set => Set(ref affectionChange, value); }
        public string[] UnlockedOutfitIds
        {
            get => unlockedOutfitIds;
            set
            {
                unlockedOutfitIds = value ?? new string[0];
                OnPropertyChanged();
                OnPropertyChanged(nameof(UnlockedOutfitIdsText));
            }
        }

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

        private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }

    public class BattlePanelResultMessageEntry : ObservableObject
    {
        private string messageId = string.Empty;
        private string resultType = "Default";
        private string message = string.Empty;
        private string voiceId;

        public string MessageId { get => messageId; set => Set(ref messageId, value); }
        public string ResultType { get => resultType; set => Set(ref resultType, value); }
        public string Message { get => message; set => Set(ref message, value); }
        public string VoiceId { get => voiceId; set => Set(ref voiceId, value); }

        private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }

    public class SoloReturnReactionEntry : ObservableObject
    {
        private string reactionId = string.Empty;
        private string resultType = "SoloVictory";
        private string battleContextId = string.Empty;
        private string message = string.Empty;
        private string voiceId;
        private string stillId = string.Empty;
        private string visualMode = "Auto";
        private string expressionId = string.Empty;

        public string ReactionId { get => reactionId; set => Set(ref reactionId, value); }
        public string ResultType { get => resultType; set => Set(ref resultType, value); }
        public string BattleContextId { get => battleContextId; set => Set(ref battleContextId, value); }
        public string Message { get => message; set => Set(ref message, value); }
        public string VoiceId { get => voiceId; set => Set(ref voiceId, value); }
        public string StillId { get => stillId; set => Set(ref stillId, value); }
        public string VisualMode { get => visualMode; set => Set(ref visualMode, value); }
        public string ExpressionId { get => expressionId; set => Set(ref expressionId, value); }

        private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
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
