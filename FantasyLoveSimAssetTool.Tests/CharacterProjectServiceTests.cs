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
    }
}
