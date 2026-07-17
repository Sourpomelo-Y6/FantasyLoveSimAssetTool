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

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(profile);

            Assert.AreEqual(ProductionStatusKind.Complete, row.BasicInformation.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.BattleMessages.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.TrainingImages.Kind);
            Assert.AreEqual(4, row.BasicInformation.Checks.Count);
            Assert.AreEqual(7, row.BattleMessages.Checks.Count);
            Assert.AreEqual(5, row.TrainingImages.Checks.Count);
            Assert.IsFalse(row.HasIncomplete);
        }

        [TestMethod]
        public void Evaluate_PartialProfile_ExplainsMissingFieldsAndSlots()
        {
            HeroineProfile profile = CompleteProfile();
            profile.Personality = string.Empty;
            profile.BattleMessages.ResultEvents.RemoveAt(0);
            profile.Assets[0].Status = AssetStatus.Pending;

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(profile);

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

        private static HeroineProfile CompleteProfile()
        {
            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = "TestHeroine",
                DisplayName = "テストヒロイン",
                Personality = "強気",
                SpeakingStyle = "丁寧",
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
            return profile;
        }
    }
}
