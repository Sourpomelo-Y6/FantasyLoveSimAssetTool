using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class OutfitCompositionService
    {
        public static IReadOnlyList<OutfitAccessoryAssetTemplate> CreateAccessoryTemplates(string costumeId)
        {
            if (string.IsNullOrWhiteSpace(costumeId))
                throw new ArgumentException("CostumeId is required.", nameof(costumeId));
            string id = costumeId.Trim();
            return new[]
            {
                new OutfitAccessoryAssetTemplate
                {
                    AssetId = $"Accessory_{id}_Back",
                    LayerKind = "BackAccessory",
                    Memo = $"{id}用の後ろアクセサリー画像",
                    Prompt = "back accessory layer only, behind character, transparent background, isolated accessory, aligned to character canvas"
                },
                new OutfitAccessoryAssetTemplate
                {
                    AssetId = $"Accessory_{id}_Front",
                    LayerKind = "FrontAccessory",
                    Memo = $"{id}用の前アクセサリー画像",
                    Prompt = "front accessory layer only, in front of character, transparent background, isolated accessory, aligned to character canvas"
                }
            };
        }

        public static OutfitCompositionSelection Read(
            IEnumerable<LayerAssetDefinition> layers,
            string costumeId)
        {
            string id = costumeId?.Trim() ?? string.Empty;
            List<LayerAssetDefinition> source = (layers ?? Enumerable.Empty<LayerAssetDefinition>())
                .Where(layer => layer != null &&
                    string.Equals(layer.CostumeId?.Trim(), id, StringComparison.OrdinalIgnoreCase))
                .OrderBy(layer => layer.DrawOrder)
                .ToList();
            return new OutfitCompositionSelection
            {
                CostumeBodyAssetId = FindAssetId(source, "CostumeBody", "Costume"),
                BackAccessoryAssetId = FindAssetId(source, "BackAccessory"),
                FrontAccessoryAssetId = FindAssetId(source, "FrontAccessory", "Accessory")
            };
        }

        public static void Apply(
            ICollection<LayerAssetDefinition> layers,
            string costumeId,
            OutfitCompositionSelection selection)
        {
            if (layers == null) throw new ArgumentNullException(nameof(layers));
            if (string.IsNullOrWhiteSpace(costumeId))
                throw new ArgumentException("CostumeId is required.", nameof(costumeId));
            selection ??= new OutfitCompositionSelection();

            SetSlot(layers, costumeId, "CostumeBody", selection.CostumeBodyAssetId, 40, "衣装本体");
            SetSlot(layers, costumeId, "BackAccessory", selection.BackAccessoryAssetId, 10, "後ろアクセサリー");
            SetSlot(layers, costumeId, "FrontAccessory", selection.FrontAccessoryAssetId, 60, "前アクセサリー");
        }

        private static string FindAssetId(IEnumerable<LayerAssetDefinition> layers, params string[] kinds)
        {
            LayerAssetDefinition layer = layers.FirstOrDefault(item => kinds.Any(kind =>
                string.Equals(item.LayerKind?.Trim(), kind, StringComparison.OrdinalIgnoreCase)));
            return layer?.AssetId ?? string.Empty;
        }

        private static void SetSlot(
            ICollection<LayerAssetDefinition> layers,
            string costumeId,
            string layerKind,
            string assetId,
            int drawOrder,
            string displayName)
        {
            List<LayerAssetDefinition> matches = layers.Where(layer => layer != null &&
                string.Equals(layer.CostumeId?.Trim(), costumeId.Trim(), StringComparison.OrdinalIgnoreCase) &&
                IsSameSlot(layer.LayerKind, layerKind)).ToList();
            string normalizedAssetId = assetId?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(normalizedAssetId))
            {
                foreach (LayerAssetDefinition match in matches) layers.Remove(match);
                return;
            }

            LayerAssetDefinition target = matches.FirstOrDefault();
            if (target == null)
            {
                target = new LayerAssetDefinition();
                layers.Add(target);
            }
            foreach (LayerAssetDefinition duplicate in matches.Skip(1).ToList()) layers.Remove(duplicate);
            target.AssetId = normalizedAssetId;
            target.LayerKind = layerKind;
            target.CostumeId = costumeId.Trim();
            target.ExpressionId = string.Empty;
            target.DisplayName = displayName + ": " + costumeId.Trim();
            target.FileName = normalizedAssetId + ".png";
            target.DrawOrder = drawOrder;
            target.Prompt ??= string.Empty;
        }

        private static bool IsSameSlot(string actualKind, string expectedKind)
        {
            if (string.Equals(actualKind?.Trim(), expectedKind, StringComparison.OrdinalIgnoreCase)) return true;
            return expectedKind == "CostumeBody" && string.Equals(actualKind?.Trim(), "Costume", StringComparison.OrdinalIgnoreCase)
                || expectedKind == "FrontAccessory" && string.Equals(actualKind?.Trim(), "Accessory", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class OutfitCompositionSelection
    {
        public string CostumeBodyAssetId { get; set; } = string.Empty;
        public string BackAccessoryAssetId { get; set; } = string.Empty;
        public string FrontAccessoryAssetId { get; set; } = string.Empty;
    }

    public class OutfitAccessoryAssetTemplate
    {
        public string AssetId { get; set; } = string.Empty;
        public string LayerKind { get; set; } = string.Empty;
        public string Memo { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }
}
