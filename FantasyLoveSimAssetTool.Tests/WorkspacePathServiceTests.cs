using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class WorkspacePathServiceTests
    {
        private string root;

        [TestInitialize]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "FantasyLoveSimWorkspaceTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        [TestMethod]
        public void ResolveWorkspaceRoot_PreservesConfiguredLocation()
        {
            string settings = Path.Combine(root, "settings", "workspace.json");
            string configured = Path.Combine(root, "custom");
            var service = new WorkspacePathService(settings, Path.Combine(root, "default"));

            service.SaveWorkspaceRoot(configured);

            Assert.AreEqual(Path.GetFullPath(configured), service.ResolveWorkspaceRoot());
        }

        [TestMethod]
        public void Migrate_CopiesDataAndBacksUpDestinationConflicts()
        {
            string source = Path.Combine(root, "source");
            string destination = Path.Combine(root, "destination");
            Directory.CreateDirectory(Path.Combine(source, "Characters", "Heroine3"));
            Directory.CreateDirectory(Path.Combine(destination, "Characters", "Heroine3"));
            File.WriteAllText(Path.Combine(source, "Characters", "Heroine3", "profile.json"), "source");
            File.WriteAllText(Path.Combine(destination, "Characters", "Heroine3", "profile.json"), "destination");
            var service = new WorkspacePathService(Path.Combine(root, "settings.json"), destination);

            WorkspaceMigrationResult result = service.Migrate(source, destination);

            Assert.AreEqual("source", File.ReadAllText(Path.Combine(destination, "Characters", "Heroine3", "profile.json")));
            Assert.AreEqual(1, result.CopiedFiles);
            Assert.AreEqual(1, result.BackedUpFiles);
            Assert.AreEqual("destination", File.ReadAllText(Path.Combine(result.BackupPath, "Characters", "Heroine3", "profile.json")));
        }

        [TestMethod]
        public void IsBuildOutputPath_DetectsDebugAndReleaseFolders()
        {
            Assert.IsTrue(WorkspacePathService.IsBuildOutputPath(Path.Combine(root, "bin", "Debug", "net5.0-windows")));
            Assert.IsTrue(WorkspacePathService.IsBuildOutputPath(Path.Combine(root, "bin", "Release", "net5.0-windows")));
            Assert.IsFalse(WorkspacePathService.IsBuildOutputPath(Path.Combine(root, "workspace")));
        }
    }
}
