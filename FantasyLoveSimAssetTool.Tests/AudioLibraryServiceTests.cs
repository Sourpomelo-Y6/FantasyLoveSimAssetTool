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

        private void WriteAudio(string relativePath)
        {
            string path = Path.Combine(
                tempRoot,
                "Assets",
                "Resources",
                "Audio",
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, new byte[] { 0 });
        }
    }
}
