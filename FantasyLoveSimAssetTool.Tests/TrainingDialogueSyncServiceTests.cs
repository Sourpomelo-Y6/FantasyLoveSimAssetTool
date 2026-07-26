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
            Assert.AreEqual(0, item.GetProperty("voicedMessages").GetArrayLength());
        }

        [TestMethod]
        public void BuildExportJson_SeparatesVoicedMessages()
        {
            TrainingDialogueSettings settings =
                SettingsWith("CooperativeDrill", "PlayerLpConsumed", "通常", "音声付き");
            settings.Items[0].Messages[1].VoiceId = " Training/Line01 ";
            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = "TestHeroine",
                TrainingDialogues = settings
            };

            string json = TrainingDialogueSyncService.BuildExportJson(
                profile,
                new ExportReport());
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement item = document.RootElement.GetProperty("items")[0];
            JsonElement voiced = item.GetProperty("voicedMessages")[0];

            CollectionAssert.AreEqual(
                new[] { "通常" },
                item.GetProperty("messages").EnumerateArray()
                    .Select(x => x.GetString()).ToArray());
            Assert.AreEqual("音声付き", voiced.GetProperty("message").GetString());
            Assert.AreEqual("Training/Line01", voiced.GetProperty("voiceId").GetString());
        }

        [TestMethod]
        public void MergeFromUnity_UpdatesVoiceIdByMessageWithoutDuplicatingText()
        {
            TrainingDialogueSettings settings =
                SettingsWith("CooperativeDrill", "PlayerLpConsumed", "音声付き");
            FromUnityTrainingDialogueItem item =
                Item("CooperativeDrill", "PlayerLpConsumed");
            item.VoicedMessages = new List<FromUnityTrainingDialogueVoiceItem>
            {
                new FromUnityTrainingDialogueVoiceItem
                {
                    Message = "音声付き",
                    VoiceId = "Training/Line01"
                }
            };

            TrainingDialogueMergeResult result = TrainingDialogueSyncService.MergeFromUnity(
                settings,
                "TestHeroine",
                Data(item));

            Assert.AreEqual(1, settings.Items[0].Messages.Count);
            Assert.AreEqual("Training/Line01", settings.Items[0].Messages[0].VoiceId);
            Assert.AreEqual(1, result.UpdatedVoiceIdCount);
        }

        [TestMethod]
        public void MergeFromUnity_LegacyMessagesPreserveExistingVoiceId()
        {
            TrainingDialogueSettings settings =
                SettingsWith("CooperativeDrill", "PlayerLpConsumed", "既存");
            settings.Items[0].Messages[0].VoiceId = "Training/Existing01";

            TrainingDialogueSyncService.MergeFromUnity(
                settings,
                "TestHeroine",
                Data(Item("CooperativeDrill", "PlayerLpConsumed", "既存")));

            Assert.AreEqual(
                "Training/Existing01",
                settings.Items[0].Messages[0].VoiceId);
        }

        [TestMethod]
        public void ExportAndImport_RoundTripsFortyMessagesWithoutRemovingToolOnlyData()
        {
            string[] trainingIds = { "BasicTraining", "MagicTraining", "EnduranceTraining", "CooperativeDrill" };
            string[] states =
            {
                "SelectedBeforeFirstStep",
                "SelectedAfterFirstStep",
                "PlayerLpConsumed",
                "HeroineLpConsumed",
                "SimultaneousLpConsumed"
            };
            TrainingDialogueSettings sourceSettings = new TrainingDialogueSettings();
            foreach (string trainingId in trainingIds)
            {
                foreach (string state in states)
                {
                    sourceSettings.Items.Add(new TrainingDialogueEntry
                    {
                        TrainingId = trainingId,
                        VisualState = state,
                        Messages = new ObservableCollection<TrainingDialogueMessage>
                        {
                            new TrainingDialogueMessage { Text = $" {trainingId}-{state}-A " },
                            new TrainingDialogueMessage { Text = $"{trainingId}-{state}-B" }
                        }
                    });
                }
            }
            HeroineProfile sourceProfile = new HeroineProfile
            {
                HeroineId = "TestHeroine",
                TrainingDialogues = sourceSettings
            };
            TrainingDialogueSettings importedSettings = SettingsWith(
                "CooperativeDrill",
                "SelectedBeforeFirstStep",
                "Toolだけの既存候補");

            string exportJson = TrainingDialogueSyncService.BuildExportJson(sourceProfile, new ExportReport());
            FromUnityTrainingDialogueDataFile exportedData = TrainingDialogueSyncService.DeserializeFromUnity(exportJson);
            TrainingDialogueMergeResult first = TrainingDialogueSyncService.MergeFromUnity(
                importedSettings,
                "TestHeroine",
                exportedData);

            Assert.AreEqual(20, importedSettings.Items.Count);
            Assert.AreEqual(40, first.AddedMessageCount);
            Assert.AreEqual(41, Flatten(importedSettings).Count);
            Assert.IsTrue(Flatten(importedSettings).Contains(
                "CooperativeDrill\nSelectedBeforeFirstStep\nToolだけの既存候補"));

            HashSet<string> expected = Flatten(sourceSettings, true);
            HashSet<string> actualExportedMessages = Flatten(importedSettings)
                .Where(value => !value.EndsWith("\nToolだけの既存候補", StringComparison.Ordinal))
                .ToHashSet(StringComparer.Ordinal);
            CollectionAssert.AreEquivalent(expected.ToArray(), actualExportedMessages.ToArray());
            Assert.IsFalse(actualExportedMessages.Any(value => value.EndsWith(" ", StringComparison.Ordinal)));

            TrainingDialogueMergeResult second = TrainingDialogueSyncService.MergeFromUnity(
                importedSettings,
                "TestHeroine",
                exportedData);

            Assert.AreEqual(0, second.AddedEntryCount);
            Assert.AreEqual(0, second.AddedMessageCount);
            Assert.AreEqual(40, second.SkippedCount);
            Assert.AreEqual(41, Flatten(importedSettings).Count);
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

        private static HashSet<string> Flatten(TrainingDialogueSettings settings, bool trimMessages = false)
        {
            return settings.Items
                .SelectMany(entry => entry.Messages.Select(message =>
                    $"{entry.TrainingId}\n{entry.VisualState}\n{(trimMessages ? message.Text.Trim() : message.Text)}"))
                .ToHashSet(StringComparer.Ordinal);
        }
    }
}
