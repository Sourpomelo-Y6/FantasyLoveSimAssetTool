using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class LayeredSpriteSyncServiceTests
    {
        [TestMethod]
        public void MergeFromUnity_NormalizesLegacyLayersForEightLayerPreview()
        {
            FromUnityLayeredSpriteDataFile data = LayeredSpriteSyncService.DeserializeFromUnity(
                "{\"schemaVersion\":1,\"heroineId\":\"TestHeroine\",\"defaultCostumeId\":\"Default\"," +
                "\"defaultExpressionId\":\"Neutral\",\"items\":[" +
                "{\"assetId\":\"Costume_Default\",\"layerKind\":\"Costume\",\"costumeId\":\"Default\",\"drawOrder\":10,\"fileName\":\"Costume_Default.png\"}," +
                "{\"assetId\":\"Expression_Neutral\",\"layerKind\":\"Expression\",\"expressionId\":\"Neutral\",\"drawOrder\":20,\"fileName\":\"Expression_Neutral.png\"}]}" );
            List<LayerAssetDefinition> layers = new List<LayerAssetDefinition>();
            List<CostumeDefinition> costumes = new List<CostumeDefinition>();
            List<ExpressionDefinition> expressions = new List<ExpressionDefinition>();

            LayeredSpriteMergeResult result = LayeredSpriteSyncService.MergeFromUnity(
                layers, costumes, expressions, data);

            Assert.AreEqual(2, result.AddedCount);
            Assert.AreEqual("CostumeBody", layers[0].LayerKind);
            Assert.AreEqual("HeadExpression", layers[1].LayerKind);
            Assert.AreEqual("Default", costumes[0].CostumeId);
            Assert.AreEqual("Neutral", expressions[0].ExpressionId);
        }

        [TestMethod]
        public void MergeFromUnity_UpdatesExistingDefinitionWithoutClearingPrompt()
        {
            List<LayerAssetDefinition> layers = new List<LayerAssetDefinition>
            {
                new LayerAssetDefinition { AssetId = "Face", LayerKind = "Expression", Prompt = "keep prompt" }
            };
            FromUnityLayeredSpriteDataFile data = new FromUnityLayeredSpriteDataFile
            {
                SchemaVersion = 1,
                Items = new List<FromUnityLayeredSpriteItem>
                {
                    new FromUnityLayeredSpriteItem
                    {
                        AssetId = "Face", LayerKind = "HeadExpression", ExpressionId = "Smile", DrawOrder = 50
                    }
                }
            };

            LayeredSpriteMergeResult result = LayeredSpriteSyncService.MergeFromUnity(
                layers,
                new List<CostumeDefinition>(),
                new List<ExpressionDefinition>(),
                data);

            Assert.AreEqual(1, result.UpdatedCount);
            Assert.AreEqual("HeadExpression", layers[0].LayerKind);
            Assert.AreEqual("keep prompt", layers[0].Prompt);
        }

        [TestMethod]
        public void DeserializeFromUnity_RejectsUnsupportedSchemaVersion()
        {
            Assert.ThrowsException<System.InvalidOperationException>(() =>
                LayeredSpriteSyncService.DeserializeFromUnity("{\"schemaVersion\":2}"));
        }
    }
}
