using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FantasyLoveSimAssetTool.Models;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class CharacterProjectServiceTests
    {
        private string workspaceRoot;

        [TestInitialize]
        public void SetUp()
        {
            workspaceRoot = Path.Combine(
                Path.GetTempPath(),
                "FantasyLoveSimAssetToolTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workspaceRoot);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(workspaceRoot))
            {
                Directory.Delete(workspaceRoot, true);
            }
        }

        [TestMethod]
        public void CreateCharacter_DoesNotOverwriteExistingProfileWithoutApproval()
        {
            CharacterProjectService service = new CharacterProjectService(workspaceRoot);
            service.CreateCharacter("TestHeroine", "最初の名前");
            string profilePath = service.GetProfilePath("TestHeroine");
            string originalJson = File.ReadAllText(profilePath);

            Assert.ThrowsException<InvalidOperationException>(
                () => service.CreateCharacter("TestHeroine", "上書き後の名前"));

            Assert.AreEqual(originalJson, File.ReadAllText(profilePath));
        }

        [TestMethod]
        public void CreateCharacter_OverwritesExistingProfileWhenExplicitlyApproved()
        {
            CharacterProjectService service = new CharacterProjectService(workspaceRoot);
            service.CreateCharacter("TestHeroine", "最初の名前");

            var overwritten = service.CreateCharacter(
                "TestHeroine",
                "上書き後の名前",
                overwriteExisting: true);

            Assert.AreEqual("上書き後の名前", overwritten.DisplayName);
            Assert.AreEqual(
                "上書き後の名前",
                service.LoadProfile("TestHeroine").DisplayName);
        }

        [TestMethod]
        public void SaveProfile_CreatesLimitedBackupsAndLeavesNoTemporaryFile()
        {
            CharacterProjectService service = new CharacterProjectService(workspaceRoot);
            var profile = service.CreateCharacter("TestHeroine", "最初の名前");

            for (int i = 0; i < 8; i++)
            {
                profile.DisplayName = "名前" + i;
                service.SaveProfile(profile);
            }

            string characterDirectory = service.GetCharacterDirectory("TestHeroine");
            Assert.AreEqual(5, Directory.GetFiles(Path.Combine(characterDirectory, "Backups"), "profile_*.json").Length);
            Assert.IsFalse(File.Exists(service.GetProfilePath("TestHeroine") + ".tmp"));
            Assert.AreEqual("名前7", service.LoadProfile("TestHeroine").DisplayName);
        }

        [TestMethod]
        public void SaveAndLoadProfile_PreservesTrainingAvailabilityConditions()
        {
            CharacterProjectService service = new CharacterProjectService(workspaceRoot);
            var profile = service.CreateCharacter("TestHeroine", "テスト");
            profile.TrainingCatalog.Items.Add(new FantasyLoveSimAssetTool.Models.TrainingCatalogItem
            {
                TrainingId = "Limited",
                DisplayName = "限定訓練",
                OccurrenceType = "OncePerSave",
                VisibleConditionRanks = new List<string> { "Excellent" },
                ExecutableConditionRanks = new List<string> { "Good", "Excellent" },
                RequiredCompletedTrainingIds = new List<string> { "Preparation" },
                UnlockNodeIds = new List<string> { "TestHeroine_Limited" },
                HideAfterCompletion = true
            });
            service.SaveProfile(profile);

            FantasyLoveSimAssetTool.Models.TrainingCatalogItem loaded = service.LoadProfile("TestHeroine")
                .TrainingCatalog.Items[0];

            Assert.AreEqual("OncePerSave", loaded.OccurrenceType);
            CollectionAssert.AreEqual(new[] { "Excellent" }, loaded.VisibleConditionRanks);
            CollectionAssert.AreEqual(new[] { "Good", "Excellent" }, loaded.ExecutableConditionRanks);
            CollectionAssert.AreEqual(new[] { "Preparation" }, loaded.RequiredCompletedTrainingIds);
            CollectionAssert.AreEqual(new[] { "TestHeroine_Limited" }, loaded.UnlockNodeIds);
            Assert.IsTrue(loaded.HideAfterCompletion);
        }

        [TestMethod]
        public void StillImageRegistration_OverwriteReloadAndUnregisterPreserveExpectedFiles()
        {
            CharacterProjectService service = new CharacterProjectService(workspaceRoot);
            HeroineProfile profile = service.CreateCharacter("TestHeroine", "テスト");
            string firstSource = Path.Combine(workspaceRoot, "first.png");
            string replacementSource = Path.Combine(workspaceRoot, "replacement.png");
            File.WriteAllBytes(firstSource, new byte[] { 1, 2, 3 });
            File.WriteAllBytes(replacementSource, new byte[] { 4, 5, 6, 7 });

            service.AddImageAsset(profile, firstSource, AssetUsage.Event,
                "Event_Test", AssetStatus.Pending);
            HeroineAsset replaced = service.AddImageAsset(profile, replacementSource,
                AssetUsage.Event, "Event_Test", AssetStatus.Accepted, overwriteExisting: true);

            HeroineAsset loaded = service.LoadProfile("TestHeroine").Assets.Single();
            string storedPath = Path.Combine(service.GetCharacterDirectory("TestHeroine"), loaded.StoredPath);
            Assert.AreEqual("Event_Test", loaded.AssetId);
            Assert.AreEqual(AssetStatus.Accepted, loaded.Status);
            CollectionAssert.AreEqual(new byte[] { 4, 5, 6, 7 }, File.ReadAllBytes(storedPath));

            Assert.IsTrue(service.UnregisterImageAsset(profile, replaced));
            Assert.AreEqual(0, service.LoadProfile("TestHeroine").Assets.Count);
            Assert.IsTrue(File.Exists(storedPath), "登録解除では画像ファイルを削除しない契約です。");
        }

        [TestMethod]
        public void SaveAndLoadProfile_PreservesHiddenStillWorkItems()
        {
            CharacterProjectService service = new CharacterProjectService(workspaceRoot);
            HeroineProfile profile = service.CreateCharacter("TestHeroine", "テスト");
            profile.StillWorkItems.Add(new StillWorkItem
            {
                AssetId = "LegacyExpression",
                IsHidden = true
            });

            service.SaveProfile(profile);

            StillWorkItem loaded = service.LoadProfile("TestHeroine").StillWorkItems.Single();
            Assert.AreEqual("LegacyExpression", loaded.AssetId);
            Assert.IsTrue(loaded.IsHidden);
        }
    }
}
