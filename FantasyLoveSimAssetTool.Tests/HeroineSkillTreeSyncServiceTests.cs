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
        public void ExportAndImport_RoundTripsTrainingSkillsNodesAndConditions()
        {
            HeroineProfile source = Profile();
            string json = HeroineSkillTreeSyncService.BuildExportJson(source);
            HeroineProfile target = new HeroineProfile { HeroineId = "TestHeroine" };

            HeroineSkillTreeSyncService.ApplyImportedValues(target, HeroineSkillTreeSyncService.Deserialize(json));

            HeroineTrainingSkill skill = target.HeroineSkillTree.TrainingSkills.Single();
            Assert.AreEqual("training_care", skill.SkillId);
            Assert.AreEqual("TrainingCategory", skill.ApplicationScope);
            Assert.AreEqual("Cooperative", skill.ApplicationTargetId);
            HeroineSkillTreeNode node = target.HeroineSkillTree.Nodes.Single();
            Assert.AreEqual("node_care", node.NodeId);
            Assert.AreEqual("training_care", node.TrainingSkillId);
            Assert.AreEqual("CooperativeDrill", node.UnlockedTrainingIds.Single());
            Assert.AreEqual("TrainingCount", node.UnlockConditions.Single().ConditionType);
            Assert.AreEqual(10, node.UnlockConditions.Single().RequiredValue);
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
                            SkillId = "training_care",
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
                            NodeId = "node_care",
                            DisplayName = "気遣い",
                            TrainingSkillId = "training_care",
                            SkillPointCost = 2,
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
