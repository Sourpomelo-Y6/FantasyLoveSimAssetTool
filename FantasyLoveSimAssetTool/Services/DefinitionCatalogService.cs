using FantasyLoveSimAssetTool.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class DefinitionCatalogService
    {
        private const string DefinitionDirectoryName = "Definitions";
        private const string ExpressionDefinitionFileName = "expressions.json";
        private const string CostumeDefinitionFileName = "costumes.json";
        private const string LayerAssetDefinitionFileName = "layer_assets.json";

        private readonly string definitionDirectory;
        private readonly JsonSerializerOptions serializerOptions;

        public DefinitionCatalogService(string workspaceRoot)
        {
            definitionDirectory = Path.Combine(workspaceRoot, DefinitionDirectoryName);
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public ExpressionDefinitionFile LoadExpressionDefinitionFile()
        {
            string path = Path.Combine(definitionDirectory, ExpressionDefinitionFileName);
            ExpressionDefinitionFile file = LoadFile<ExpressionDefinitionFile>(path);
            file.SchemaVersion = file.SchemaVersion <= 0 ? 1 : file.SchemaVersion;
            file.Expressions ??= new List<ExpressionDefinition>();
            return file;
        }

        public CostumeDefinitionFile LoadCostumeDefinitionFile()
        {
            string path = Path.Combine(definitionDirectory, CostumeDefinitionFileName);
            CostumeDefinitionFile file = LoadFile<CostumeDefinitionFile>(path);
            file.SchemaVersion = file.SchemaVersion <= 0 ? 1 : file.SchemaVersion;
            file.Costumes ??= new List<CostumeDefinition>();
            return file;
        }

        public LayerAssetDefinitionFile LoadLayerAssetDefinitionFile()
        {
            string path = Path.Combine(definitionDirectory, LayerAssetDefinitionFileName);
            LayerAssetDefinitionFile file = LoadFile<LayerAssetDefinitionFile>(path);
            file.SchemaVersion = file.SchemaVersion <= 0 ? 1 : file.SchemaVersion;
            file.Layers ??= new List<LayerAssetDefinition>();
            foreach (LayerAssetDefinition layer in file.Layers)
            {
                NormalizeLayerAssetDefinition(layer);
            }
            return file;
        }

        public void SaveExpressionDefinitionFile(IEnumerable<ExpressionDefinition> expressions)
        {
            SaveFile(
                Path.Combine(definitionDirectory, ExpressionDefinitionFileName),
                new ExpressionDefinitionFile
                {
                    SchemaVersion = 1,
                    Expressions = new List<ExpressionDefinition>(expressions)
                });
        }

        public void SaveCostumeDefinitionFile(IEnumerable<CostumeDefinition> costumes)
        {
            SaveFile(
                Path.Combine(definitionDirectory, CostumeDefinitionFileName),
                new CostumeDefinitionFile
                {
                    SchemaVersion = 1,
                    Costumes = new List<CostumeDefinition>(costumes)
                });
        }

        public void SaveLayerAssetDefinitionFile(IEnumerable<LayerAssetDefinition> layers)
        {
            SaveFile(
                Path.Combine(definitionDirectory, LayerAssetDefinitionFileName),
                new LayerAssetDefinitionFile
                {
                    SchemaVersion = 1,
                    Layers = new List<LayerAssetDefinition>(layers)
                });
        }

        private T LoadFile<T>(string path)
            where T : new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }

            T file = JsonSerializer.Deserialize<T>(File.ReadAllText(path), serializerOptions);
            return file == null ? new T() : file;
        }

        private void SaveFile<T>(string path, T file)
        {
            Directory.CreateDirectory(definitionDirectory);
            File.WriteAllText(path, JsonSerializer.Serialize(file, serializerOptions));
        }

        public static void NormalizeLayerAssetDefinition(LayerAssetDefinition layer)
        {
            if (layer == null)
            {
                return;
            }

            layer.AssetId = layer.AssetId ?? string.Empty;
            layer.LayerKind = layer.LayerKind ?? string.Empty;
            layer.CostumeId = layer.CostumeId ?? string.Empty;
            layer.ExpressionId = layer.ExpressionId ?? string.Empty;
            layer.DisplayName = layer.DisplayName ?? string.Empty;
            layer.FileName = layer.FileName ?? string.Empty;
            layer.Prompt = layer.Prompt ?? string.Empty;

            if (string.Equals(layer.LayerKind.Trim(), "Costume", System.StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(layer.CostumeId))
            {
                layer.CostumeId = InferLayerId(layer, "Costume_");
            }

            if (string.Equals(layer.LayerKind.Trim(), "Expression", System.StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(layer.ExpressionId))
            {
                layer.ExpressionId = InferLayerId(layer, "Expression_");
            }
        }

        private static string InferLayerId(LayerAssetDefinition layer, string prefix)
        {
            string id = InferLayerIdFromValue(layer.AssetId, prefix);
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            return InferLayerIdFromValue(Path.GetFileNameWithoutExtension(layer.FileName), prefix);
        }

        private static string InferLayerIdFromValue(string value, string prefix)
        {
            if (string.IsNullOrWhiteSpace(value)
                || !value.Trim().StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return value.Trim().Substring(prefix.Length).Trim();
        }
    }
}
