using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class SkillTextCandidateQualityService
    {
        public static IReadOnlyList<string> Evaluate(
            string text,
            ShortTextGenerationTarget target,
            IEnumerable<string> existingNames,
            string currentName,
            string technicalContext,
            string effectType)
        {
            var warnings = new List<string>();
            string value = (text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return warnings;

            if (IsNameTarget(target) && (existingNames ?? Enumerable.Empty<string>()).Any(name =>
                !string.IsNullOrWhiteSpace(name) &&
                !string.Equals(name.Trim(), currentName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(name.Trim(), value, StringComparison.OrdinalIgnoreCase)))
                warnings.Add("既存名と重複");

            string leakedToken = FindLeakedTechnicalToken(value, technicalContext);
            if (!string.IsNullOrEmpty(leakedToken)) warnings.Add("内部設定値を含む: " + leakedToken);
            if (ContainsAny(value, "プロンプト", "生成AI", "言語モデル", "キャラクター設定"))
                warnings.Add("制作メモの混入を確認");

            if (target?.RequiredContext == "BattleSkill")
            {
                if (effectType == "Damage" && ContainsAny(value, "回復", "治癒")) warnings.Add("Damage効果と表現が不一致");
                else if (effectType == "Heal" && ContainsAny(value, "攻撃", "ダメージ", "弱体")) warnings.Add("Heal効果と表現が不一致");
                else if (effectType == "Buff" && ContainsAny(value, "弱体", "低下", "減少")) warnings.Add("Buff効果と表現が不一致");
                else if (effectType == "Debuff" && ContainsAny(value, "強化", "上昇", "増加")) warnings.Add("Debuff効果と表現が不一致");
            }
            return warnings;
        }

        public static bool AreTooSimilar(string left, string right)
        {
            string a = Normalize(left);
            string b = Normalize(right);
            if (a.Length < 2 || b.Length < 2) return string.Equals(a, b, StringComparison.Ordinal);
            if (a.Contains(b) || b.Contains(a)) return true;
            HashSet<string> aPairs = Bigrams(a);
            HashSet<string> bPairs = Bigrams(b);
            int union = aPairs.Union(bPairs).Count();
            return union > 0 && (double)aPairs.Intersect(bPairs).Count() / union >= 0.6;
        }

        private static bool IsNameTarget(ShortTextGenerationTarget target) =>
            target?.Id == "BattleSkillDisplayName" || target?.Id == "TrainingSkillDisplayName" ||
            target?.Id == "SkillTreeNodeDisplayName";

        private static string FindLeakedTechnicalToken(string text, string context)
        {
            foreach (string token in (context ?? string.Empty).Split(new[] { ';', '=', ',', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string value = token.Trim();
                if (value.Length >= 4 && value.Any(char.IsLetter) && value.Any(c => c == '_' || char.IsUpper(c)) &&
                    text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0) return value;
            }
            return string.Empty;
        }

        private static bool ContainsAny(string value, params string[] terms) => terms.Any(value.Contains);
        private static string Normalize(string value) => new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        private static HashSet<string> Bigrams(string value) => new HashSet<string>(Enumerable.Range(0, value.Length - 1).Select(i => value.Substring(i, 2)), StringComparer.Ordinal);
    }
}
