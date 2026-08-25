using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class HeroineSkillTreeSyncServiceTests
    {
        [TestMethod]
        public void TrainingSkill_AllTrainingsClearsTargetId()
        {
            HeroineTrainingSkill skill = new HeroineTrainingSkill
            {
                ApplicationScope = "Training",
                ApplicationTargetId = "TrainingA"
            };

            skill.ApplicationScope = "AllTrainings";

            Assert.AreEqual(string.Empty, skill.ApplicationTargetId);
        }

        [TestMethod]
        public void SkillTreeCondition_RequiredTypesNormalizeScopeAndTarget()
        {
            HeroineSkillTreeCondition condition = new HeroineSkillTreeCondition
            {
                Scope = "Training",
                TargetId = "TrainingA"
            };

            condition.ConditionType = "Affection";

            Assert.AreEqual("Total", condition.Scope);
            Assert.AreEqual(string.Empty, condition.TargetId);

            condition.ConditionType = "TrainingProficiency";
            Assert.AreEqual("Training", condition.Scope);
        }

        [TestMethod]
        public void ExportAndImport_RoundTripsTrainingSkillsNodesAndConditions()
        {
            HeroineProfile source = Profile();
            string json = HeroineSkillTreeSyncService.BuildExportJson(source);
            HeroineProfile target = new HeroineProfile { HeroineId = "TestHeroine" };

            HeroineSkillTreeSyncService.ApplyImportedValues(target, HeroineSkillTreeSyncService.Deserialize(json));

            HeroineTrainingSkill skill = target.HeroineSkillTree.TrainingSkills.Single();
            Assert.AreEqual("TestHeroine_training_care", skill.SkillId);
            Assert.AreEqual("TrainingCategory", skill.ApplicationScope);
            Assert.AreEqual("Cooperative", skill.ApplicationTargetId);
            HeroineSkillTreeNode node = target.HeroineSkillTree.Nodes.Single();
            Assert.AreEqual("TestHeroine_node_care", node.NodeId);
            Assert.AreEqual("TestHeroine_training_care", node.TrainingSkillId);
            Assert.AreEqual("CooperativeDrill", node.UnlockedTrainingIds.Single());
            Assert.AreEqual("Manual_Care_01", node.UnlockEventId);
            Assert.AreEqual("TrainingCount", node.UnlockConditions.Single().ConditionType);
            Assert.AreEqual(10, node.UnlockConditions.Single().RequiredValue);
        }

        [TestMethod]
        public void ExportAndImport_PreservesEveryFieldMultipleConditionsAndEmptyValues()
        {
            HeroineProfile source = Profile();
            HeroineTrainingSkill skill = source.HeroineSkillTree.TrainingSkills.Single();
            skill.Description = "説明";
            skill.SortOrder = 7;
            skill.IsEnabled = false;
            skill.HeroineHpCostReduction = 3;
            skill.AffectionRewardModifier = -4;
            skill.ProficiencyRewardModifier = 5;
            HeroineSkillTreeNode node = source.HeroineSkillTree.Nodes.Single();
            node.GrantedHeroineSkillId = "BattleSkillA";
            node.SortOrder = 9;
            node.PrerequisiteNodeIds.Add("TestHeroine_Root");
            node.UnlockConditions.Add(new HeroineSkillTreeCondition
            {
                ConditionType = "Day",
                Scope = "Total",
                TargetId = string.Empty,
                RequiredValue = 20
            });
            node.TreePositionX = 120.5f;
            node.TreePositionY = -36.25f;
            HeroineProfile target = new HeroineProfile { HeroineId = source.HeroineId };

            HeroineSkillTreeSyncService.ApplyImportedValues(target,
                HeroineSkillTreeSyncService.Deserialize(HeroineSkillTreeSyncService.BuildExportJson(source)));

            HeroineTrainingSkill importedSkill = target.HeroineSkillTree.TrainingSkills.Single();
            Assert.AreEqual("説明", importedSkill.Description);
            Assert.AreEqual(7, importedSkill.SortOrder);
            Assert.IsFalse(importedSkill.IsEnabled);
            Assert.AreEqual(1, importedSkill.PlayerHpCostReduction);
            Assert.AreEqual(3, importedSkill.HeroineHpCostReduction);
            Assert.AreEqual(-4, importedSkill.AffectionRewardModifier);
            Assert.AreEqual(5, importedSkill.ProficiencyRewardModifier);
            Assert.AreEqual("TrainingCategory", importedSkill.ApplicationScope);
            Assert.AreEqual("Cooperative", importedSkill.ApplicationTargetId);
            HeroineSkillTreeNode importedNode = target.HeroineSkillTree.Nodes.Single();
            Assert.AreEqual("BattleSkillA", importedNode.GrantedHeroineSkillId);
            Assert.AreEqual(9, importedNode.SortOrder);
            Assert.AreEqual(2, importedNode.SkillPointCost);
            CollectionAssert.AreEqual(new[] { "TestHeroine_Root" }, importedNode.PrerequisiteNodeIds.ToArray());
            CollectionAssert.AreEqual(new[] { "CooperativeDrill" }, importedNode.UnlockedTrainingIds.ToArray());
            Assert.AreEqual("Manual_Care_01", importedNode.UnlockEventId);
            Assert.AreEqual(2, importedNode.UnlockConditions.Count);
            Assert.AreEqual("Day", importedNode.UnlockConditions[1].ConditionType);
            Assert.AreEqual(string.Empty, importedNode.UnlockConditions[1].TargetId);
            Assert.AreEqual(120.5f, importedNode.TreePositionX);
            Assert.AreEqual(-36.25f, importedNode.TreePositionY);
        }

        [TestMethod]
        public void ApplyImportedValues_MissingArraysPreserveExistingValues()
        {
            HeroineProfile profile = Profile();
            HeroineSkillsDataFile data = HeroineSkillTreeSyncService.Deserialize(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\"}");

            HeroineSkillTreeSyncService.ApplyImportedValues(profile, data);

            Assert.AreEqual(1, profile.HeroineSkillTree.TrainingSkills.Count);
            Assert.AreEqual(1, profile.HeroineSkillTree.Nodes.Count);
        }

        [TestMethod]
        public void ApplyImportedValues_ExplicitEmptyArraysClearValues()
        {
            HeroineProfile profile = Profile();
            HeroineSkillsDataFile data = HeroineSkillTreeSyncService.Deserialize(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"trainingSkills\":[],\"nodes\":[]}");

            HeroineSkillTreeSyncService.ApplyImportedValues(profile, data);

            Assert.AreEqual(0, profile.HeroineSkillTree.TrainingSkills.Count);
            Assert.AreEqual(0, profile.HeroineSkillTree.Nodes.Count);
        }

        [TestMethod]
        public void ApplyImportedValues_RejectsDifferentHeroine()
        {
            HeroineProfile profile = Profile();
            HeroineSkillsDataFile data = new HeroineSkillsDataFile { HeroineId = "OtherHeroine" };

            Assert.ThrowsException<System.InvalidOperationException>(() =>
                HeroineSkillTreeSyncService.ApplyImportedValues(profile, data));
        }

        private static HeroineProfile Profile()
        {
            return new HeroineProfile
            {
                HeroineId = "TestHeroine",
                HeroineSkillTree = new HeroineSkillTreeSettings
                {
                    TrainingSkills = new ObservableCollection<HeroineTrainingSkill>
                    {
                        new HeroineTrainingSkill
                        {
                            SkillId = "TestHeroine_training_care",
                            DisplayName = "気遣い",
                            PlayerHpCostReduction = 1,
                            ApplicationScope = "TrainingCategory",
                            ApplicationTargetId = "Cooperative"
                        }
                    },
                    Nodes = new ObservableCollection<HeroineSkillTreeNode>
                    {
                        new HeroineSkillTreeNode
                        {
                            NodeId = "TestHeroine_node_care",
                            DisplayName = "気遣い",
                            TrainingSkillId = "TestHeroine_training_care",
                            SkillPointCost = 2,
                            UnlockEventId = "Manual_Care_01",
                            UnlockedTrainingIds = new ObservableCollection<string> { "CooperativeDrill" },
                            UnlockConditions = new ObservableCollection<HeroineSkillTreeCondition>
                            {
                                new HeroineSkillTreeCondition
                                {
                                    ConditionType = "TrainingCount",
                                    Scope = "TrainingCategory",
                                    TargetId = "Cooperative",
                                    RequiredValue = 10
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
