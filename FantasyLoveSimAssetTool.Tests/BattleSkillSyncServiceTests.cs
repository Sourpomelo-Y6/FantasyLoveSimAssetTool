using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class BattleSkillSyncServiceTests
    {
        [TestMethod]
        public void CreateExportValues_ReturnsNullWhenFieldWasNotSpecified()
        {
            Assert.IsNull(BattleSkillSyncService.CreateExportValues(new HeroineProfile { BattleSkillsSpecified = false }));
        }

        [TestMethod]
        public void ApplyImportedValues_PreservesExistingSkillsWhenFieldIsMissing()
        {
            HeroineProfile profile = ProfileWith(Skill("existing", "Damage", 3));

            BattleSkillSyncService.ApplyImportedValues(profile, null);

            Assert.AreEqual("existing", profile.BattleSkills.Single().SkillId);
        }

        [TestMethod]
        public void ApplyImportedValues_ClearsSkillsForExplicitEmptyArray()
        {
            HeroineProfile profile = ProfileWith(Skill("existing", "Damage", 3));

            BattleSkillSyncService.ApplyImportedValues(profile, new HeroineBattleSkill[0]);

            Assert.IsTrue(profile.BattleSkillsSpecified);
            Assert.AreEqual(0, profile.BattleSkills.Count);
        }

        [TestMethod]
        public void Normalize_PreservesAllFieldsOrderAndUnknownEnumText()
        {
            HeroineBattleSkill first = Skill(" skill_b ", "FutureEffect", 7);
            first.DisplayName = " Skill B ";
            first.Target = "FutureTarget";
            first.AffectedStat = "FutureStat";
            first.Power = -4;
            first.StatusDurationTurns = 6;
            first.UseChancePercent = 123;
            first.Priority = -2;
            first.MaxUsesPerBattle = 0;

            List<HeroineBattleSkill> result = BattleSkillSyncService.Normalize(new[]
            {
                first,
                Skill("skill_a", "Heal", 2),
                Skill("SKILL_B", "Damage", 9),
                Skill(" ", "Damage", 1)
            });

            CollectionAssert.AreEqual(new[] { "skill_b", "skill_a" }, result.Select(x => x.SkillId).ToArray());
            HeroineBattleSkill exported = result[0];
            Assert.AreEqual("Skill B", exported.DisplayName);
            Assert.AreEqual("FutureEffect", exported.EffectType);
            Assert.AreEqual("FutureTarget", exported.Target);
            Assert.AreEqual("FutureStat", exported.AffectedStat);
            Assert.AreEqual(7, exported.Cost);
            Assert.AreEqual(-4, exported.Power);
            Assert.AreEqual(6, exported.StatusDurationTurns);
            Assert.AreEqual(123, exported.UseChancePercent);
            Assert.AreEqual(-2, exported.Priority);
            Assert.AreEqual(0, exported.MaxUsesPerBattle);
        }

        [TestMethod]
        public void ExportAndImport_RoundTripsMultipleSkillsWithoutDuplication()
        {
            HeroineProfile source = ProfileWith(Skill("skill_b", "Buff", 4), Skill("skill_a", "Heal", 2));
            HeroineProfile target = new HeroineProfile();

            BattleSkillSyncService.ApplyImportedValues(target, BattleSkillSyncService.CreateExportValues(source));
            BattleSkillSyncService.ApplyImportedValues(target, BattleSkillSyncService.CreateExportValues(target));

            CollectionAssert.AreEqual(new[] { "skill_b", "skill_a" }, target.BattleSkills.Select(x => x.SkillId).ToArray());
            Assert.AreEqual("Buff", target.BattleSkills[0].EffectType);
            Assert.AreEqual(4, target.BattleSkills[0].Cost);
        }

        private static HeroineProfile ProfileWith(params HeroineBattleSkill[] skills)
        {
            return new HeroineProfile
            {
                BattleSkillsSpecified = true,
                BattleSkills = new ObservableCollection<HeroineBattleSkill>(skills)
            };
        }

        private static HeroineBattleSkill Skill(string id, string effect, int cost)
        {
            return new HeroineBattleSkill { SkillId = id, EffectType = effect, Cost = cost };
        }
    }
}
