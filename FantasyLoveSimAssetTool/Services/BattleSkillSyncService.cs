using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Services
{
    public static class BattleSkillSyncService
    {
        public static IReadOnlyList<HeroineBattleSkill> CreateExportValues(HeroineProfile profile)
        {
            return profile != null && profile.BattleSkillsSpecified
                ? Normalize(profile.BattleSkills)
                : null;
        }

        public static void ApplyImportedValues(HeroineProfile profile, IEnumerable<HeroineBattleSkill> source)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (source == null) return;

            profile.BattleSkillsSpecified = true;
            profile.BattleSkills ??= new ObservableCollection<HeroineBattleSkill>();
            profile.BattleSkills.Clear();
            foreach (HeroineBattleSkill skill in Normalize(source))
            {
                profile.BattleSkills.Add(skill);
            }
        }

        public static List<HeroineBattleSkill> Normalize(IEnumerable<HeroineBattleSkill> source)
        {
            List<HeroineBattleSkill> result = new List<HeroineBattleSkill>();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (source == null) return result;
            foreach (HeroineBattleSkill item in source)
            {
                string skillId = (item?.SkillId ?? string.Empty).Trim();
                if (skillId.Length == 0 || !ids.Add(skillId)) continue;
                result.Add(new HeroineBattleSkill
                {
                    SkillId = skillId,
                    DisplayName = (item.DisplayName ?? string.Empty).Trim(),
                    EffectType = (item.EffectType ?? string.Empty).Trim(),
                    Target = (item.Target ?? string.Empty).Trim(),
                    Cost = item.Cost,
                    Power = item.Power,
                    AffectedStat = (item.AffectedStat ?? string.Empty).Trim(),
                    StatusDurationTurns = item.StatusDurationTurns,
                    UseChancePercent = item.UseChancePercent,
                    Priority = item.Priority,
                    MaxUsesPerBattle = item.MaxUsesPerBattle
                });
            }
            return result;
        }
    }
}
