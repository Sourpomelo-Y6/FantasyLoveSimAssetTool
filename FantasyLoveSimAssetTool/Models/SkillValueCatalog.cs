using System;
using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public static class SkillValueCatalog
    {
        public static IReadOnlyList<string> BattleEffectTypes { get; } =
            new[] { "None", "Damage", "Heal", "Guard", "Buff", "Debuff" };

        public static IReadOnlyList<string> BattleTargets { get; } =
            new[] { "Enemy", "Self", "Player", "LowestHpAlly" };

        public static IReadOnlyList<string> BattleStats { get; } =
            new[] { "Attack", "Defense", "Speed" };

        public static IReadOnlyList<string> TrainingApplicationScopes { get; } =
            new[] { "AllTrainings", "TrainingCategory", "Training" };

        public static bool Contains(IReadOnlyList<string> values, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            foreach (string candidate in values)
                if (string.Equals(candidate, value.Trim(), StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
