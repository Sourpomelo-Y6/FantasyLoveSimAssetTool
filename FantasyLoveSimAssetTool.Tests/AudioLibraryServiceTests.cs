using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class AudioLibraryServiceTests
    {
        private string tempRoot;

        [TestInitialize]
        public void SetUp()
        {
            tempRoot = Path.Combine(
                Path.GetTempPath(),
                "FantasyLoveSimAssetToolTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(tempRoot, "Assets"));
            Directory.CreateDirectory(Path.Combine(tempRoot, "ProjectSettings"));
            File.WriteAllText(
                Path.Combine(tempRoot, "ProjectSettings", "ProjectVersion.txt"),
                "m_EditorVersion: 2021.3.45f2");
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }

        [TestMethod]
        public void Scan_CombinesExpectedSlotsDiscoveredFilesAndVoiceReferences()
        {
            WriteAudio("Bgm/Main.ogg");
            WriteAudio("SE/UI/Confirm.wav");
            WriteAudio("Voice/TestHeroine/Training/Line01.mp3");

            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = "TestHeroine"
            };
            profile.TrainingDialogues.Items.Add(new TrainingDialogueEntry
            {
                Messages =
                {
                    new TrainingDialogueMessage
                    {
                        Text = "音声あり",
                        VoiceId = "Training/Line01"
                    },
                    new TrainingDialogueMessage
                    {
                        Text = "未配置",
                        VoiceId = "Training/Missing01"
                    }
                }
            });

            AudioLibraryScanResult result =
                new AudioLibraryService().Scan(tempRoot, new[] { profile });

            Assert.IsTrue(result.Items.Single(
                item => item.Category == "BGM" && item.LogicalId == "Main").IsAvailable);
            Assert.IsFalse(result.Items.Single(
                item => item.Category == "BGM" && item.LogicalId == "Battle").IsAvailable);

            AudioLibraryItem existingVoice = result.Items.Single(
                item => item.Category == "VOICE" &&
                    item.LogicalId == "TestHeroine/Training/Line01");
            Assert.IsTrue(existingVoice.IsAvailable);
            Assert.AreEqual(1, existingVoice.ReferenceCount);

            AudioLibraryItem missingVoice = result.Items.Single(
                item => item.Category == "VOICE" &&
                    item.LogicalId == "TestHeroine/Training/Missing01");
            Assert.IsFalse(missingVoice.IsAvailable);
            Assert.AreEqual(1, missingVoice.ReferenceCount);
            Assert.IsTrue(missingVoice.ExpectedPath.EndsWith(
                Path.Combine("Voice", "TestHeroine", "Training", "Missing01") + ".*"));
        }

        [TestMethod]
        public void CollectVoiceReferences_AllowsSharedVoiceAndFullResourceIds()
        {
            HeroineProfile profile = new HeroineProfile { HeroineId = "TestHeroine" };
            profile.BattleMessages.ResultEvents.Add(new BattleResultEventEntry
            {
                VoiceId = "Battle/Victory01"
            });
            profile.BattleMessages.PanelMessages.Add(new BattlePanelResultMessageEntry
            {
                VoiceId = "Audio/Voice/TestHeroine/Battle/Victory01"
            });

            var references = AudioLibraryService.CollectVoiceReferences(new[] { profile });

            Assert.AreEqual(1, references.Count);
            Assert.AreEqual(2, references["Voice/TestHeroine/Battle/Victory01"]);
        }

        [TestMethod]
        public void IsUnityProjectPath_RequiresAssetsAndProjectVersion()
        {
            Assert.IsTrue(AudioLibraryService.IsUnityProjectPath(tempRoot));

            File.Delete(Path.Combine(tempRoot, "ProjectSettings", "ProjectVersion.txt"));

            Assert.IsFalse(AudioLibraryService.IsUnityProjectPath(tempRoot));
        }

        [TestMethod]
        public void CreateRegistrationPlan_BuildsCanonicalBgmDestination()
        {
            string sourcePath = Path.Combine(tempRoot, "source.OGG");
            File.WriteAllBytes(sourcePath, new byte[] { 1 });
            AudioLibraryItem item = new AudioLibraryItem
            {
                Category = "BGM",
                LogicalId = "Battle"
            };

            AudioRegistrationPlan plan = new AudioLibraryService()
                .CreateRegistrationPlan(tempRoot, item, sourcePath);

            Assert.AreEqual(
                Path.Combine(
                    tempRoot,
                    "Assets",
                    "Resources",
                    "Audio",
                    "Bgm",
                    "Battle.ogg"),
                plan.DestinationPath);
            Assert.IsFalse(plan.HasConflicts);
        }

        [TestMethod]
        public void CreateRegistrationPlan_DetectsExistingFileWithDifferentExtension()
        {
            WriteAudio("SE/UI/Confirm.wav");
            string sourcePath = Path.Combine(tempRoot, "confirm.ogg");
            File.WriteAllBytes(sourcePath, new byte[] { 2 });
            AudioLibraryItem item = new AudioLibraryItem
            {
                Category = "SE",
                LogicalId = "UI/Confirm"
            };

            AudioRegistrationPlan plan = new AudioLibraryService()
                .CreateRegistrationPlan(tempRoot, item, sourcePath);

            Assert.IsTrue(plan.HasConflicts);
            Assert.AreEqual(1, plan.ExistingPaths.Count);
            Assert.IsTrue(plan.DestinationPath.EndsWith(
                Path.Combine("SE", "UI", "Confirm.ogg")));
        }

        [TestMethod]
        public void RegisterAudio_ReplacesApprovedConflictAndCopiesSource()
        {
            string oldPath = WriteAudio("SE/UI/Confirm.wav");
            File.WriteAllText(oldPath + ".meta", "test meta");
            string sourcePath = Path.Combine(tempRoot, "confirm.ogg");
            File.WriteAllBytes(sourcePath, new byte[] { 7, 8, 9 });
            AudioLibraryItem item = new AudioLibraryItem
            {
                Category = "SE",
                LogicalId = "UI/Confirm"
            };
            AudioLibraryService service = new AudioLibraryService();
            AudioRegistrationPlan plan =
                service.CreateRegistrationPlan(tempRoot, item, sourcePath);

            string destination = service.RegisterAudio(plan, true);

            CollectionAssert.AreEqual(new byte[] { 7, 8, 9 }, File.ReadAllBytes(destination));
            Assert.IsFalse(File.Exists(oldPath));
            Assert.IsFalse(File.Exists(oldPath + ".meta"));
        }

        [TestMethod]
        public void CreateRegistrationPlan_RejectsVoiceAndUnsafeId()
        {
            string sourcePath = Path.Combine(tempRoot, "source.wav");
            File.WriteAllBytes(sourcePath, new byte[] { 1 });
            AudioLibraryService service = new AudioLibraryService();

            Assert.ThrowsException<InvalidOperationException>(() =>
                service.CreateRegistrationPlan(
                    tempRoot,
                    new AudioLibraryItem { Category = "VOICE", LogicalId = "Line01" },
                    sourcePath));
            Assert.ThrowsException<InvalidOperationException>(() =>
                service.CreateRegistrationPlan(
                    tempRoot,
                    new AudioLibraryItem { Category = "SE", LogicalId = "../Outside" },
                    sourcePath));
        }

        [TestMethod]
        public void CreateVoiceRegistrationPlan_BuildsHeroineUsagePath()
        {
            string sourcePath = Path.Combine(tempRoot, "line01.ogg");
            File.WriteAllBytes(sourcePath, new byte[] { 4 });

            AudioRegistrationPlan plan = new AudioLibraryService()
                .CreateVoiceRegistrationPlan(
                    tempRoot,
                    "TestHeroine",
                    "Training",
                    "Line01",
                    sourcePath);

            Assert.AreEqual("TestHeroine/Training/Line01", plan.LogicalId);
            Assert.AreEqual(
                Path.Combine(
                    tempRoot,
                    "Assets",
                    "Resources",
                    "Audio",
                    "Voice",
                    "TestHeroine",
                    "Training",
                    "Line01.ogg"),
                plan.DestinationPath);
        }

        [TestMethod]
        public void CreateVoiceRegistrationPlan_DoesNotDuplicateUsagePrefix()
        {
            string sourcePath = Path.Combine(tempRoot, "victory.wav");
            File.WriteAllBytes(sourcePath, new byte[] { 5 });

            AudioRegistrationPlan plan = new AudioLibraryService()
                .CreateVoiceRegistrationPlan(
                    tempRoot,
                    "TestHeroine",
                    "Battle",
                    "Battle/Victory01",
                    sourcePath);

            Assert.AreEqual("TestHeroine/Battle/Victory01", plan.LogicalId);
        }

        [TestMethod]
        public void CreateVoiceRegistrationPlan_RejectsUnsafeHeroineOrVoiceId()
        {
            string sourcePath = Path.Combine(tempRoot, "line.wav");
            File.WriteAllBytes(sourcePath, new byte[] { 1 });
            AudioLibraryService service = new AudioLibraryService();

            Assert.ThrowsException<InvalidOperationException>(() =>
                service.CreateVoiceRegistrationPlan(
                    tempRoot,
                    "../Heroine",
                    "Training",
                    "Line01",
                    sourcePath));
            Assert.ThrowsException<InvalidOperationException>(() =>
                service.CreateVoiceRegistrationPlan(
                    tempRoot,
                    "TestHeroine",
                    "Training",
                    "../Line01",
                    sourcePath));
        }

        private string WriteAudio(string relativePath)
        {
            string path = Path.Combine(
                tempRoot,
                "Assets",
                "Resources",
                "Audio",
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, new byte[] { 0 });
            return path;
        }
    }
}
