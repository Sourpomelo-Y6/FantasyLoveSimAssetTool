using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class StillDefinitionService
    {
        private const string DefinitionDirectoryName = "Definitions";
        private const string ExpressionDefinitionFileName = "expressions.json";
        private const string CostumeDefinitionFileName = "costumes.json";
        private const string LayerAssetDefinitionFileName = "layer_assets.json";

        private readonly IReadOnlyList<StillDefinition> defaultDefinitions;
        private readonly IReadOnlyList<StillDefinition> layerDefinitions;

        public StillDefinitionService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public StillDefinitionService(string workspaceRoot)
        {
            defaultDefinitions = new List<StillDefinition>
            {
                Create("Heroine_Normal", "立ち絵: 通常", AssetUsage.Sprites, "Heroine_Normal.png", "standing character sprite, full body, neutral expression, transparent background"),
                Create("Heroine_Smile", "立ち絵: 笑顔", AssetUsage.Sprites, "Heroine_Smile.png", "standing character sprite, full body, gentle smile, transparent background"),
                Create("Heroine_Spring", "立ち絵: 春服", AssetUsage.Sprites, "Heroine_Spring.png", "standing character sprite, full body, spring outfit, transparent background"),
                Create("Heroine_Summer", "立ち絵: 夏服", AssetUsage.Sprites, "Heroine_Summer.png", "standing character sprite, full body, summer outfit, transparent background"),
                Create("Heroine_Autumn", "立ち絵: 秋服", AssetUsage.Sprites, "Heroine_Autumn.png", "standing character sprite, full body, autumn outfit, transparent background"),
                Create("Heroine_Winter", "立ち絵: 冬服", AssetUsage.Sprites, "Heroine_Winter.png", "standing character sprite, full body, winter outfit, transparent background"),
                Create("Heroine_Dress", "立ち絵: ドレス", AssetUsage.Sprites, "Heroine_Dress.png", "standing character sprite, full body, elegant dress, transparent background"),
                Create("Heroine_NightDress", "立ち絵: ナイトドレス", AssetUsage.Sprites, "Heroine_NightDress.png", "standing character sprite, full body, night dress, transparent background"),
                Create("Heroine_Raincoat", "立ち絵: レインコート", AssetUsage.Sprites, "Heroine_Raincoat.png", "standing character sprite, full body, raincoat, transparent background"),

                Create("GameStartIntro_01", "イベント: 導入", AssetUsage.Event, "GameStartIntro_01.png", "visual novel event still, first meeting scene, warm light, cinematic composition"),
                Create("DayStart_Routine_01", "イベント: 日常開始", AssetUsage.Event, "DayStart_Routine_01.png", "visual novel event still, calm morning routine, relaxed atmosphere, detailed room"),
                Create("DayStart_Rainy_01", "イベント: 雨の日開始", AssetUsage.Event, "DayStart_Rainy_01.png", "visual novel event still, rainy morning, soft window light, quiet mood"),
                Create("WithForest_01", "イベント: 森", AssetUsage.Event, "WithForest_01.png", "visual novel event still, walking together in a forest, dappled sunlight, romantic mood"),
                Create("WithLake_01", "イベント: 湖", AssetUsage.Event, "WithLake_01.png", "visual novel event still, lakeside scene, clear water, gentle breeze, romantic mood"),
                Create("WithCave_01", "イベント: 洞窟", AssetUsage.Event, "WithCave_01.png", "visual novel event still, cave exploration scene, magical light, adventurous mood"),

                Create("Tea_01", "行動: お茶", AssetUsage.Actions, "Tea_01.png", "visual novel event still, drinking tea together, cozy room, warm lighting"),
                Create("Rest_01", "行動: 休憩", AssetUsage.Actions, "Rest_01.png", "visual novel event still, resting together, relaxed pose, peaceful atmosphere"),
                Create("Walk_01", "行動: 散歩", AssetUsage.Actions, "Walk_01.png", "visual novel event still, walking outdoors together, natural sunlight, peaceful path"),
                Create("Gift_01", "行動: 贈り物", AssetUsage.Actions, "Gift_01.png", "visual novel event still, receiving a gift, surprised happy expression, intimate composition"),

                Create("GoodEnding_01", "エンディング: Good", AssetUsage.Ending, "GoodEnding_01.png", "good ending still, emotional smile, hopeful atmosphere, beautiful lighting"),
                Create("NormalEnding_01", "エンディング: Normal", AssetUsage.Ending, "NormalEnding_01.png", "normal ending still, bittersweet smile, calm atmosphere, soft lighting"),
                Create("BadEnding_01", "エンディング: Bad", AssetUsage.Ending, "BadEnding_01.png", "bad ending still, distant expression, lonely atmosphere, subdued lighting")
            };
            Dictionary<string, ExpressionDefinition> expressions = LoadExpressionDefinitions(workspaceRoot);
            Dictionary<string, CostumeDefinition> costumes = LoadCostumeDefinitions(workspaceRoot);
            layerDefinitions = LoadLayerDefinitions(workspaceRoot, expressions, costumes);
        }

        public IReadOnlyList<StillDefinition> GetDefaultDefinitions()
        {
            return defaultDefinitions
                .Concat(layerDefinitions)
                .GroupBy(definition => definition.AssetId)
                .Select(group => Clone(group.First()))
                .ToList();
        }

        private static StillDefinition Create(string assetId, string displayName, AssetUsage usage, string fileName, string specificPrompt)
        {
            return new StillDefinition
            {
                AssetId = assetId,
                DisplayName = displayName,
                Usage = usage,
                FileName = fileName,
                SpecificPrompt = specificPrompt,
                Status = StillStatus.NotGenerated
            };
        }

        private static StillDefinition Clone(StillDefinition source)
        {
            return new StillDefinition
            {
                AssetId = source.AssetId,
                DisplayName = source.DisplayName,
                Usage = source.Usage,
                FileName = source.FileName,
                SpecificPrompt = source.SpecificPrompt,
                NegativePromptAddition = source.NegativePromptAddition,
                Status = source.Status
            };
        }

        private static IReadOnlyList<StillDefinition> LoadLayerDefinitions(
            string workspaceRoot,
            Dictionary<string, ExpressionDefinition> expressions,
            Dictionary<string, CostumeDefinition> costumes)
        {
            string path = Path.Combine(workspaceRoot, DefinitionDirectoryName, LayerAssetDefinitionFileName);
            if (!File.Exists(path))
            {
                return new List<StillDefinition>();
            }

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                LayerAssetDefinitionFile definitionFile = JsonSerializer.Deserialize<LayerAssetDefinitionFile>(
                    File.ReadAllText(path),
                    options);
                if (definitionFile == null || definitionFile.Layers == null)
                {
                    return new List<StillDefinition>();
                }

                return definitionFile.Layers
                    .Where(IsValidLayerDefinition)
                    .Select(layer => Create(
                        layer.AssetId.Trim(),
                        layer.DisplayName.Trim(),
                        AssetUsage.Sprites,
                        layer.FileName.Trim(),
                        BuildLayerPrompt(layer, expressions, costumes)))
                    .ToList();
            }
            catch
            {
                return new List<StillDefinition>();
            }
        }

        private static bool IsValidLayerDefinition(LayerAssetDefinition layer)
        {
            return layer != null
                && !string.IsNullOrWhiteSpace(layer.AssetId)
                && !string.IsNullOrWhiteSpace(layer.DisplayName)
                && !string.IsNullOrWhiteSpace(layer.FileName)
                && !string.IsNullOrWhiteSpace(layer.Prompt);
        }

        private static Dictionary<string, ExpressionDefinition> LoadExpressionDefinitions(string workspaceRoot)
        {
            string path = Path.Combine(workspaceRoot, DefinitionDirectoryName, ExpressionDefinitionFileName);
            if (!File.Exists(path))
            {
                return new Dictionary<string, ExpressionDefinition>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                ExpressionDefinitionFile definitionFile = JsonSerializer.Deserialize<ExpressionDefinitionFile>(
                    File.ReadAllText(path),
                    options);

                return (definitionFile?.Expressions ?? new List<ExpressionDefinition>())
                    .Where(expression => expression != null && !string.IsNullOrWhiteSpace(expression.ExpressionId))
                    .GroupBy(expression => expression.ExpressionId.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, ExpressionDefinition>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static Dictionary<string, CostumeDefinition> LoadCostumeDefinitions(string workspaceRoot)
        {
            string path = Path.Combine(workspaceRoot, DefinitionDirectoryName, CostumeDefinitionFileName);
            if (!File.Exists(path))
            {
                return new Dictionary<string, CostumeDefinition>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                CostumeDefinitionFile definitionFile = JsonSerializer.Deserialize<CostumeDefinitionFile>(
                    File.ReadAllText(path),
                    options);

                return (definitionFile?.Costumes ?? new List<CostumeDefinition>())
                    .Where(costume => costume != null && !string.IsNullOrWhiteSpace(costume.CostumeId))
                    .GroupBy(costume => costume.CostumeId.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, CostumeDefinition>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string BuildLayerPrompt(
            LayerAssetDefinition layer,
            Dictionary<string, ExpressionDefinition> expressions,
            Dictionary<string, CostumeDefinition> costumes)
        {
            List<string> parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(layer.LayerKind))
            {
                parts.Add("sprite layer kind: " + layer.LayerKind.Trim());
            }

            if (!string.IsNullOrWhiteSpace(layer.CostumeId))
            {
                parts.Add("costume id: " + layer.CostumeId.Trim());
                if (costumes.TryGetValue(layer.CostumeId.Trim(), out CostumeDefinition costume)
                    && !string.IsNullOrWhiteSpace(costume.Prompt))
                {
                    parts.Add(costume.Prompt.Trim());
                }
            }

            if (!string.IsNullOrWhiteSpace(layer.ExpressionId))
            {
                parts.Add("expression id: " + layer.ExpressionId.Trim());
                if (expressions.TryGetValue(layer.ExpressionId.Trim(), out ExpressionDefinition expression)
                    && !string.IsNullOrWhiteSpace(expression.Prompt))
                {
                    parts.Add(expression.Prompt.Trim());
                }
            }

            parts.Add(layer.Prompt.Trim());
            return string.Join(", ", parts);
        }

        private class LayerAssetDefinitionFile
        {
            public int SchemaVersion { get; set; }

            public List<LayerAssetDefinition> Layers { get; set; }
        }

        private class LayerAssetDefinition
        {
            public string AssetId { get; set; }

            public string LayerKind { get; set; }

            public string CostumeId { get; set; }

            public string ExpressionId { get; set; }

            public string DisplayName { get; set; }

            public string FileName { get; set; }

            public int DrawOrder { get; set; }

            public string Prompt { get; set; }
        }

        private class ExpressionDefinitionFile
        {
            public int SchemaVersion { get; set; }

            public List<ExpressionDefinition> Expressions { get; set; }
        }

        private class ExpressionDefinition
        {
            public string ExpressionId { get; set; }

            public string DisplayName { get; set; }

            public string Prompt { get; set; }

            public string UnityExpressionId { get; set; }
        }

        private class CostumeDefinitionFile
        {
            public int SchemaVersion { get; set; }

            public List<CostumeDefinition> Costumes { get; set; }
        }

        private class CostumeDefinition
        {
            public string CostumeId { get; set; }

            public string DisplayName { get; set; }

            public string Prompt { get; set; }

            public string UnityCostumeId { get; set; }
        }
    }
}
