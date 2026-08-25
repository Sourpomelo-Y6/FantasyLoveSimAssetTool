using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationDraftSession
    {
        private readonly ConversationEntry sourceEntry;
        private readonly string sourcePrompt;

        public ConversationDraftSession(ConversationEntry entry, string prompt)
        {
            sourceEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            sourcePrompt = prompt ?? string.Empty;
        }

        public bool IsCurrent(ConversationEntry entry, string prompt) =>
            ReferenceEquals(sourceEntry, entry) && string.Equals(sourcePrompt, prompt ?? string.Empty, StringComparison.Ordinal);

        public bool TryApply(ConversationEntry entry, string prompt,
            IEnumerable<ConversationDraftLine> draftLines, bool replace)
        {
            if (!IsCurrent(entry, prompt)) return false;
            List<ConversationLine> lines = (draftLines ?? Array.Empty<ConversationDraftLine>())
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.Text))
                .Take(3)
                .Select(value => new ConversationLine
                {
                    Speaker = string.IsNullOrWhiteSpace(value.Speaker) ? "Heroine" : value.Speaker.Trim(),
                    Text = value.Text.Trim(),
                    Expression = value.ExpressionId?.Trim() ?? string.Empty
                }).ToList();
            if (lines.Count == 0) return false;
            sourceEntry.Lines ??= new ObservableCollection<ConversationLine>();
            if (replace) sourceEntry.Lines.Clear();
            foreach (ConversationLine line in lines) sourceEntry.Lines.Add(line);
            return true;
        }
    }
}
