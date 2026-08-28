using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class TrainingCatalogSyncServiceTests
    {
        [TestMethod]
        public void MergeFromUnity_ImportsConditionalTrainingMetadata()
        {
            FromUnityTrainingCatalogDataFile data = TrainingCatalogSyncService.DeserializeFromUnity(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                "\"trainingId\":\"Advanced\",\"displayName\":\"連携訓練\",\"sortOrder\":42," +
                "\"occurrenceType\":\"OncePerSave\"," +
                "\"visibleConditionRanks\":[\"Excellent\"]," +
                "\"executableConditionRanks\":[\"Normal\",\"Excellent\"]," +
                "\"requiredCompletedTrainingIds\":[\"Trial\"]," +
                "\"requireAllCompletedTrainings\":false," +
                "\"hideUntilPrerequisitesMet\":true,\"hideAfterCompletion\":true}]}" );
            TrainingCatalogSettings settings = new TrainingCatalogSettings
            {
                Items = new ObservableCollection<TrainingCatalogItem>
                {
                    new TrainingCatalogItem { TrainingId = "Trial", DisplayName = "試験訓練" }
                }
            };

            TrainingCatalogMergeResult result = TrainingCatalogSyncService.MergeFromUnity(
                settings, "TestHeroine", data);

            TrainingCatalogItem item = settings.Items.Single(value => value.TrainingId == "Advanced");
            Assert.AreEqual(1, result.AddedCount);
            Assert.AreEqual(0, result.WarningCount);
            Assert.AreEqual(42, item.SortOrder);
            Assert.AreEqual("OncePerSave", item.OccurrenceType);
            CollectionAssert.AreEqual(new[] { "Excellent" }, item.VisibleConditionRanks);
            CollectionAssert.AreEqual(new[] { "Normal", "Excellent" }, item.ExecutableConditionRanks);
            CollectionAssert.AreEqual(new[] { "Trial" }, item.RequiredCompletedTrainingIds);
            Assert.IsFalse(item.RequireAllCompletedTrainings);
            Assert.IsTrue(item.HideUntilPrerequisitesMet);
            Assert.IsTrue(item.HideAfterCompletion);
            StringAssert.Contains(item.ConditionBadgeSummary, "一回限定");
            StringAssert.Contains(item.ConditionBadgeSummary, "絶好調限定");
            StringAssert.Contains(item.ConditionBadgeSummary, "不調時不可");
            StringAssert.Contains(item.ConditionDetails, "実行: 普通 / 絶好調");
        }

        [TestMethod]
        public void MergeFromUnity_OldJsonPreservesExistingConditionalMetadata()
        {
            TrainingCatalogItem existing = new TrainingCatalogItem
            {
                TrainingId = "Trial",
                OccurrenceType = "OncePerSave",
                VisibleConditionRanks = new List<string> { "Excellent" },
                RequiredCompletedTrainingIds = new List<string> { "Preparation" },
                HideAfterCompletion = true
            };
            TrainingCatalogSettings settings = new TrainingCatalogSettings
            {
                Items = new ObservableCollection<TrainingCatalogItem>
                {
                    existing,
                    new TrainingCatalogItem { TrainingId = "Preparation" }
                }
            };
            FromUnityTrainingCatalogDataFile data = TrainingCatalogSyncService.DeserializeFromUnity(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                "\"trainingId\":\"Trial\",\"displayName\":\"更新名\",\"unlockedByDefault\":true}]}" );

            TrainingCatalogSyncService.MergeFromUnity(settings, "TestHeroine", data);

            Assert.AreEqual("更新名", existing.DisplayName);
            Assert.AreEqual("OncePerSave", existing.OccurrenceType);
            CollectionAssert.AreEqual(new[] { "Excellent" }, existing.VisibleConditionRanks);
            CollectionAssert.AreEqual(new[] { "Preparation" }, existing.RequiredCompletedTrainingIds);
            Assert.IsTrue(existing.HideAfterCompletion);
        }

        [TestMethod]
        public void MergeFromUnity_SkipsDuplicateIdsAndWarnsForUnknownPrerequisite()
        {
            FromUnityTrainingCatalogDataFile data = TrainingCatalogSyncService.DeserializeFromUnity(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[" +
                "{\"trainingId\":\"Advanced\",\"requiredCompletedTrainingIds\":[\"Missing\"]}," +
                "{\"trainingId\":\"Advanced\"}]}" );
            TrainingCatalogSettings settings = new TrainingCatalogSettings();

            TrainingCatalogMergeResult result = TrainingCatalogSyncService.MergeFromUnity(
                settings, "TestHeroine", data);

            Assert.AreEqual(1, result.AddedCount);
            Assert.AreEqual(1, result.SkippedCount);
            Assert.AreEqual(1, result.WarningCount);
            StringAssert.Contains(settings.Items.Single().ReferenceWarning, "Missing");
        }

        [TestMethod]
        public void DeserializeFromUnity_RejectsUnsupportedSchemaVersion()
        {
            Assert.ThrowsException<System.InvalidOperationException>(() =>
                TrainingCatalogSyncService.DeserializeFromUnity("{\"schemaVersion\":2}"));
        }

        [TestMethod]
        public void RefreshReferenceWarnings_DetectsCyclesSelfReferencesAndMissingNodes()
        {
            TrainingCatalogSettings settings = new TrainingCatalogSettings
            {
                Items = new ObservableCollection<TrainingCatalogItem>
                {
                    new TrainingCatalogItem
                    {
                        TrainingId = "A",
                        RequiredCompletedTrainingIds = new List<string> { "B" },
                        UnlockNodeIds = new List<string> { "MissingNode" }
                    },
                    new TrainingCatalogItem
                    {
                        TrainingId = "B",
                        RequiredCompletedTrainingIds = new List<string> { "A" }
                    },
                    new TrainingCatalogItem
                    {
                        TrainingId = "Self",
                        RequiredCompletedTrainingIds = new List<string> { "Self" }
                    }
                }
            };

            int warnings = TrainingCatalogSyncService.RefreshReferenceWarnings(
                settings, new[] { "ExistingNode" });

            Assert.IsTrue(warnings >= 4);
            StringAssert.Contains(settings.Items[0].ReferenceWarning, "MissingNode");
            StringAssert.Contains(settings.Items[0].ReferenceWarning, "循環");
            StringAssert.Contains(settings.Items[1].ReferenceWarning, "循環");
            StringAssert.Contains(settings.Items[2].ReferenceWarning, "自分自身");
        }

        [TestMethod]
        public void ImportAndExport_RoundTripsAvailabilityConditions()
        {
            FromUnityTrainingCatalogDataFile imported = TrainingCatalogSyncService.DeserializeFromUnity(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                "\"trainingId\":\"Limited\",\"displayName\":\"限定\",\"occurrenceType\":\"OncePerSave\"," +
                "\"visibleConditionRanks\":[\"Excellent\"],\"requiredCompletedTrainingIds\":[\"Preparation\"]," +
                "\"unlockNodeIds\":[\"TestHeroine_Limited\"],\"hideAfterCompletion\":true}]}" );
            HeroineProfile profile = new HeroineProfile { HeroineId = "TestHeroine" };
            profile.TrainingCatalog.Items.Add(new TrainingCatalogItem
                { TrainingId = "Preparation", DisplayName = "準備" });
            TrainingCatalogSyncService.MergeFromUnity(profile.TrainingCatalog, profile.HeroineId, imported);

            FromUnityTrainingCatalogDataFile exported = TrainingCatalogSyncService.DeserializeFromUnity(
                TrainingCatalogSyncService.BuildExportJson(profile));
            FromUnityTrainingCatalogItem item = exported.Items.Single(value => value.TrainingId == "Limited");

            Assert.AreEqual("OncePerSave", item.OccurrenceType);
            CollectionAssert.AreEqual(new[] { "Excellent" }, item.VisibleConditionRanks);
            CollectionAssert.AreEqual(new[] { "Preparation" }, item.RequiredCompletedTrainingIds);
            CollectionAssert.AreEqual(new[] { "TestHeroine_Limited" }, item.UnlockNodeIds);
            Assert.IsTrue(item.HideAfterCompletion.Value);
        }

        [TestMethod]
        public void ToolItems_CanBeAddedPreservedByUnityMergeAndRemovedSafely()
        {
            var settings = new TrainingCatalogSettings();
            TrainingCatalogItem toolItem = TrainingCatalogSyncService.AddToolItem(
                settings, "Custom_Training1", "追加訓練", "Custom");

            Assert.IsTrue(toolItem.IsToolCreated);
            Assert.AreEqual("Tool追加", toolItem.SourceLabel);
            Assert.AreEqual("追加訓練", toolItem.DisplayName);

            FromUnityTrainingCatalogDataFile data = TrainingCatalogSyncService.DeserializeFromUnity(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"items\":[{" +
                "\"trainingId\":\"UnityTraining\",\"displayName\":\"Unity訓練\"}]}");
            TrainingCatalogSyncService.MergeFromUnity(settings, "TestHeroine", data);

            Assert.IsTrue(settings.Items.Any(item => item.TrainingId == "Custom_Training1" && item.IsToolCreated));
            Assert.IsFalse(settings.Items.Single(item => item.TrainingId == "UnityTraining").IsToolCreated);
            Assert.IsFalse(TrainingCatalogSyncService.RemoveToolItem(
                settings, settings.Items.Single(item => item.TrainingId == "UnityTraining")));
            Assert.IsTrue(TrainingCatalogSyncService.RemoveToolItem(settings, toolItem));
        }

        [TestMethod]
        public void AddToolItem_RejectsInvalidAndDuplicateTrainingIds()
        {
            var settings = new TrainingCatalogSettings();
            TrainingCatalogSyncService.AddToolItem(settings, "Custom", "", "");

            Assert.ThrowsException<System.InvalidOperationException>(() =>
                TrainingCatalogSyncService.AddToolItem(settings, "Custom", "重複", ""));
            Assert.ThrowsException<System.InvalidOperationException>(() =>
                TrainingCatalogSyncService.AddToolItem(settings, "1 Invalid", "不正", ""));
        }
    }
}
