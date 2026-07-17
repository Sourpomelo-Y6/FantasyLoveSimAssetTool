using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Models
{
    public enum ProductionStatusKind
    {
        Complete,
        Partial,
        Missing,
        NotApplicable
    }

    public sealed class ProductionStatusCell
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CharacterId { get; set; } = string.Empty;
        public ProductionStatusKind Kind { get; set; }
        public string Details { get; set; } = string.Empty;
        public int TargetTabIndex { get; set; }
        public IReadOnlyList<ProductionStatusCheckItem> Checks { get; set; } = new List<ProductionStatusCheckItem>();

        public string Symbol => Kind == ProductionStatusKind.Complete ? "○" :
            Kind == ProductionStatusKind.Partial ? "△" :
            Kind == ProductionStatusKind.NotApplicable ? "―" : "×";
    }

    public sealed class ProductionStatusCheckItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public string Details { get; set; } = string.Empty;
        public string Symbol => IsComplete ? "○" : "×";
    }

    public sealed class CharacterProductionStatusRow
    {
        public string CharacterId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public ProductionStatusCell BasicInformation { get; set; }
        public ProductionStatusCell BattleMessages { get; set; }
        public ProductionStatusCell TrainingImages { get; set; }

        public bool HasIncomplete =>
            BasicInformation?.Kind != ProductionStatusKind.Complete ||
            BattleMessages?.Kind != ProductionStatusKind.Complete ||
            TrainingImages?.Kind != ProductionStatusKind.Complete;
    }
}
