using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class ConversationChoiceValidationService
    {
        public const int ChoiceTextMaxLength = 40;
        public const int ResponseTextMaxLength = 160;
        public const int AffectionChangeMin = -9999;
        public const int AffectionChangeMax = 9999;

        public static IReadOnlyList<string> Evaluate(
            ConversationChoice choice, IEnumerable<ConversationChoice> choices)
        {
            var warnings = new List<string>();
            if (choice == null)
            {
                warnings.Add("選択肢が空です");
                return warnings;
            }
            string choiceText = (choice.ChoiceText ?? string.Empty).Trim();
            string responseText = (choice.ResponseText ?? string.Empty).Trim();
            if (choiceText.Length == 0) warnings.Add("選択肢文言が空です");
            else if (choiceText.Length > ChoiceTextMaxLength)
                warnings.Add($"選択肢文言が{ChoiceTextMaxLength}文字を超えています");
            if (responseText.Length == 0) warnings.Add("ヒロインの返答が空です");
            else if (responseText.Length > ResponseTextMaxLength)
                warnings.Add($"ヒロインの返答が{ResponseTextMaxLength}文字を超えています");
            if (choice.AffectionChange.HasValue &&
                (choice.AffectionChange.Value < AffectionChangeMin ||
                 choice.AffectionChange.Value > AffectionChangeMax))
                warnings.Add($"好感度変化は{AffectionChangeMin}～{AffectionChangeMax}で指定してください");
            if (choiceText.Length > 0 && (choices ?? Array.Empty<ConversationChoice>())
                .Any(other => other != null && !ReferenceEquals(other, choice) &&
                    string.Equals((other.ChoiceText ?? string.Empty).Trim(), choiceText,
                        StringComparison.OrdinalIgnoreCase)))
                warnings.Add("選択肢文言が重複しています");
            return warnings;
        }
    }
}
