using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class RequiredSkillIdSyncServiceTests
    {
        [TestMethod]
        public void CreateExportValues_ReturnsNullWhenFieldWasNotSpecified()
        {
            ConversationCondition condition = new ConversationCondition
            {
                RequiredSkillIdsSpecified = false,
                RequiredSkillIdsText = "keep_internal_value"
            };

            Assert.IsNull(RequiredSkillIdSyncService.CreateExportValues(condition));
        }

        [TestMethod]
        public void CreateExportValues_ReturnsEmptyListForExplicitEmptyCondition()
        {
            ConversationCondition condition = new ConversationCondition
            {
                RequiredSkillIdsSpecified = true,
                RequiredSkillIdsText = "  "
            };

            Assert.AreEqual(0, RequiredSkillIdSyncService.CreateExportValues(condition).Count);
        }

        [TestMethod]
        public void CreateExportValues_NormalizesDuplicatesAndPreservesOrder()
        {
            ConversationCondition condition = new ConversationCondition
            {
                RequiredSkillIdsSpecified = true,
                RequiredSkillIdsText = " skill_b, skill_a\nskill_B; unknown_skill "
            };

            CollectionAssert.AreEqual(
                new[] { "skill_b", "skill_a", "unknown_skill" },
                RequiredSkillIdSyncService.CreateExportValues(condition).ToArray());
        }

        [TestMethod]
        public void ApplyImportedValues_LeavesExistingConditionWhenFieldIsMissing()
        {
            ConversationCondition condition = new ConversationCondition
            {
                RequiredSkillIdsSpecified = true,
                RequiredSkillIdsText = "existing_skill"
            };

            RequiredSkillIdSyncService.ApplyImportedValues(condition, null);

            Assert.IsTrue(condition.RequiredSkillIdsSpecified);
            Assert.AreEqual("existing_skill", condition.RequiredSkillIdsText);
        }

        [TestMethod]
        public void ApplyImportedValues_ClearsConditionForExplicitEmptyArray()
        {
            ConversationCondition condition = new ConversationCondition
            {
                RequiredSkillIdsSpecified = false,
                RequiredSkillIdsText = "existing_skill"
            };

            RequiredSkillIdSyncService.ApplyImportedValues(condition, new string[0]);

            Assert.IsTrue(condition.RequiredSkillIdsSpecified);
            Assert.AreEqual(string.Empty, condition.RequiredSkillIdsText);
        }

        [TestMethod]
        public void ExportAndImport_RoundTripsWithoutDuplicatingUnknownIds()
        {
            ConversationCondition source = new ConversationCondition
            {
                RequiredSkillIdsSpecified = true,
                RequiredSkillIdsText = "skill_b\nskill_a\nunknown_skill\nskill_b"
            };
            ConversationCondition target = new ConversationCondition();

            RequiredSkillIdSyncService.ApplyImportedValues(
                target,
                RequiredSkillIdSyncService.CreateExportValues(source));
            RequiredSkillIdSyncService.ApplyImportedValues(
                target,
                RequiredSkillIdSyncService.CreateExportValues(target));

            Assert.IsTrue(target.RequiredSkillIdsSpecified);
            CollectionAssert.AreEqual(
                new[] { "skill_b", "skill_a", "unknown_skill" },
                RequiredSkillIdSyncService.NormalizeText(target.RequiredSkillIdsText).ToArray());
        }
    }
}
