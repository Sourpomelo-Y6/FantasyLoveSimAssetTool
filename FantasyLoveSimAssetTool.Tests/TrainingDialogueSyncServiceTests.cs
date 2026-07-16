using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class TrainingDialogueSyncServiceTests
    {
        [TestMethod]
        public void MergeFromUnity_AddsOnlyNewMessagesToExistingEntry()
        {
            TrainingDialogueSettings settings = SettingsWith("CooperativeDrill", "SelectedBeforeFirstStep", "セリフA");
            FromUnityTrainingDialogueDataFile data = Data(Item("CooperativeDrill", "SelectedBeforeFirstStep", "セリフA", " セリフB "));

            TrainingDialogueMergeResult result = TrainingDialogueSyncService.MergeFromUnity(settings, "TestHeroine", data);

            CollectionAssert.AreEqual(new[] { "セリフA", "セリフB" }, settings.Items.Single().Messages.Select(x => x.Text).ToArray());
            Assert.AreEqual(0, result.AddedEntryCount);
            Assert.AreEqual(1, result.AddedMessageCount);
            Assert.AreEqual(1, result.SkippedCount);
        }

        [TestMethod]
        public void MergeFromUnity_IsIdempotent()
        {
            TrainingDialogueSettings settings = new TrainingDialogueSettings();
            FromUnityTrainingDialogueDataFile data = Data(Item("CooperativeDrill", "PlayerLpConsumed", "セリフA", "セリフB"));

            TrainingDialogueSyncService.MergeFromUnity(settings, "TestHeroine", data);
            TrainingDialogueMergeResult second = TrainingDialogueSyncService.MergeFromUnity(settings, "TestHeroine", data);

            Assert.AreEqual(1, settings.Items.Count);
            Assert.AreEqual(2, settings.Items.Single().Messages.Count);
            Assert.AreEqual(0, second.AddedEntryCount);
            Assert.AreEqual(0, second.AddedMessageCount);
            Assert.AreEqual(2, second.SkippedCount);
        }

        [DataTestMethod]
        [DataRow("BeforeFirstStep", "SelectedBeforeFirstStep")]
        [DataRow("AfterFirstStep", "SelectedAfterFirstStep")]
        public void MergeFromUnity_NormalizesLegacyVisualStates(string legacyState, string currentState)
        {
            TrainingDialogueSettings settings = SettingsWith("TrainingA", legacyState, "既存");

            TrainingDialogueSyncService.MergeFromUnity(settings, "TestHeroine", Data(Item("TrainingA", currentState, "追加")));

            Assert.AreEqual(1, settings.Items.Count);
            Assert.AreEqual(currentState, settings.Items.Single().VisualState);
            Assert.AreEqual(2, settings.Items.Single().Messages.Count);
        }

        [TestMethod]
        public void MergeFromUnity_PreservesAllFiveStatesForAdditionalTraining()
        {
            string[] states = { "SelectedBeforeFirstStep", "SelectedAfterFirstStep", "PlayerLpConsumed", "HeroineLpConsumed", "SimultaneousLpConsumed" };
            FromUnityTrainingDialogueDataFile data = Data(states.Select(state => Item("CooperativeDrill", state, state)).ToArray());
            TrainingDialogueSettings settings = new TrainingDialogueSettings();

            TrainingDialogueSyncService.MergeFromUnity(settings, "TestHeroine", data);

            CollectionAssert.AreEquivalent(states, settings.Items.Select(x => x.VisualState).ToArray());
            Assert.IsTrue(settings.Items.All(x => x.TrainingId == "CooperativeDrill"));
        }

        [TestMethod]
        public void MergeFromUnity_SkipsInvalidItemsAndMessages()
        {
            FromUnityTrainingDialogueDataFile data = Data(
                Item("", "PlayerLpConsumed", "invalid"),
                Item("TrainingA", "Unknown", "invalid"),
                Item("TrainingA", "PlayerLpConsumed", null, " ", "valid", "valid"));
            TrainingDialogueSettings settings = new TrainingDialogueSettings();

            TrainingDialogueMergeResult result = TrainingDialogueSyncService.MergeFromUnity(settings, "TestHeroine", data);

            Assert.AreEqual(1, settings.Items.Count);
            CollectionAssert.AreEqual(new[] { "valid" }, settings.Items.Single().Messages.Select(x => x.Text).ToArray());
            Assert.AreEqual(5, result.SkippedCount);
        }

        [TestMethod]
        public void MergeFromUnity_RejectsUnsupportedSchemaVersion()
        {
            FromUnityTrainingDialogueDataFile data = Data();
            data.SchemaVersion = 2;
            Assert.ThrowsException<InvalidOperationException>(() =>
                TrainingDialogueSyncService.MergeFromUnity(new TrainingDialogueSettings(), "TestHeroine", data));
        }

        [TestMethod]
        public void MergeFromUnity_RejectsDifferentHeroine()
        {
            FromUnityTrainingDialogueDataFile data = Data();
            data.HeroineId = "OtherHeroine";
            Assert.ThrowsException<InvalidOperationException>(() =>
                TrainingDialogueSyncService.MergeFromUnity(new TrainingDialogueSettings(), "TestHeroine", data));
        }

        [TestMethod]
        public void BuildExportJson_TrimsFiltersAndDeduplicatesMessages()
        {
            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = "TestHeroine",
                TrainingDialogues = SettingsWith("CooperativeDrill", "SimultaneousLpConsumed", " セリフA ", "", "セリフA", "セリフB")
            };

            string json = TrainingDialogueSyncService.BuildExportJson(profile, new ExportReport());
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            JsonElement item = root.GetProperty("items")[0];

            Assert.AreEqual(1, root.GetProperty("schemaVersion").GetInt32());
            Assert.AreEqual("TestHeroine", root.GetProperty("heroineId").GetString());
            Assert.AreEqual("CooperativeDrill", item.GetProperty("trainingId").GetString());
            CollectionAssert.AreEqual(new[] { "セリフA", "セリフB" }, item.GetProperty("messages").EnumerateArray().Select(x => x.GetString()).ToArray());
        }

        private static TrainingDialogueSettings SettingsWith(string trainingId, string state, params string[] messages)
        {
            return new TrainingDialogueSettings
            {
                Items = new ObservableCollection<TrainingDialogueEntry>
                {
                    new TrainingDialogueEntry
                    {
                        TrainingId = trainingId,
                        VisualState = state,
                        Messages = new ObservableCollection<TrainingDialogueMessage>(messages.Select(x => new TrainingDialogueMessage { Text = x }))
                    }
                }
            };
        }

        private static FromUnityTrainingDialogueDataFile Data(params FromUnityTrainingDialogueItem[] items)
        {
            return new FromUnityTrainingDialogueDataFile { SchemaVersion = 1, HeroineId = "TestHeroine", Items = items.ToList() };
        }

        private static FromUnityTrainingDialogueItem Item(string trainingId, string state, params string[] messages)
        {
            return new FromUnityTrainingDialogueItem { TrainingId = trainingId, VisualState = state, Messages = messages.ToList() };
        }
    }
}
