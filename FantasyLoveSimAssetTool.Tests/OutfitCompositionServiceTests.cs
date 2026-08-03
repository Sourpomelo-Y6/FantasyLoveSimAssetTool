using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class OutfitCompositionServiceTests
    {
        [TestMethod]
        public void Apply_CreatesBodyAndFrontBackAccessorySlots()
        {
            List<LayerAssetDefinition> layers = new List<LayerAssetDefinition>();
            OutfitCompositionService.Apply(layers, "Dress", new OutfitCompositionSelection
            {
                CostumeBodyAssetId = "DressBody",
                BackAccessoryAssetId = "DressRibbonBack",
                FrontAccessoryAssetId = "DressRibbonFront"
            });

            Assert.AreEqual(3, layers.Count);
            CollectionAssert.AreEquivalent(
                new[] { "CostumeBody", "BackAccessory", "FrontAccessory" },
                layers.Select(layer => layer.LayerKind).ToArray());
            Assert.IsTrue(layers.All(layer => layer.CostumeId == "Dress"));
        }

        [TestMethod]
        public void Apply_UpdatesLegacyCostumeAndAccessoryWithoutDuplicates()
        {
            List<LayerAssetDefinition> layers = new List<LayerAssetDefinition>
            {
                new LayerAssetDefinition { AssetId = "OldBody", LayerKind = "Costume", CostumeId = "Default" },
                new LayerAssetDefinition { AssetId = "OldFront", LayerKind = "Accessory", CostumeId = "Default" }
            };
            OutfitCompositionService.Apply(layers, "Default", new OutfitCompositionSelection
            {
                CostumeBodyAssetId = "NewBody",
                FrontAccessoryAssetId = "NewFront"
            });

            Assert.AreEqual(2, layers.Count);
            Assert.AreEqual("NewBody", layers.Single(layer => layer.LayerKind == "CostumeBody").AssetId);
            Assert.AreEqual("NewFront", layers.Single(layer => layer.LayerKind == "FrontAccessory").AssetId);
        }

        [TestMethod]
        public void Apply_EmptyAccessoryRemovesOnlySelectedCostumeSlot()
        {
            List<LayerAssetDefinition> layers = new List<LayerAssetDefinition>
            {
                new LayerAssetDefinition { AssetId = "DefaultBack", LayerKind = "BackAccessory", CostumeId = "Default" },
                new LayerAssetDefinition { AssetId = "DressBack", LayerKind = "BackAccessory", CostumeId = "Dress" }
            };
            OutfitCompositionService.Apply(layers, "Dress", new OutfitCompositionSelection());

            Assert.AreEqual(1, layers.Count);
            Assert.AreEqual("DefaultBack", layers[0].AssetId);
        }
    }
}
