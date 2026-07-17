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
            Assert.AreEqual(4, row.BasicInformation.Checks.Count);
            Assert.AreEqual(7, row.BattleMessages.Checks.Count);
            Assert.AreEqual(5, row.TrainingImages.Checks.Count);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Conversations.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Expressions.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Costumes.Kind);
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
            foreach (string id in new[] { "A1", "A2", "A3", "A4", "A5" })
            {
                profile.Assets.Add(new HeroineAsset { AssetId = id, Status = AssetStatus.Accepted });
            }
            profile.Assets.Add(new HeroineAsset { AssetId = "Expression_Neutral", Status = AssetStatus.Accepted });
            profile.Assets.Add(new HeroineAsset { AssetId = "Costume_Default", Status = AssetStatus.Accepted });
            profile.ConversationEntries.Add(new ConversationEntry
            {
                Id = "Conversation01",
                Kind = ConversationDataKind.Conversations,
                Lines = new ObservableCollection<ConversationLine>
                {
                    new ConversationLine { Text = "こんにちは", Expression = "Neutral" }
                }
            });
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
