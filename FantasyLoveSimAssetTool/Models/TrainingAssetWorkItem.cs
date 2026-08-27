using FantasyLoveSimAssetTool.Common;
using FantasyLoveSimAssetTool.Services;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Models
{
    public sealed class TrainingAssetWorkItem : ObservableObject, IDisposable
    {
        private static readonly string[] VisualStates =
        {
            "SelectedBeforeFirstStep", "SelectedAfterFirstStep", "PlayerLpConsumed",
            "HeroineLpConsumed", "SimultaneousLpConsumed"
        };
        private readonly TrainingDialogueEntry dialogueEntry;

        public TrainingAssetWorkItem(HeroineAsset asset, string trainingDisplayName,
            TrainingDialogueEntry entry)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
            TrainingDisplayName = trainingDisplayName ?? string.Empty;
            dialogueEntry = entry;
            if (dialogueEntry?.Messages != null)
            {
                dialogueEntry.Messages.CollectionChanged += MessagesCollectionChanged;
                foreach (TrainingDialogueMessage message in dialogueEntry.Messages.Where(x => x != null))
                    message.PropertyChanged += MessagePropertyChanged;
            }
        }

        public event EventHandler ProgressChanged;
        public HeroineAsset Asset { get; }
        public string AssetId => Asset.AssetId;
        public AssetStatus Status => Asset.Status;
        public string TrainingId => Parse(AssetId).trainingId;
        public string VisualState => Parse(AssetId).visualState;
        public string TrainingDisplayName { get; }
        public string VisualStateDisplayName => TrainingDialogueGenerationService.FormatVisualState(VisualState);
        public int DialogueMessageCount => dialogueEntry?.Messages?.Count(x => x != null && !string.IsNullOrWhiteSpace(x.Text)) ?? 0;
        public int MissingVoiceIdCount => dialogueEntry?.Messages?.Count(x => x != null &&
            !string.IsNullOrWhiteSpace(x.Text) && string.IsNullOrWhiteSpace(x.VoiceId)) ?? 0;
        public bool IsDialogueMissing => DialogueMessageCount == 0;
        public string DialogueProgressSummary => IsDialogueMissing
            ? "セリフ未入力"
            : MissingVoiceIdCount > 0
                ? $"セリフ {DialogueMessageCount}件 / Voice未設定 {MissingVoiceIdCount}件"
                : $"セリフ {DialogueMessageCount}件 / Voice設定済み";

        public static (string trainingId, string visualState) Parse(string assetId)
        {
            string value = assetId ?? string.Empty;
            if (!value.StartsWith("Training_", StringComparison.Ordinal)) return (string.Empty, string.Empty);
            foreach (string state in VisualStates.OrderByDescending(x => x.Length))
            {
                string suffix = "_" + state;
                if (value.EndsWith(suffix, StringComparison.Ordinal))
                    return (value.Substring("Training_".Length,
                        value.Length - "Training_".Length - suffix.Length), state);
            }
            return (string.Empty, string.Empty);
        }

        private void MessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (TrainingDialogueMessage message in e.OldItems.OfType<TrainingDialogueMessage>())
                    message.PropertyChanged -= MessagePropertyChanged;
            if (e.NewItems != null)
                foreach (TrainingDialogueMessage message in e.NewItems.OfType<TrainingDialogueMessage>())
                    message.PropertyChanged += MessagePropertyChanged;
            NotifyProgressChanged();
        }

        private void MessagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingDialogueMessage.Text) ||
                e.PropertyName == nameof(TrainingDialogueMessage.VoiceId)) NotifyProgressChanged();
        }

        private void NotifyProgressChanged()
        {
            OnPropertyChanged(nameof(DialogueMessageCount));
            OnPropertyChanged(nameof(MissingVoiceIdCount));
            OnPropertyChanged(nameof(IsDialogueMissing));
            OnPropertyChanged(nameof(DialogueProgressSummary));
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (dialogueEntry?.Messages == null) return;
            dialogueEntry.Messages.CollectionChanged -= MessagesCollectionChanged;
            foreach (TrainingDialogueMessage message in dialogueEntry.Messages.Where(x => x != null))
                message.PropertyChanged -= MessagePropertyChanged;
        }
    }
}
