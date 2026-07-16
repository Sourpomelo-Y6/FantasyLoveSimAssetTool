using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class RequiredSkillIdSyncService
    {
        public static IReadOnlyList<string> CreateExportValues(ConversationCondition condition)
        {
            return condition != null && condition.RequiredSkillIdsSpecified
                ? NormalizeText(condition.RequiredSkillIdsText)
                : null;
        }

        public static void ApplyImportedValues(ConversationCondition condition, IEnumerable<string> values)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (values == null)
            {
                return;
            }

            condition.RequiredSkillIdsSpecified = true;
            condition.RequiredSkillIdsText = string.Join(Environment.NewLine, NormalizeValues(values));
        }

        public static List<string> NormalizeText(string text)
        {
            return string.IsNullOrWhiteSpace(text)
                ? new List<string>()
                : NormalizeValues(text.Split(
                    new[] { '\r', '\n', ',', ';' },
                    StringSplitOptions.RemoveEmptyEntries));
        }

        private static List<string> NormalizeValues(IEnumerable<string> values)
        {
            return (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
