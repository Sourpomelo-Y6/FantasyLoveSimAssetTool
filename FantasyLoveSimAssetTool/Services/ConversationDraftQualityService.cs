using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class ConversationDraftQualityService
    {
        private const int RecommendedMaximumLength = 120;

        public static int Evaluate(
            IReadOnlyList<ConversationDraftLine> draftLines,
            IEnumerable<ConversationEntry> existingEntries,
            ConversationEntry targetEntry,
            ConversationSituationPrompt situation,
            ConversationCharacterPrompt character,
            IEnumerable<string> allowedExpressionIds)
        {
            if (draftLines == null) return 0;
            var expressions = new HashSet<string>(
                (allowedExpressionIds ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
                StringComparer.Ordinal);
            List<string> existingTexts = (existingEntries ?? Enumerable.Empty<ConversationEntry>())
                .Where(entry => entry != null && !ReferenceEquals(entry, targetEntry))
                .SelectMany(entry => entry.Lines ?? Enumerable.Empty<ConversationLine>())
                .Select(line => line?.Text?.Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text)).ToList();
            List<string> technicalTokens = BuildTechnicalTokens(targetEntry);
            int warningCount = 0;

            for (int index = 0; index < draftLines.Count; index++)
            {
                ConversationDraftLine line = draftLines[index];
                var warnings = new List<string>();
                string text = line?.Text?.Trim() ?? string.Empty;
                if (text.Length == 0) warnings.Add("本文が空");
                if (text.Length > RecommendedMaximumLength)
                    warnings.Add($"本文が長い（{text.Length}/{RecommendedMaximumLength}文字）");
                if (line != null && line.Speaker != "Heroine" && line.Speaker != "Player" && line.Speaker != "System")
                    warnings.Add("話者が候補外");
                if (line != null && !string.IsNullOrWhiteSpace(line.ExpressionId) &&
                    (line.Speaker != "Heroine" || !expressions.Contains(line.ExpressionId.Trim())))
                    warnings.Add("表情IDが候補外");
                if (existingTexts.Any(existing => string.Equals(existing, text, StringComparison.Ordinal)))
                    warnings.Add("既存台詞と重複");
                else if (existingTexts.Any(existing => SkillTextCandidateQualityService.AreTooSimilar(existing, text)))
                    warnings.Add("既存台詞と類似");
                if (draftLines.Where((_, other) => other != index).Any(other =>
                    SkillTextCandidateQualityService.AreTooSimilar(other?.Text, text)))
                    warnings.Add("下書き内の別行と類似");
                if (ContainsAny(text, "プロンプト", "生成AI", "言語モデル", "キャラクター設定", "出力形式", "expressionId"))
                    warnings.Add("制作指示の混入を確認");
                string leakedToken = technicalTokens.FirstOrDefault(token =>
                    text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrEmpty(leakedToken)) warnings.Add("内部IDを含む: " + leakedToken);
                string avoided = (character?.AvoidExpressions ?? new List<string>()).FirstOrDefault(value =>
                    !string.IsNullOrWhiteSpace(value) && text.Contains(value.Trim()));
                if (!string.IsNullOrEmpty(avoided)) warnings.Add("禁止表現を含む: " + avoided.Trim());
                if (line != null) line.WarningText = string.Join(" / ", warnings.Distinct());
                warningCount += warnings.Distinct().Count();
            }

            if (targetEntry != null && situation != null &&
                !string.IsNullOrWhiteSpace(situation.Category) &&
                !string.Equals(targetEntry.Category?.Trim(), situation.Category.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                string warning = $"状況カテゴリ `{situation.Category}` と会話カテゴリ `{targetEntry.Category}` が不一致";
                if (draftLines.Count > 0)
                    draftLines[0].WarningText = JoinWarning(draftLines[0].WarningText, warning);
                warningCount++;
            }
            return warningCount;
        }

        private static List<string> BuildTechnicalTokens(ConversationEntry entry)
        {
            var values = new[]
            {
                entry?.Id, entry?.Conditions?.LocationId, entry?.Conditions?.ActionId,
                entry?.Conditions?.RequiredItemId, entry?.Conditions?.TriggerContextId
            };
            return values.Where(value => !string.IsNullOrWhiteSpace(value) && value.Trim().Length >= 4)
                .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool ContainsAny(string text, params string[] values) =>
            values.Any(value => text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);

        private static string JoinWarning(string current, string addition) =>
            string.IsNullOrWhiteSpace(current) ? addition : current + " / " + addition;
    }
}
