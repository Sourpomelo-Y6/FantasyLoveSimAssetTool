using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class BattleMessageSyncServiceTests
    {
        [TestMethod]
        public void ExportAndImport_RoundTripsResultEventsAndPanelMessages()
        {
            HeroineProfile source = Profile();
            HeroineProfile target = new HeroineProfile { HeroineId = "TestHeroine" };

            BattleMessageSyncService.ApplyResultEvents(target,
                BattleMessageSyncService.DeserializeResultEvents(BattleMessageSyncService.BuildResultEventsJson(source)));
            BattleMessageSyncService.ApplyPanelMessages(target,
                BattleMessageSyncService.DeserializePanelMessages(BattleMessageSyncService.BuildPanelMessagesJson(source)));

            BattleResultEventEntry result = target.BattleMessages.ResultEvents.Single();
            Assert.AreEqual("DuoVictory_Forest", result.EventId);
            Assert.AreEqual("Forest", result.BattleContextId);
            Assert.AreEqual("Formal, Casual", result.UnlockedOutfitIdsText);
            Assert.AreEqual(3, result.AffectionChange);
            Assert.AreEqual("StillWithPortrait", result.VisualMode);
            Assert.AreEqual("Battle/DuoVictoryForest01", result.VoiceId);
            Assert.AreEqual("勝利しました", target.BattleMessages.PanelMessages.Single().Message);
            Assert.AreEqual(
                "Battle/Victory01",
                target.BattleMessages.PanelMessages.Single().VoiceId);
        }

        [TestMethod]
        public void Apply_MissingItemsPreservesExistingData()
        {
            HeroineProfile profile = Profile();
            BattleMessageSyncService.ApplyResultEvents(profile,
                BattleMessageSyncService.DeserializeResultEvents("{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\"}"));
            BattleMessageSyncService.ApplyPanelMessages(profile,
                BattleMessageSyncService.DeserializePanelMessages("{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\"}"));

            Assert.AreEqual(1, profile.BattleMessages.ResultEvents.Count);
            Assert.AreEqual(1, profile.BattleMessages.PanelMessages.Count);
        }

        [TestMethod]
        public void Apply_ExplicitEmptyItemsClearsData()
        {
            HeroineProfile profile = Profile();
            BattleMessageSyncService.ApplyResultEvents(profile,
                BattleMessageSyncService.DeserializeResultEvents("{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[]}"));
            BattleMessageSyncService.ApplyPanelMessages(profile,
                BattleMessageSyncService.DeserializePanelMessages("{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[]}"));

            Assert.AreEqual(0, profile.BattleMessages.ResultEvents.Count);
            Assert.AreEqual(0, profile.BattleMessages.PanelMessages.Count);
        }

        [TestMethod]
        public void Apply_OldJsonPreservesVoiceIdsAndExplicitEmptyClearsThem()
        {
            HeroineProfile profile = Profile();
            BattleMessageSyncService.ApplyResultEvents(
                profile,
                BattleMessageSyncService.DeserializeResultEvents(
                    "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                    "\"eventId\":\"DuoVictory_Forest\",\"resultType\":\"DuoVictory\"," +
                    "\"battleContextId\":\"Forest\",\"message\":\"更新\"}]}"));
            BattleMessageSyncService.ApplyPanelMessages(
                profile,
                BattleMessageSyncService.DeserializePanelMessages(
                    "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                    "\"messageId\":\"Victory\",\"resultType\":\"Victory\",\"message\":\"更新\"}]}"));

            Assert.AreEqual(
                "Battle/DuoVictoryForest01",
                profile.BattleMessages.ResultEvents.Single().VoiceId);
            Assert.AreEqual(
                "Battle/Victory01",
                profile.BattleMessages.PanelMessages.Single().VoiceId);

            BattleMessageSyncService.ApplyResultEvents(
                profile,
                BattleMessageSyncService.DeserializeResultEvents(
                    "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                    "\"eventId\":\"DuoVictory_Forest\",\"resultType\":\"DuoVictory\"," +
                    "\"battleContextId\":\"Forest\",\"message\":\"更新\",\"voiceId\":\"\"}]}"));
            BattleMessageSyncService.ApplyPanelMessages(
                profile,
                BattleMessageSyncService.DeserializePanelMessages(
                    "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                    "\"messageId\":\"Victory\",\"resultType\":\"Victory\",\"message\":\"更新\"," +
                    "\"voiceId\":\"\"}]}"));

            Assert.AreEqual(string.Empty, profile.BattleMessages.ResultEvents.Single().VoiceId);
            Assert.AreEqual(string.Empty, profile.BattleMessages.PanelMessages.Single().VoiceId);
        }

        [TestMethod]
        public void Apply_RejectsDifferentHeroine()
        {
            Assert.ThrowsException<System.InvalidOperationException>(() =>
                BattleMessageSyncService.ApplyResultEvents(Profile(), new BattleResultEventsDataFile { HeroineId = "Other" }));
        }

        [TestMethod]
        public void Validate_ReportsDuplicateUnknownAndMissingValues()
        {
            HeroineProfile profile = Profile();
            profile.BattleMessages.ResultEvents.Add(new BattleResultEventEntry
            {
                EventId = "OtherId",
                ResultType = "DuoVictory",
                BattleContextId = "Forest",
                SpeakerType = "FutureSpeaker",
                VisualMode = "FutureVisual",
                Message = string.Empty,
                StillId = "UnknownStill",
                ExpressionId = "UnknownExpression",
                UnlockedOutfitIds = new[] { "UnknownOutfit" }
            });
            profile.BattleMessages.PanelMessages.Add(new BattlePanelResultMessageEntry
            {
                MessageId = "duplicate",
                ResultType = "Victory",
                Message = string.Empty
            });
            profile.BattleMessages.PanelMessages.Add(new BattlePanelResultMessageEntry
            {
                MessageId = string.Empty,
                ResultType = "FutureResult",
                Message = "future"
            });

            string[] messages = BattleMessageSyncService.Validate(
                profile,
                new[] { "VictoryStill" },
                new[] { "Formal", "Casual" },
                new[] { "Smile" }).ToArray();

            Assert.IsTrue(messages.Any(x => x.Contains("resultType + battleContextId")));
            Assert.IsTrue(messages.Any(x => x.Contains("message が空")));
            Assert.IsTrue(messages.Any(x => x.Contains("UnknownStill")));
            Assert.IsTrue(messages.Any(x => x.Contains("UnknownOutfit")));
            Assert.IsTrue(messages.Any(x => x.Contains("FutureSpeaker")));
            Assert.IsTrue(messages.Any(x => x.Contains("FutureVisual")));
            Assert.IsTrue(messages.Any(x => x.Contains("UnknownExpression")));
            Assert.IsTrue(messages.Any(x => x.Contains("FutureResult")));
            Assert.IsTrue(messages.Any(x => x.Contains("MessageId が空")));
            Assert.IsTrue(messages.Any(x => x.Contains("resultType `Victory` が重複")));
        }

        [TestMethod]
        public void Validate_AcceptsKnownCompleteValues()
        {
            string[] messages = BattleMessageSyncService.Validate(
                Profile(),
                new[] { "VictoryStill" },
                new[] { "Formal", "Casual" },
                new[] { "Smile" }).ToArray();

            Assert.AreEqual(0, messages.Length);
        }

        [TestMethod]
        public void AnalyzeChanges_ReportsResultDetailsAndPanelCounts()
        {
            BattleResultEventEntry before = Profile().BattleMessages.ResultEvents.Single();
            BattleResultEventEntry updated = new BattleResultEventEntry
            {
                EventId = before.EventId,
                ResultType = before.ResultType,
                BattleContextId = before.BattleContextId,
                SpeakerType = "Player",
                SpeakerName = "主人公",
                Message = before.Message,
                StillId = before.StillId,
                VisualMode = "PortraitOnly",
                ExpressionId = "Angry",
                AffectionChange = before.AffectionChange,
                UnlockedOutfitIds = before.UnlockedOutfitIds
            };
            BattleMessageChangeSummary summary = BattleMessageSyncService.AnalyzeChanges(
                new[] { before, new BattleResultEventEntry { EventId = "Deleted" } },
                new[] { updated, new BattleResultEventEntry { EventId = "Added" } },
                new[] { new BattlePanelResultMessageEntry { MessageId = "Victory", Message = "old" } },
                new[] { new BattlePanelResultMessageEntry { MessageId = "Victory", Message = "new" } });

            Assert.AreEqual(1, summary.ResultAdded);
            Assert.AreEqual(1, summary.ResultUpdated);
            Assert.AreEqual(1, summary.ResultDeleted);
            Assert.AreEqual(1, summary.PanelUpdated);
            Assert.AreEqual(1, summary.SpeakerChanged);
            Assert.AreEqual(1, summary.ExpressionChanged);
            Assert.AreEqual(1, summary.VisualModeChanged);
        }

        private static HeroineProfile Profile()
        {
            return new HeroineProfile
            {
                HeroineId = "TestHeroine",
                BattleMessages = new BattleMessageSettings
                {
                    ResultEvents = new ObservableCollection<BattleResultEventEntry>
                    {
                        new BattleResultEventEntry
                        {
                            EventId = "DuoVictory_Forest",
                            ResultType = "DuoVictory",
                            BattleContextId = "Forest",
                            SpeakerType = "Heroine",
                            SpeakerName = "テストヒロイン",
                            VisualMode = "StillWithPortrait",
                            Message = "二人で勝てましたね",
                            VoiceId = "Battle/DuoVictoryForest01",
                            StillId = "VictoryStill",
                            ExpressionId = "Smile",
                            AffectionChange = 3,
                            UnlockedOutfitIdsText = " Formal, Casual, Formal "
                        }
                    },
                    PanelMessages = new ObservableCollection<BattlePanelResultMessageEntry>
                    {
                        new BattlePanelResultMessageEntry
                        {
                            MessageId = "Victory",
                            ResultType = "Victory",
                            Message = "勝利しました",
                            VoiceId = "Battle/Victory01"
                        }
                    }
                }
            };
        }
    }
}
