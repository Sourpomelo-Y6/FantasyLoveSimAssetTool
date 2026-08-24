using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class LocalAiInstructionServiceTests
    {
        private string root;

        [TestInitialize]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "FantasyLoveSimLocalAiInstructionTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        [TestMethod]
        public void Load_WhenFileDoesNotExist_ReturnsDefaultInstruction()
        {
            string instruction = new LocalAiInstructionService(root).Load();

            StringAssert.Contains(instruction, "口調");
            StringAssert.Contains(instruction, "JSON");
        }

        [TestMethod]
        public void SaveAndLoad_RoundTripsInstruction()
        {
            var service = new LocalAiInstructionService(root);
            service.Save("独自の共通指示");

            Assert.AreEqual("独自の共通指示", service.Load());
            Assert.IsTrue(File.Exists(service.InstructionPath));
        }
    }
}
