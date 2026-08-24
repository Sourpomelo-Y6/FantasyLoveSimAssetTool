using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class LocalAiSettingsServiceTests
    {
        private string root;

        [TestInitialize]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "FantasyLoveSimLocalAiSettingsTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        [TestMethod]
        public void Load_WhenFileDoesNotExist_ReturnsDefaults()
        {
            LocalAiSettings settings = new LocalAiSettingsService(root).Load();

            Assert.AreEqual("http://127.0.0.1:8080", settings.ServerUrl);
            Assert.AreEqual(120, settings.TimeoutSeconds);
        }

        [TestMethod]
        public void SaveAndLoad_RoundTripsConnectionSettings()
        {
            var service = new LocalAiSettingsService(root);
            service.Save(new LocalAiSettings
            {
                ServerUrl = "http://localhost:9000/",
                ModelId = "local-model",
                TimeoutSeconds = 45,
                Temperature = 0.4,
                MaxTokens = 2048
            });

            LocalAiSettings loaded = service.Load();

            Assert.AreEqual("http://localhost:9000/", loaded.ServerUrl);
            Assert.AreEqual("local-model", loaded.ModelId);
            Assert.AreEqual(45, loaded.TimeoutSeconds);
            Assert.AreEqual(0.4, loaded.Temperature);
            Assert.AreEqual(2048, loaded.MaxTokens);
            Assert.IsTrue(File.Exists(service.SettingsPath));
        }

        [TestMethod]
        public void Load_WhenJsonIsInvalid_ReturnsDefaults()
        {
            var service = new LocalAiSettingsService(root);
            Directory.CreateDirectory(Path.GetDirectoryName(service.SettingsPath));
            File.WriteAllText(service.SettingsPath, "not-json");

            LocalAiSettings settings = service.Load();

            Assert.AreEqual("http://127.0.0.1:8080", settings.ServerUrl);
        }
    }
}
