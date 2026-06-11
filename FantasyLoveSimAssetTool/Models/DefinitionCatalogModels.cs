using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public class ExpressionDefinitionFile
    {
        public int SchemaVersion { get; set; }

        public List<ExpressionDefinition> Expressions { get; set; }
    }

    public class ExpressionDefinition
    {
        public string ExpressionId { get; set; }

        public string DisplayName { get; set; }

        public string Prompt { get; set; }

        public string UnityExpressionId { get; set; }
    }

    public class CostumeDefinitionFile
    {
        public int SchemaVersion { get; set; }

        public List<CostumeDefinition> Costumes { get; set; }
    }

    public class CostumeDefinition
    {
        public string CostumeId { get; set; }

        public string DisplayName { get; set; }

        public string Prompt { get; set; }

        public string UnityCostumeId { get; set; }
    }

    public class LayerAssetDefinitionFile
    {
        public int SchemaVersion { get; set; }

        public List<LayerAssetDefinition> Layers { get; set; }
    }

    public class LayerAssetDefinition
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
}
