using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public static class LayeredSpriteSyncService
    {
        public static FromUnityLayeredSpriteDataFile DeserializeFromUnity(string json)
        {
            FromUnityLayeredSpriteDataFile data = JsonSerializer.Deserialize<FromUnityLayeredSpriteDataFile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data == null)
            {
                throw new InvalidOperationException("レイヤーデータJSONを読み込めませんでした。");
            }
            if (data.SchemaVersion != 1)
            {
                throw new InvalidOperationException($"未対応の schemaVersion です: {data.SchemaVersion}");
            }
            data.Items ??= new List<FromUnityLayeredSpriteItem>();
            return data;
        }

        public static LayeredSpriteMergeResult MergeFromUnity(
            ICollection<LayerAssetDefinition> layers,
            ICollection<CostumeDefinition> costumes,
            ICollection<ExpressionDefinition> expressions,
            FromUnityLayeredSpriteDataFile data)
        {
            if (layers == null) throw new ArgumentNullException(nameof(layers));
            if (costumes == null) throw new ArgumentNullException(nameof(costumes));
            if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            if (data == null) throw new ArgumentNullException(nameof(data));

            LayeredSpriteMergeResult result = new LayeredSpriteMergeResult();
            HashSet<string> importedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (FromUnityLayeredSpriteItem source in data.Items ?? new List<FromUnityLayeredSpriteItem>())
            {
                string assetId = source?.AssetId?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(assetId) || !importedIds.Add(assetId))
                {
                    result.SkippedCount++;
                    continue;
                }

                string layerKind = NormalizeLayerKind(source.LayerKind);
                if (string.IsNullOrEmpty(layerKind))
                {
                    result.SkippedCount++;
                    continue;
                }

                LayerAssetDefinition target = layers.FirstOrDefault(item =>
                    item != null && string.Equals(item.AssetId, assetId, StringComparison.OrdinalIgnoreCase));
                if (target == null)
                {
                    target = new LayerAssetDefinition { AssetId = assetId, Prompt = string.Empty };
                    layers.Add(target);
                    result.AddedCount++;
                }
                else
                {
                    result.UpdatedCount++;
                }

                target.LayerKind = layerKind;
                target.CostumeId = source.CostumeId?.Trim() ?? string.Empty;
                target.ExpressionId = source.ExpressionId?.Trim() ?? string.Empty;
                target.DisplayName = string.IsNullOrWhiteSpace(source.DisplayName) ? assetId : source.DisplayName;
                target.FileName = string.IsNullOrWhiteSpace(source.FileName) ? assetId + ".png" : source.FileName;
                target.DrawOrder = source.DrawOrder;
                target.Prompt ??= string.Empty;
                result.ImportedItems.Add(source);

                AddCostumeIfMissing(costumes, target.CostumeId);
                AddExpressionIfMissing(expressions, target.ExpressionId);
            }

            AddCostumeIfMissing(costumes, data.DefaultCostumeId);
            AddExpressionIfMissing(expressions, data.DefaultExpressionId);
            return result;
        }

        public static string NormalizeLayerKind(string layerKind)
        {
            string value = layerKind?.Trim() ?? string.Empty;
            if (value.Equals("BaseBody", StringComparison.OrdinalIgnoreCase)) return "BackHair";
            if (value.Equals("Costume", StringComparison.OrdinalIgnoreCase)) return "CostumeBody";
            if (value.Equals("Expression", StringComparison.OrdinalIgnoreCase)) return "HeadExpression";
            if (value.Equals("Accessory", StringComparison.OrdinalIgnoreCase)) return "FrontAccessory";

            string[] known =
            {
                "Background", "BackAccessory", "BackHair", "CostumeBody",
                "HeadExpression", "FrontAccessory", "FrontArm", "Effect"
            };
            return known.FirstOrDefault(item => item.Equals(value, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }

        private static void AddCostumeIfMissing(ICollection<CostumeDefinition> costumes, string costumeId)
        {
            string id = costumeId?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(id) || costumes.Any(item => item != null &&
                string.Equals(item.CostumeId, id, StringComparison.OrdinalIgnoreCase))) return;
            costumes.Add(new CostumeDefinition
            {
                CostumeId = id,
                UnityCostumeId = id,
                DisplayName = id,
                Prompt = string.Empty
            });
        }

        private static void AddExpressionIfMissing(ICollection<ExpressionDefinition> expressions, string expressionId)
        {
            string id = expressionId?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(id) || expressions.Any(item => item != null &&
                string.Equals(item.ExpressionId, id, StringComparison.OrdinalIgnoreCase))) return;
            expressions.Add(new ExpressionDefinition
            {
                ExpressionId = id,
                UnityExpressionId = id,
                DisplayName = id,
                Prompt = string.Empty
            });
        }
    }

    public class LayeredSpriteMergeResult
    {
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<FromUnityLayeredSpriteItem> ImportedItems { get; } = new List<FromUnityLayeredSpriteItem>();
    }
}
