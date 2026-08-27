using FantasyLoveSimAssetTool.Models;
using System;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class TrainingDialogueGenerationSession
    {
        private readonly TrainingDialogueEntry sourceEntry;
        private readonly TrainingDialogueMessage sourceMessage;
        private readonly string trainingId;
        private readonly string visualState;

        public TrainingDialogueGenerationSession(
            TrainingDialogueEntry entry, TrainingDialogueMessage message,
            string sourceTrainingId, string sourceVisualState)
        {
            sourceEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            sourceMessage = message;
            trainingId = sourceTrainingId ?? string.Empty;
            visualState = sourceVisualState ?? string.Empty;
        }

        public bool IsCurrent(TrainingDialogueEntry entry, string currentTrainingId, string currentVisualState) =>
            ReferenceEquals(sourceEntry, entry) &&
            string.Equals(trainingId, currentTrainingId ?? string.Empty, StringComparison.Ordinal) &&
            string.Equals(visualState, currentVisualState ?? string.Empty, StringComparison.Ordinal);

        public TrainingDialogueMessage TryAdd(
            TrainingDialogueEntry entry, string currentTrainingId, string currentVisualState, string text)
        {
            if (!IsCurrent(entry, currentTrainingId, currentVisualState) || string.IsNullOrWhiteSpace(text)) return null;
            entry.Messages ??= new System.Collections.ObjectModel.ObservableCollection<TrainingDialogueMessage>();
            var message = new TrainingDialogueMessage { Text = text.Trim() };
            entry.Messages.Add(message);
            return message;
        }

        public bool TryReplace(
            TrainingDialogueEntry entry, TrainingDialogueMessage message,
            string currentTrainingId, string currentVisualState, string text)
        {
            if (!IsCurrent(entry, currentTrainingId, currentVisualState) ||
                !ReferenceEquals(sourceMessage, message) || string.IsNullOrWhiteSpace(text)) return false;
            sourceMessage.Text = text.Trim();
            return true;
        }
    }
}
