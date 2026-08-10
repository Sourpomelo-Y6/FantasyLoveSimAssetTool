using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

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
    }
}
