using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ExportServiceTests
    {
        [TestMethod]
        public void SpriteLayerExport_ReloadsDefinitionsSavedAfterServiceCreation()
        {
            string workspace = Path.Combine(
                Path.GetTempPath(),
                "FantasyLoveSimAssetTool_ExportServiceTests_" + Guid.NewGuid().ToString("N"));

            try
            {
                CharacterProjectService projectService = new CharacterProjectService(workspace);
                ExportService exportService = new ExportService(projectService);
                DefinitionCatalogService catalogService = new DefinitionCatalogService(workspace);
                catalogService.SaveLayerAssetDefinitionFile(new[]
                {
                    new LayerAssetDefinition
                    {
                        AssetId = "Accessory_Town_Front",
                        LayerKind = "FrontAccessory",
                        CostumeId = "Town",
                        DisplayName = "前アクセサリー",
                        FileName = "Accessory_Town_Front.png",
                        DrawOrder = 60
                    }
                });

                HeroineProfile profile = new HeroineProfile { HeroineId = "TestHeroine" };
                List<HeroineAsset> acceptedAssets = new List<HeroineAsset>
                {
                    new HeroineAsset
                    {
                        AssetId = "Accessory_Town_Front",
                        Usage = AssetUsage.Sprites,
                        Status = AssetStatus.Accepted,
                        FileName = "Accessory_Town_Front.png"
                    }
                };
                MethodInfo method = typeof(ExportService).GetMethod(
                    "BuildSpriteLayersExportJson",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                string json = (string)method.Invoke(
                    exportService,
                    new object[] { profile, acceptedAssets, new ExportReport() });

                StringAssert.Contains(json, "Accessory_Town_Front");
                StringAssert.Contains(json, "FrontAccessory");
            }
            finally
            {
                if (Directory.Exists(workspace)) Directory.Delete(workspace, true);
            }
        }
    }
}
