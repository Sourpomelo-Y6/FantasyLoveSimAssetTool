using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class CharacterProductionStatusServiceTests
    {
        [TestMethod]
        public void Evaluate_CompleteProfile_ReturnsCompleteForInitialThreeCategories()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Complete, row.BasicInformation.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.BattleMessages.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.TrainingImages.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.TrainingDialogues.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.CharacterImages.Kind);
            Assert.AreEqual(4, row.BasicInformation.Checks.Count);
            Assert.AreEqual(7, row.BattleMessages.Checks.Count);
            Assert.AreEqual(5, row.TrainingImages.Checks.Count);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Conversations.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Expressions.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Costumes.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.BattleSkills.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.SkillTree.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Events.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.ExportReadiness.Kind);
            Assert.IsFalse(row.HasIncomplete);
        }

        [TestMethod]
        public void Evaluate_PartialProfile_ExplainsMissingFieldsAndSlots()
        {
            HeroineProfile profile = CompleteProfile();
            profile.Personality = string.Empty;
            profile.BattleMessages.ResultEvents.RemoveAt(0);
            profile.Assets[0].Status = AssetStatus.Pending;

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.BasicInformation.Kind);
            StringAssert.Contains(row.BasicInformation.Details, "性格");
            Assert.AreEqual(ProductionStatusKind.Partial, row.BattleMessages.Kind);
            StringAssert.Contains(row.BattleMessages.Details, "SoloVictory");
            Assert.AreEqual(ProductionStatusKind.Partial, row.TrainingImages.Kind);
            StringAssert.Contains(row.TrainingImages.Details, "4/5");
            Assert.IsTrue(row.TrainingImages.Checks.Any(x => !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_EmptyProfile_ReturnsMissingWithoutThrowing()
        {
            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(new HeroineProfile());

            Assert.AreEqual("×", row.BasicInformation.Symbol);
            Assert.AreEqual(ProductionStatusKind.Missing, row.BattleMessages.Kind);
            Assert.AreEqual(ProductionStatusKind.Missing, row.TrainingImages.Kind);
        }

        [TestMethod]
        public void Evaluate_UnknownExpressionAndCostumeReferences_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            profile.ConversationEntries[0].Lines[0].Expression = "UnknownExpression";
            profile.ConversationEntries[0].Conditions.CostumeId = "UnknownCostume";

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.Expressions.Kind);
            Assert.IsTrue(row.Expressions.Checks.Any(x => x.Name.Contains("UnknownExpression") && !x.IsComplete));
            Assert.AreEqual(ProductionStatusKind.Partial, row.Costumes.Kind);
            Assert.IsTrue(row.Costumes.Checks.Any(x => x.Name.Contains("UnknownCostume") && !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_BrokenSkillTreeReferences_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            profile.HeroineSkillTree.Nodes[1].PrerequisiteNodeIds.Add("MissingNode");
            profile.HeroineSkillTree.Nodes[1].GrantedHeroineSkillId = "MissingBattleSkill";
            profile.HeroineSkillTree.Nodes[1].UnlockedTrainingIds.Add("MissingTraining");

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.SkillTree.Kind);
            Assert.IsTrue(row.SkillTree.Checks.Any(x => x.Details.Contains("MissingNode") && !x.IsComplete));
            Assert.IsTrue(row.SkillTree.Checks.Any(x => x.Details.Contains("MissingBattleSkill") && !x.IsComplete));
            Assert.IsTrue(row.SkillTree.Checks.Any(x => x.Details.Contains("MissingTraining") && !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_BrokenEventReferences_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationEntry gameEvent = profile.ConversationEntries.First(x => x.Kind == ConversationDataKind.GameEvents);
            gameEvent.Conditions.Once = true;
            gameEvent.Conditions.CostumeId = "MissingCostume";
            gameEvent.ImageAssetIdsText = "MissingImage";
            gameEvent.Conditions.RequiredSkillIdsText = "MissingSkill";

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.Events.Kind);
            Assert.IsTrue(row.Events.Checks.Any(x => x.Details.Contains("Once用フラグ") && !x.IsComplete));
            Assert.IsTrue(row.Events.Checks.Any(x => x.Details.Contains("MissingImage") && !x.IsComplete));
            Assert.AreEqual(ProductionStatusKind.Partial, row.ExportReadiness.Kind);
        }

        [TestMethod]
        public void Evaluate_MissingAcceptedImageFile_ReturnsPartialExportReadiness()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(
                profile,
                new[] { new ExpressionDefinition { ExpressionId = "Neutral" } },
                new[] { new CostumeDefinition { CostumeId = "Default" } },
                new[]
                {
                    new LayerAssetDefinition { AssetId = "Expression_Neutral", LayerKind = "Expression", ExpressionId = "Neutral" },
                    new LayerAssetDefinition { AssetId = "Costume_Default", LayerKind = "Costume", CostumeId = "Default" }
                },
                asset => asset.AssetId != "A1");

            Assert.AreEqual(ProductionStatusKind.Partial, row.ExportReadiness.Kind);
            Assert.IsTrue(row.ExportReadiness.Checks.Any(x => x.Details.Contains("A1") && !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_DetailChecksContainNavigationTargets()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            ProductionStatusCheckItem gameEvent = row.Events.Checks.First(x => x.Name.Contains("GameEvents01"));
            Assert.AreEqual(ProductionStatusTargetKind.Conversation, gameEvent.TargetKind);
            Assert.AreEqual(ConversationDataKind.GameEvents, gameEvent.ConversationKind);
            Assert.AreEqual("GameEvents01", gameEvent.TargetId);
            Assert.AreEqual(1, gameEvent.TargetTabIndex);

            ProductionStatusCheckItem battleSkill = row.BattleSkills.Checks.First(x => x.Name.Contains("BattleSkillA"));
            Assert.AreEqual(ProductionStatusTargetKind.BattleSkill, battleSkill.TargetKind);
            Assert.AreEqual("BattleSkillA", battleSkill.TargetId);

            ProductionStatusCheckItem trainingDialogue = row.TrainingDialogues.Checks.First();
            Assert.AreEqual(ProductionStatusTargetKind.TrainingDialogue, trainingDialogue.TargetKind);
            Assert.AreEqual("TrainingA", trainingDialogue.TargetId);
            Assert.AreEqual("SelectedBeforeFirstStep", trainingDialogue.TargetSubId);
            Assert.AreEqual(4, trainingDialogue.TargetTabIndex);
        }

        [TestMethod]
        public void Evaluate_MissingTrainingDialogueText_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            profile.TrainingDialogues.Items[0].Messages[0].Text = string.Empty;

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.TrainingDialogues.Kind);
            Assert.IsTrue(row.TrainingDialogues.Checks.Any(x => !x.IsComplete && x.Details.Contains("本文")));
        }

        [TestMethod]
        public void Evaluate_CharacterImages_DistinguishesRequiredAndOptionalDefinitions()
        {
            HeroineProfile profile = CompleteProfile();
            profile.Assets.Add(new HeroineAsset
            {
                AssetId = "Heroine_Normal",
                Usage = AssetUsage.Sprites,
                Status = AssetStatus.Pending
            });
            StillDefinition[] stills =
            {
                new StillDefinition { AssetId = "Heroine_Normal", Usage = AssetUsage.Sprites },
                new StillDefinition { AssetId = "OptionalEnding", Usage = AssetUsage.Ending }
            };

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(
                profile,
                new[] { new ExpressionDefinition { ExpressionId = "Neutral" } },
                new[] { new CostumeDefinition { CostumeId = "Default" } },
                new[]
                {
                    new LayerAssetDefinition { AssetId = "Expression_Neutral", LayerKind = "Expression", ExpressionId = "Neutral" },
                    new LayerAssetDefinition { AssetId = "Costume_Default", LayerKind = "Costume", CostumeId = "Default" }
                },
                asset => !string.IsNullOrWhiteSpace(asset.StoredPath),
                null,
                stills);

            Assert.AreEqual(ProductionStatusKind.Partial, row.CharacterImages.Kind);
            ProductionStatusCheckItem required = row.CharacterImages.Checks.First(x => x.TargetId == "Heroine_Normal");
            ProductionStatusCheckItem optional = row.CharacterImages.Checks.First(x => x.TargetId == "OptionalEnding");
            Assert.AreEqual("×", required.Symbol);
            Assert.AreEqual("―", optional.Symbol);
            Assert.AreEqual(ProductionStatusTargetKind.StillDefinition, required.TargetKind);
            Assert.AreEqual(7, required.TargetTabIndex);
        }

        private static HeroineProfile CompleteProfile()
        {
            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = "TestHeroine",
                DisplayName = "テストヒロイン",
                Personality = "強気",
                SpeakingStyle = "丁寧",
                InitialDialogueMessage = "こんにちは",
                TrainingCatalog = new TrainingCatalogSettings
                {
                    Items = new ObservableCollection<TrainingCatalogItem>
                    {
                        new TrainingCatalogItem { TrainingId = "TrainingA" }
                    }
                },
                TrainingImages = new TrainingImageSettings
                {
                    Items = new ObservableCollection<TrainingImageEntry>
                    {
                        new TrainingImageEntry
                        {
                            TrainingId = "TrainingA",
                            BeforeFirstStepImageAssetId = "A1",
                            AfterFirstStepImageAssetId = "A2",
                            PlayerLpConsumedImageAssetId = "A3",
                            HeroineLpConsumedImageAssetId = "A4",
                            SimultaneousLpConsumedImageAssetId = "A5"
                        }
                    }
                }
            };
            foreach (string type in new[] { "SoloVictory", "DuoVictory", "SoloDefeat", "DuoDefeat" })
            {
                profile.BattleMessages.ResultEvents.Add(new BattleResultEventEntry
                {
                    EventId = type,
                    ResultType = type,
                    Message = "message"
                });
            }
            foreach (string type in new[] { "Victory", "Defeat" })
            {
                profile.BattleMessages.PanelMessages.Add(new BattlePanelResultMessageEntry
                {
                    MessageId = type,
                    ResultType = type,
                    Message = "message"
                });
            }
            profile.BattleSkills.Add(new HeroineBattleSkill
            {
                SkillId = "BattleSkillA",
                DisplayName = "攻撃",
                EffectType = "Damage",
                Target = "Enemy",
                Cost = 2,
                Power = 10,
                UseChancePercent = 50,
                MaxUsesPerBattle = 2
            });
            profile.HeroineSkillTree.TrainingSkills.Add(new HeroineTrainingSkill
            {
                SkillId = "TrainingSkillA",
                DisplayName = "訓練補助"
            });
            profile.HeroineSkillTree.Nodes.Add(new HeroineSkillTreeNode
            {
                NodeId = "Root",
                DisplayName = "ルート",
                GrantedHeroineSkillId = "BattleSkillA"
            });
            profile.HeroineSkillTree.Nodes.Add(new HeroineSkillTreeNode
            {
                NodeId = "TrainingNode",
                DisplayName = "訓練ノード",
                TrainingSkillId = "TrainingSkillA",
                PrerequisiteNodeIds = new ObservableCollection<string> { "Root" },
                UnlockedTrainingIds = new ObservableCollection<string> { "TrainingA" }
            });
            foreach (string id in new[] { "A1", "A2", "A3", "A4", "A5" })
            {
                profile.Assets.Add(new HeroineAsset { AssetId = id, Status = AssetStatus.Accepted, StoredPath = id + ".png" });
            }
            profile.Assets.Add(new HeroineAsset { AssetId = "Expression_Neutral", Status = AssetStatus.Accepted, StoredPath = "Expression_Neutral.png" });
            profile.Assets.Add(new HeroineAsset { AssetId = "Costume_Default", Status = AssetStatus.Accepted, StoredPath = "Costume_Default.png" });
            foreach (string state in new[]
            {
                "SelectedBeforeFirstStep", "SelectedAfterFirstStep", "PlayerLpConsumed",
                "HeroineLpConsumed", "SimultaneousLpConsumed"
            })
            {
                profile.TrainingDialogues.Items.Add(new TrainingDialogueEntry
                {
                    TrainingId = "TrainingA",
                    VisualState = state,
                    Messages = new ObservableCollection<TrainingDialogueMessage>
                    {
                        new TrainingDialogueMessage { Text = state + "のセリフ" }
                    }
                });
            }
            profile.ConversationEntries.Add(new ConversationEntry
            {
                Id = "Conversation01",
                Kind = ConversationDataKind.Conversations,
                Lines = new ObservableCollection<ConversationLine>
                {
                    new ConversationLine { Text = "こんにちは", Expression = "Neutral" }
                }
            });
            foreach (ConversationDataKind kind in new[]
            {
                ConversationDataKind.GameEvents,
                ConversationDataKind.ScheduledEvents,
                ConversationDataKind.Endings
            })
            {
                profile.ConversationEntries.Add(new ConversationEntry
                {
                    Id = kind + "01",
                    Kind = kind,
                    Lines = new ObservableCollection<ConversationLine>
                    {
                        new ConversationLine { Text = kind + "の本文", Expression = "Neutral" }
                    }
                });
            }
            return profile;
        }

        private static CharacterProductionStatusRow EvaluateWithDefinitions(HeroineProfile profile) =>
            CharacterProductionStatusService.Evaluate(
                profile,
                new[] { new ExpressionDefinition { ExpressionId = "Neutral" } },
                new[] { new CostumeDefinition { CostumeId = "Default" } },
                new[]
                {
                    new LayerAssetDefinition { AssetId = "Expression_Neutral", LayerKind = "Expression", ExpressionId = "Neutral" },
                    new LayerAssetDefinition { AssetId = "Costume_Default", LayerKind = "Costume", CostumeId = "Default" }
                });
    }
}
