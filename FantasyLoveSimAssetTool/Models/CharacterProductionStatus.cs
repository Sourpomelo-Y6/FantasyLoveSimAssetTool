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

    public enum ProductionStatusTargetKind
    {
        None,
        Conversation,
        Asset,
        BattleSkill,
        TrainingSkill,
        SkillTreeNode,
        Expression,
        Costume,
        LayerAsset
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
        public string CharacterId { get; set; } = string.Empty;
        public int TargetTabIndex { get; set; }
        public ProductionStatusTargetKind TargetKind { get; set; }
        public string TargetId { get; set; } = string.Empty;
        public ConversationDataKind ConversationKind { get; set; }
        public string Symbol => IsComplete ? "○" : "×";
    }

    public sealed class CharacterProductionStatusRow
    {
        public string CharacterId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public ProductionStatusCell BasicInformation { get; set; }
        public ProductionStatusCell BattleMessages { get; set; }
        public ProductionStatusCell TrainingImages { get; set; }
        public ProductionStatusCell Conversations { get; set; }
        public ProductionStatusCell Expressions { get; set; }
        public ProductionStatusCell Costumes { get; set; }
        public ProductionStatusCell BattleSkills { get; set; }
        public ProductionStatusCell SkillTree { get; set; }
        public ProductionStatusCell Events { get; set; }
        public ProductionStatusCell ExportReadiness { get; set; }

        public bool HasIncomplete =>
            BasicInformation?.Kind != ProductionStatusKind.Complete ||
            BattleMessages?.Kind != ProductionStatusKind.Complete ||
            TrainingImages?.Kind != ProductionStatusKind.Complete ||
            Conversations?.Kind != ProductionStatusKind.Complete ||
            Expressions?.Kind != ProductionStatusKind.Complete ||
            Costumes?.Kind != ProductionStatusKind.Complete ||
            BattleSkills?.Kind != ProductionStatusKind.Complete ||
            SkillTree?.Kind != ProductionStatusKind.Complete ||
            Events?.Kind != ProductionStatusKind.Complete ||
            ExportReadiness?.Kind != ProductionStatusKind.Complete;
    }
}
