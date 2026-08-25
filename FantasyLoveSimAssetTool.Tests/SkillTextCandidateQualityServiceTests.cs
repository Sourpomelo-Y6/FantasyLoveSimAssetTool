using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class SkillTextCandidateQualityServiceTests
    {
        [TestMethod]
        public void Evaluate_NameTargetWarnsForExistingNameAndTechnicalId()
        {
            var target = new ShortTextGenerationTarget(
                "TrainingSkillDisplayName", "訓練スキル名", "表示名", 2, 18,
                requiredContext: "TrainingSkill");

            var duplicate = SkillTextCandidateQualityService.Evaluate(
                "集中訓練", target, new[] { "集中訓練", "基礎訓練" }, "現在名",
                "SkillId=Heroine_TrainingSkill; Scope=Training", string.Empty);
            var leaked = SkillTextCandidateQualityService.Evaluate(
                "Heroine_TrainingSkill", target, new string[0], string.Empty,
                "SkillId=Heroine_TrainingSkill; Scope=Training", string.Empty);

            Assert.IsTrue(duplicate.Contains("既存名と重複"));
            Assert.IsTrue(leaked.Any(x => x.Contains("内部設定値")));
        }

        [TestMethod]
        public void Evaluate_BattleSkillWarnsForContradictingEffectWords()
        {
            var target = new ShortTextGenerationTarget(
                "BattleSkillDisplayName", "戦闘スキル名", "表示名", 2, 18,
                requiredContext: "BattleSkill");

            var warnings = SkillTextCandidateQualityService.Evaluate(
                "癒やしの回復術", target, new string[0], string.Empty,
                "SkillId=Heroine_Attack; Effect=Damage", "Damage");

            Assert.IsTrue(warnings.Contains("Damage効果と表現が不一致"));
        }

        [TestMethod]
        public void AreTooSimilar_DetectsMinorVariationButNotDifferentNames()
        {
            Assert.IsTrue(SkillTextCandidateQualityService.AreTooSimilar("集中訓練・改", "集中訓練"));
            Assert.IsFalse(SkillTextCandidateQualityService.AreTooSimilar("炎の一撃", "癒やしの祈り"));
        }

        [TestMethod]
        public void TextGenerationCandidate_CombinesLengthAndQualityWarnings()
        {
            var candidate = new TextGenerationCandidate("短い", 15, 50, warnings: new[] { "既存名と重複" });

            Assert.IsTrue(candidate.HasWarning);
            StringAssert.Contains(candidate.ValidationMessage, "短め");
            StringAssert.Contains(candidate.ValidationMessage, "既存名と重複");
        }
    }
}
