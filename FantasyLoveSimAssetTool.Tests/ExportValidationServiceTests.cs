using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ExportValidationServiceTests
    {
        [TestMethod]
        public void Validate_MissingAcceptedImage_ReturnsNavigableError()
        {
            string workspace = CreateWorkspace();
            try
            {
                CharacterProjectService project = new CharacterProjectService(workspace);
                HeroineProfile profile = project.CreateCharacter("TestHeroine", "Test");
                profile.Assets.Add(new HeroineAsset
                {
                    AssetId = "MissingImage",
                    Status = AssetStatus.Accepted,
                    StoredPath = Path.Combine("Images", "Event", "missing.png")
                });

                ExportValidationResult result = new ExportValidationService(project).Validate(profile);
                ExportValidationIssue issue = result.Issues.First(x => x.Severity == ExportValidationSeverity.Error);

                Assert.IsFalse(result.CanExport);
                Assert.AreEqual(ProductionStatusTargetKind.Asset, issue.TargetKind);
                Assert.AreEqual("MissingImage", issue.TargetId);
                Assert.AreEqual(3, issue.TargetTabIndex);
            }
            finally
            {
                Directory.Delete(workspace, true);
            }
        }

        [TestMethod]
        public void Validate_EmptyProfile_ReturnsExportCountInformation()
        {
            string workspace = CreateWorkspace();
            try
            {
                CharacterProjectService project = new CharacterProjectService(workspace);
                HeroineProfile profile = project.CreateCharacter("TestHeroine", "Test");

                ExportValidationResult result = new ExportValidationService(project).Validate(profile);

                Assert.IsTrue(result.Issues.Any(x => x.Severity == ExportValidationSeverity.Information &&
                    x.Message.Contains("Accepted画像 0 件")));
            }
            finally
            {
                Directory.Delete(workspace, true);
            }
        }

        [TestMethod]
        public void Validate_UnnamespacedHeroineSkillTreeIds_ReturnsNavigableErrors()
        {
            string workspace = CreateWorkspace();
            try
            {
                CharacterProjectService project = new CharacterProjectService(workspace);
                HeroineProfile profile = project.CreateCharacter("TestHeroine", "Test");
                profile.HeroineSkillTree.TrainingSkills.Add(new HeroineTrainingSkill { SkillId = "SharedSkill" });
                profile.HeroineSkillTree.Nodes.Add(new HeroineSkillTreeNode { NodeId = "SharedNode" });

                ExportValidationResult result = new ExportValidationService(project).Validate(profile);

                Assert.IsTrue(result.Issues.Any(x => x.Severity == ExportValidationSeverity.Error &&
                    x.TargetKind == ProductionStatusTargetKind.TrainingSkill && x.TargetId == "SharedSkill"));
                Assert.IsTrue(result.Issues.Any(x => x.Severity == ExportValidationSeverity.Error &&
                    x.TargetKind == ProductionStatusTargetKind.SkillTreeNode && x.TargetId == "SharedNode"));
            }
            finally
            {
                Directory.Delete(workspace, true);
            }
        }

        private static string CreateWorkspace()
        {
            string path = Path.Combine(Path.GetTempPath(), "FantasyLoveSimAssetToolTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
