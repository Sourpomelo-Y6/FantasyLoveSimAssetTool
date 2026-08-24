using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    /// <summary>Exportを書き込まず、制作状況と実Exportで共有する検証結果を生成します。</summary>
    public sealed class ExportValidationService
    {
        private static readonly HashSet<string> GameEventTriggerTypes =
            new HashSet<string>(
                new[]
                {
                    "GameStart",
                    "DayStart",
                    "Manual",
                    "ScheduledEventCompleted",
                    "ActionCompleted",
                    "LocationEntered",
                    "QuestCompleted"
                },
                StringComparer.OrdinalIgnoreCase);

        private readonly CharacterProjectService projectService;
        private readonly DefinitionCatalogService catalogService;
        private readonly StillDefinitionService stillService;

        public ExportValidationService(CharacterProjectService projectService)
        {
            this.projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            catalogService = new DefinitionCatalogService(projectService.WorkspaceRoot);
            stillService = new StillDefinitionService(projectService.WorkspaceRoot);
        }

        public ExportValidationResult Validate(HeroineProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            List<ExportValidationIssue> issues = new List<ExportValidationIssue>();
            List<HeroineAsset> accepted = (profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(x => x != null && x.Status == AssetStatus.Accepted).ToList();
            HashSet<string> acceptedIds = IdSet(accepted.Select(x => x.AssetId));
            HashSet<string> expressionIds = IdSet(catalogService.LoadExpressionDefinitionFile().Expressions
                .Where(x => x != null).Select(x => x.ExpressionId));
            HashSet<string> costumeIds = IdSet(catalogService.LoadCostumeDefinitionFile().Costumes
                .Where(x => x != null).Select(x => x.CostumeId));
            HashSet<string> skillIds = IdSet((profile.BattleSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineBattleSkill>())
                .Where(x => x != null).Select(x => x.SkillId));
            foreach (HeroineTrainingSkill skill in profile.HeroineSkillTree?.TrainingSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineTrainingSkill>())
                if (skill != null && !string.IsNullOrWhiteSpace(skill.SkillId)) skillIds.Add(skill.SkillId.Trim());

            if (string.IsNullOrWhiteSpace(profile.HeroineId)) Add(issues, ExportValidationSeverity.Error, "HeroineId が空です。");
            ValidateHeroineSkillTreeNamespaces(issues, profile);
            ValidateCostumeLayers(issues, acceptedIds);
            ValidateOutfitMessageExpressions(issues, profile, expressionIds);
            foreach (HeroineAsset asset in accepted)
            {
                string directory = projectService.GetCharacterDirectory(profile.HeroineId);
                if (string.IsNullOrWhiteSpace(asset.StoredPath) || !File.Exists(Path.Combine(directory, asset.StoredPath)))
                    Add(issues, ExportValidationSeverity.Error, $"{asset.AssetId}: Accepted画像の実ファイルが見つかりません。",
                        ProductionStatusTargetKind.Asset, asset.AssetId, 3);
                if (string.IsNullOrWhiteSpace(asset.PromptRecordPath) || !File.Exists(Path.Combine(directory, asset.PromptRecordPath)))
                    Add(issues, ExportValidationSeverity.Warning, $"{asset.AssetId}: prompt JSON が見つかりません。",
                        ProductionStatusTargetKind.Asset, asset.AssetId, 3);
            }

            List<ConversationEntry> entries = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(x => x != null).ToList();
            foreach (IGrouping<ConversationDataKind, ConversationEntry> kind in entries.GroupBy(x => x.Kind))
                foreach (IGrouping<string, ConversationEntry> duplicate in kind.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .GroupBy(x => x.Id.Trim(), StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
                    Add(issues, ExportValidationSeverity.Error, $"{kind.Key}: ID {duplicate.Key} が重複しています。",
                        ProductionStatusTargetKind.Conversation, duplicate.Key, 1, kind.Key);

            foreach (ConversationEntry entry in entries)
            {
                string label = string.IsNullOrWhiteSpace(entry.Id) ? entry.Kind + "/ID未設定" : entry.Kind + "/" + entry.Id;
                if (string.IsNullOrWhiteSpace(entry.Id)) AddConversation(issues, entry, ExportValidationSeverity.Error, label + ": ID が空です。");
                if (string.IsNullOrWhiteSpace(entry.Title)) AddConversation(issues, entry, ExportValidationSeverity.Warning, label + ": タイトルが空です。");
                if (string.IsNullOrWhiteSpace(entry.Category)) AddConversation(issues, entry, ExportValidationSeverity.Warning, label + ": カテゴリが空です。");
                if (entry.Kind == ConversationDataKind.Conversations &&
                    !ConversationValueCatalog.ConversationGenres.Contains(entry.Category, StringComparer.OrdinalIgnoreCase))
                    AddConversation(issues, entry, ExportValidationSeverity.Warning,
                        label + ": categoryは Daily / Food / Adventure / Love のいずれかにしてください。");
                if (entry.Priority < 0) AddConversation(issues, entry, ExportValidationSeverity.Error, label + ": 優先度が0未満です。");
                if (entry.Kind == ConversationDataKind.GameEvents &&
                    (entry.AffectionChange < -9999 || entry.AffectionChange > 9999))
                    AddConversation(
                        issues,
                        entry,
                        ExportValidationSeverity.Error,
                        label + ": イベント完了時の好感度変化は -9999〜9999 で指定してください。");
                ValidateGameEventTrigger(issues, entry, label);
                if (entry.Lines == null || entry.Lines.Count == 0 || entry.Lines.Any(x => x == null || string.IsNullOrWhiteSpace(x.Text)))
                    AddConversation(issues, entry, ExportValidationSeverity.Error, label + ": 台詞本文が空です。");
                ValidateCondition(issues, entry, label, costumeIds, skillIds);
                if (entry.Kind == ConversationDataKind.Conversations && HasUnsupportedConversationConditions(entry.Conditions))
                    AddConversation(issues, entry, ExportValidationSeverity.Warning,
                        label + ": locationId、actionId、Item、Flag、Skill条件は通常会話のUnity選択処理では使われません。");
                foreach (ConversationLine line in entry.Lines ?? new System.Collections.ObjectModel.ObservableCollection<ConversationLine>())
                    if (line != null && !string.IsNullOrWhiteSpace(line.Expression) && !expressionIds.Contains(line.Expression.Trim()))
                        AddConversation(issues, entry, ExportValidationSeverity.Error, label + $": 表情 {line.Expression} が未登録です。");
                foreach (string id in SplitIds(entry.ImageAssetIdsText))
                    if (!acceptedIds.Contains(id)) AddConversation(issues, entry, ExportValidationSeverity.Error, label + $": 画像 {id} がAcceptedではありません。");
            }

            foreach (string warning in BattleMessageSyncService.Validate(profile,
                stillService.GetDefaultDefinitions().Where(x => x != null).Select(x => x.AssetId), costumeIds, expressionIds))
                Add(issues, ExportValidationSeverity.Warning, warning);
            Add(issues, ExportValidationSeverity.Information,
                $"Export対象: Accepted画像 {accepted.Count} 件、会話・イベント {entries.Count} 件、戦闘スキル {profile.BattleSkills?.Count ?? 0} 件。");
            return new ExportValidationResult { Issues = issues };
        }

        private static void ValidateGameEventTrigger(
            List<ExportValidationIssue> issues,
            ConversationEntry entry,
            string label)
        {
            if (entry.Kind != ConversationDataKind.GameEvents ||
                entry.Conditions == null ||
                string.IsNullOrWhiteSpace(entry.Conditions.GameEventTriggerType))
            {
                return;
            }

            string triggerType = entry.Conditions.GameEventTriggerType.Trim();
            if (!GameEventTriggerTypes.Contains(triggerType))
            {
                AddConversation(
                    issues,
                    entry,
                    ExportValidationSeverity.Error,
                    label + ": 未対応のイベント発火種類です: " + triggerType);
                return;
            }

            if (RequiresGameEventTriggerContext(triggerType) &&
                string.IsNullOrWhiteSpace(entry.Conditions.TriggerContextId))
            {
                AddConversation(
                    issues,
                    entry,
                    ExportValidationSeverity.Error,
                    label + ": " + triggerType + " には発火対象IDが必要です。");
            }
        }

        private static bool RequiresGameEventTriggerContext(string triggerType)
        {
            return string.Equals(triggerType, "ScheduledEventCompleted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(triggerType, "ActionCompleted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(triggerType, "LocationEntered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(triggerType, "QuestCompleted", StringComparison.OrdinalIgnoreCase);
        }

        private void ValidateCostumeLayers(
            List<ExportValidationIssue> issues,
            HashSet<string> acceptedAssetIds)
        {
            IReadOnlyList<CostumeDefinition> costumes = catalogService
                .LoadCostumeDefinitionFile().Costumes
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.CostumeId))
                .ToList();
            IReadOnlyList<LayerAssetDefinition> layers = catalogService
                .LoadLayerAssetDefinitionFile().Layers;

            foreach (CostumeDefinition costume in costumes)
            {
                string costumeId = costume.CostumeId.Trim();
                bool isDefault = string.Equals(
                    costumeId, "Default", StringComparison.OrdinalIgnoreCase);
                LayerAssetDefinition body = layers.FirstOrDefault(layer =>
                    layer != null &&
                    (string.Equals(layer.LayerKind?.Trim(), "CostumeBody", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(layer.LayerKind?.Trim(), "Costume", StringComparison.OrdinalIgnoreCase)) &&
                    string.Equals(layer.CostumeId?.Trim(), costumeId, StringComparison.OrdinalIgnoreCase));
                ExportValidationSeverity severity = isDefault
                    ? ExportValidationSeverity.Error
                    : ExportValidationSeverity.Warning;

                if (body == null || string.IsNullOrWhiteSpace(body.AssetId))
                {
                    Add(
                        issues,
                        severity,
                        $"衣装 {costumeId}: 衣装本体（CostumeBody）が未設定です。",
                        ProductionStatusTargetKind.Costume,
                        costumeId,
                        9);
                    continue;
                }

                if (!acceptedAssetIds.Contains(body.AssetId.Trim()))
                {
                    Add(
                        issues,
                        severity,
                        $"衣装 {costumeId}: 衣装本体 {body.AssetId} がAccepted画像ではありません。",
                        ProductionStatusTargetKind.LayerAsset,
                        body.AssetId,
                        9);
                }
            }
        }

        private static void ValidateHeroineSkillTreeNamespaces(List<ExportValidationIssue> issues, HeroineProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.HeroineId)) return;
            string prefix = profile.HeroineId.Trim() + "_";
            foreach (HeroineTrainingSkill skill in profile.HeroineSkillTree?.TrainingSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineTrainingSkill>())
            {
                if (skill != null && !string.IsNullOrWhiteSpace(skill.SkillId) &&
                    !skill.SkillId.StartsWith(prefix, StringComparison.Ordinal))
                    Add(issues, ExportValidationSeverity.Error,
                        $"訓練SkillId {skill.SkillId} は {prefix} で始めてください。",
                        ProductionStatusTargetKind.TrainingSkill, skill.SkillId, 6);
            }
            foreach (HeroineSkillTreeNode node in profile.HeroineSkillTree?.Nodes ?? new System.Collections.ObjectModel.ObservableCollection<HeroineSkillTreeNode>())
            {
                if (node != null && !string.IsNullOrWhiteSpace(node.NodeId) &&
                    !node.NodeId.StartsWith(prefix, StringComparison.Ordinal))
                    Add(issues, ExportValidationSeverity.Error,
                        $"NodeId {node.NodeId} は {prefix} で始めてください。",
                        ProductionStatusTargetKind.SkillTreeNode, node.NodeId, 6);
            }
        }

        private static void ValidateOutfitMessageExpressions(
            List<ExportValidationIssue> issues,
            HeroineProfile profile,
            HashSet<string> expressionIds)
        {
            foreach (OutfitMessageOverride item in profile.OutfitMessageOverrides ??
                new System.Collections.ObjectModel.ObservableCollection<OutfitMessageOverride>())
            {
                if (item == null) continue;
                string label = string.IsNullOrWhiteSpace(item.OutfitId) ? "OutfitId未設定" : item.OutfitId.Trim();
                ValidateOutfitExpression(issues, expressionIds, item.LockedExpressionId,
                    $"衣装 {label}: 未解放メッセージの表情", ProductionStatusTargetKind.OutfitMessage, label);
                ValidateOutfitExpression(issues, expressionIds, item.ChangedExpressionId,
                    $"衣装 {label}: 着替え完了メッセージの表情", ProductionStatusTargetKind.OutfitMessage, label);
            }

            foreach (OutfitReactionMessageOverride item in profile.OutfitReactionMessageOverrides ??
                new System.Collections.ObjectModel.ObservableCollection<OutfitReactionMessageOverride>())
            {
                if (item == null) continue;
                string label = string.IsNullOrWhiteSpace(item.ReactionType) ? "ReactionType未設定" : item.ReactionType.Trim();
                ValidateOutfitExpression(issues, expressionIds, item.ExpressionId,
                    $"衣装反応 {label}: 表情", ProductionStatusTargetKind.OutfitReactionMessage, label);
            }
        }

        private static void ValidateOutfitExpression(
            List<ExportValidationIssue> issues,
            HashSet<string> expressionIds,
            string expressionId,
            string label,
            ProductionStatusTargetKind targetKind,
            string targetId)
        {
            if (string.IsNullOrWhiteSpace(expressionId))
            {
                Add(issues, ExportValidationSeverity.Error, label + "ID が空です。", targetKind, targetId, 0);
            }
            else if (!expressionIds.Contains(expressionId.Trim()))
            {
                Add(issues, ExportValidationSeverity.Error,
                    label + $"ID {expressionId.Trim()} が差分定義に存在しません。", targetKind, targetId, 0);
            }
        }

        private static void ValidateCondition(List<ExportValidationIssue> issues, ConversationEntry entry, string label,
            HashSet<string> costumeIds, HashSet<string> skillIds)
        {
            if (entry.Conditions == null) return;
            if (entry.Conditions.MinAffection > entry.Conditions.MaxAffection)
                AddConversation(issues, entry, ExportValidationSeverity.Error, label + ": 好感度の最小値が最大値を超えています。");
            if (entry.Kind == ConversationDataKind.GameEvents && entry.Conditions.Once && string.IsNullOrWhiteSpace(entry.Conditions.RequiredFlagIdsText))
                AddConversation(issues, entry, ExportValidationSeverity.Warning, label + ": onceイベントの必須フラグが空です。");
            if (!string.IsNullOrWhiteSpace(entry.Conditions.CostumeId) && !costumeIds.Contains(entry.Conditions.CostumeId.Trim()))
                AddConversation(issues, entry, ExportValidationSeverity.Error, label + $": 衣装 {entry.Conditions.CostumeId} が未登録です。");
            foreach (string id in RequiredSkillIdSyncService.NormalizeText(entry.Conditions.RequiredSkillIdsText))
                if (!skillIds.Contains(id)) AddConversation(issues, entry, ExportValidationSeverity.Error, label + $": スキル {id} が未登録です。");
        }

        private static bool HasUnsupportedConversationConditions(ConversationCondition condition) => condition != null &&
            (!string.IsNullOrWhiteSpace(condition.LocationId) || !string.IsNullOrWhiteSpace(condition.ActionId) ||
             !string.IsNullOrWhiteSpace(condition.RequiredItemId) || !string.IsNullOrWhiteSpace(condition.RequiredFlagIdsText) ||
             !string.IsNullOrWhiteSpace(condition.RequiredSkillIdsText));

        private static HashSet<string> IdSet(IEnumerable<string> values) => new HashSet<string>((values ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase);
        private static void AddConversation(List<ExportValidationIssue> issues, ConversationEntry entry, ExportValidationSeverity severity, string message) =>
            Add(issues, severity, message, ProductionStatusTargetKind.Conversation, entry.Id, 1, entry.Kind);
        private static void Add(List<ExportValidationIssue> issues, ExportValidationSeverity severity, string message,
            ProductionStatusTargetKind targetKind = ProductionStatusTargetKind.None, string targetId = null, int targetTabIndex = 12,
            ConversationDataKind conversationKind = ConversationDataKind.Conversations) => issues.Add(new ExportValidationIssue
            {
                Severity = severity, Message = message, TargetKind = targetKind, TargetId = targetId ?? string.Empty,
                TargetTabIndex = targetTabIndex, ConversationKind = conversationKind
            });
        private static IEnumerable<string> SplitIds(string text) => string.IsNullOrWhiteSpace(text)
            ? Enumerable.Empty<string>()
            : text.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0);
    }
}
