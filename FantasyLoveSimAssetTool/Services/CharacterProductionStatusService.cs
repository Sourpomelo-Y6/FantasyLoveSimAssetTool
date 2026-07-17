using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class CharacterProductionStatusService
    {
        private static readonly string[] RequiredResultTypes =
            { "SoloVictory", "DuoVictory", "SoloDefeat", "DuoDefeat" };
        private static readonly string[] RequiredPanelTypes = { "Victory", "Defeat" };

        public static CharacterProductionStatusRow Evaluate(
            HeroineProfile profile,
            IEnumerable<ExpressionDefinition> expressions = null,
            IEnumerable<CostumeDefinition> costumes = null,
            IEnumerable<LayerAssetDefinition> layers = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            List<ExpressionDefinition> expressionList = (expressions ?? Enumerable.Empty<ExpressionDefinition>()).Where(x => x != null).ToList();
            List<CostumeDefinition> costumeList = (costumes ?? Enumerable.Empty<CostumeDefinition>()).Where(x => x != null).ToList();
            List<LayerAssetDefinition> layerList = (layers ?? Enumerable.Empty<LayerAssetDefinition>()).Where(x => x != null).ToList();
            return new CharacterProductionStatusRow
            {
                CharacterId = profile.HeroineId ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.HeroineId : profile.DisplayName,
                BasicInformation = EvaluateBasicInformation(profile),
                BattleMessages = EvaluateBattleMessages(profile),
                TrainingImages = EvaluateTrainingImages(profile),
                Conversations = EvaluateConversations(profile),
                Expressions = EvaluateExpressions(profile, expressionList, layerList),
                Costumes = EvaluateCostumes(profile, costumeList, layerList),
                BattleSkills = EvaluateBattleSkills(profile),
                SkillTree = EvaluateSkillTree(profile)
            };
        }

        private static ProductionStatusCell EvaluateBattleSkills(HeroineProfile profile)
        {
            List<HeroineBattleSkill> skills = profile.BattleSkills?.Where(x => x != null).ToList()
                ?? new List<HeroineBattleSkill>();
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>
            {
                Check("戦闘スキル登録", skills.Count > 0,
                    skills.Count > 0 ? $"{skills.Count} 件登録済みです。" : "戦闘スキルを1件以上登録してください。")
            };
            bool idsValid = skills.All(x => !string.IsNullOrWhiteSpace(x.SkillId)) &&
                skills.Where(x => !string.IsNullOrWhiteSpace(x.SkillId))
                    .GroupBy(x => x.SkillId.Trim(), StringComparer.OrdinalIgnoreCase).All(x => x.Count() == 1);
            checks.Add(Check("SkillId", idsValid, idsValid ? "空ID・重複IDはありません。" : "空IDまたは重複IDがあります。"));
            foreach (HeroineBattleSkill skill in skills)
            {
                string label = string.IsNullOrWhiteSpace(skill.SkillId) ? "SkillId未設定" : skill.SkillId.Trim();
                bool valid = !string.IsNullOrWhiteSpace(skill.DisplayName) &&
                    !string.IsNullOrWhiteSpace(skill.EffectType) && !string.IsNullOrWhiteSpace(skill.Target) &&
                    skill.Cost >= 0 && skill.Power >= 0 && skill.StatusDurationTurns >= 0 &&
                    skill.UseChancePercent >= 0 && skill.UseChancePercent <= 100 && skill.MaxUsesPerBattle >= 0;
                checks.Add(Check($"戦闘スキル {label}", valid, valid
                    ? $"{skill.DisplayName} / {skill.EffectType} / {skill.Target} / MP {skill.Cost}"
                    : "表示名、効果、対象、MP・威力・期間・確率・回数の値を確認してください。"));
            }
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "戦闘スキル", 0, Kind(complete, checks.Count),
                $"完成条件 {complete}/{checks.Count}。登録、ID、効果設定を確認します。", checks);
        }

        private static ProductionStatusCell EvaluateSkillTree(HeroineProfile profile)
        {
            List<HeroineTrainingSkill> trainingSkills = profile.HeroineSkillTree?.TrainingSkills?.Where(x => x != null).ToList()
                ?? new List<HeroineTrainingSkill>();
            List<HeroineSkillTreeNode> nodes = profile.HeroineSkillTree?.Nodes?.Where(x => x != null).ToList()
                ?? new List<HeroineSkillTreeNode>();
            HashSet<string> battleSkillIds = new HashSet<string>((profile.BattleSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineBattleSkill>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.SkillId)).Select(x => x.SkillId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> trainingSkillIds = new HashSet<string>(trainingSkills
                .Where(x => !string.IsNullOrWhiteSpace(x.SkillId)).Select(x => x.SkillId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> trainingIds = new HashSet<string>((profile.TrainingCatalog?.Items ?? new System.Collections.ObjectModel.ObservableCollection<TrainingCatalogItem>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.TrainingId)).Select(x => x.TrainingId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> nodeIds = new HashSet<string>(nodes
                .Where(x => !string.IsNullOrWhiteSpace(x.NodeId)).Select(x => x.NodeId.Trim()), StringComparer.OrdinalIgnoreCase);
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>
            {
                Check("ツリーノード登録", nodes.Count > 0, nodes.Count > 0 ? $"{nodes.Count} 件登録済みです。" : "ノードを1件以上登録してください。"),
                Check("ルートノード", nodes.Any(x => x.PrerequisiteNodeIds == null || x.PrerequisiteNodeIds.Count == 0),
                    nodes.Any(x => x.PrerequisiteNodeIds == null || x.PrerequisiteNodeIds.Count == 0) ? "前提なしのルートがあります。" : "前提なしのルートノードが必要です。"),
                Check("NodeId", nodes.All(x => !string.IsNullOrWhiteSpace(x.NodeId)) && nodeIds.Count == nodes.Count,
                    nodes.All(x => !string.IsNullOrWhiteSpace(x.NodeId)) && nodeIds.Count == nodes.Count ? "空ID・重複IDはありません。" : "空IDまたは重複IDがあります。")
            };
            bool trainingSkillsValid = trainingSkills.All(x => !string.IsNullOrWhiteSpace(x.SkillId) && !string.IsNullOrWhiteSpace(x.DisplayName)) &&
                trainingSkillIds.Count == trainingSkills.Count;
            checks.Add(Check("訓練SkillId", trainingSkillsValid,
                trainingSkillsValid ? $"訓練スキル {trainingSkills.Count} 件のIDと表示名は有効です。" : "訓練スキルに空ID、重複ID、表示名不足があります。"));
            foreach (HeroineSkillTreeNode node in nodes)
            {
                string label = string.IsNullOrWhiteSpace(node.NodeId) ? "NodeId未設定" : node.NodeId.Trim();
                List<string> problems = new List<string>();
                if (string.IsNullOrWhiteSpace(node.DisplayName)) problems.Add("表示名");
                if (node.SkillPointCost < 0) problems.Add("SP");
                foreach (string id in node.PrerequisiteNodeIds ?? new System.Collections.ObjectModel.ObservableCollection<string>())
                    if (!nodeIds.Contains(id) || string.Equals(id, node.NodeId, StringComparison.OrdinalIgnoreCase)) problems.Add("前提:" + id);
                if (!string.IsNullOrWhiteSpace(node.GrantedHeroineSkillId) && !battleSkillIds.Contains(node.GrantedHeroineSkillId))
                    problems.Add("戦闘Skill:" + node.GrantedHeroineSkillId);
                if (!string.IsNullOrWhiteSpace(node.TrainingSkillId) && !trainingSkillIds.Contains(node.TrainingSkillId))
                    problems.Add("訓練Skill:" + node.TrainingSkillId);
                foreach (string id in node.UnlockedTrainingIds ?? new System.Collections.ObjectModel.ObservableCollection<string>())
                    if (!trainingIds.Contains(id)) problems.Add("解放Training:" + id);
                bool hasReward = !string.IsNullOrWhiteSpace(node.GrantedHeroineSkillId) ||
                    !string.IsNullOrWhiteSpace(node.TrainingSkillId) || (node.UnlockedTrainingIds?.Count ?? 0) > 0;
                if (!hasReward) problems.Add("付与内容なし");
                checks.Add(Check($"ノード {label}", problems.Count == 0,
                    problems.Count == 0 ? "前提と付与先の参照は有効です。" : "要確認: " + string.Join(", ", problems)));
            }
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "スキルツリー", 0, Kind(complete, checks.Count),
                $"完成条件 {complete}/{checks.Count}。ルート、ID、前提、付与スキル、解放訓練を確認します。", checks);
        }

        private static ProductionStatusCell EvaluateConversations(HeroineProfile profile)
        {
            List<ConversationEntry> entries = profile.ConversationEntries?.Where(x => x != null).ToList()
                ?? new List<ConversationEntry>();
            List<ConversationEntry> normal = entries.Where(x => x.Kind == ConversationDataKind.Conversations).ToList();
            bool hasNormal = normal.Count > 0;
            bool hasInitial = !string.IsNullOrWhiteSpace(profile.InitialDialogueMessage);
            bool idsValid = entries.All(x => !string.IsNullOrWhiteSpace(x.Id)) &&
                entries.GroupBy(x => x.Kind).All(kind => kind.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .GroupBy(x => x.Id.Trim(), StringComparer.OrdinalIgnoreCase).All(ids => ids.Count() == 1));
            bool textValid = entries.All(x => x.Lines != null && x.Lines.Count > 0 &&
                x.Lines.All(line => line != null && !string.IsNullOrWhiteSpace(line.Text)));
            ProductionStatusCheckItem[] checks =
            {
                Check("通常会話", hasNormal, hasNormal ? $"{normal.Count} 件登録済みです。" : "通常会話を1件以上登録してください。"),
                Check("開始時メッセージ", hasInitial, hasInitial ? "設定済みです。" : "InitialDialogueMessageが未設定です。"),
                Check("会話ID", idsValid, idsValid ? "空ID・重複IDはありません。" : "空IDまたは同じ種別内の重複IDがあります。"),
                Check("会話本文", textValid, textValid ? "全データに本文があります。" : "台詞行がない、または本文が空のデータがあります。")
            };
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "会話データ", 1, Kind(complete, checks.Length),
                $"完成条件 {complete}/{checks.Length}。通常会話、開始時文、ID、本文を確認します。", checks);
        }

        private static ProductionStatusCell EvaluateExpressions(
            HeroineProfile profile,
            IReadOnlyList<ExpressionDefinition> expressions,
            IReadOnlyList<LayerAssetDefinition> layers)
        {
            HashSet<string> definitionIds = new HashSet<string>(expressions
                .Where(x => !string.IsNullOrWhiteSpace(x.ExpressionId)).Select(x => x.ExpressionId.Trim()), StringComparer.OrdinalIgnoreCase);
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>
            {
                Check("Neutral表情定義", definitionIds.Contains("Neutral"),
                    definitionIds.Contains("Neutral") ? "Neutralを登録済みです。" : "Neutral表情定義が必要です。")
            };
            foreach (string expressionId in definitionIds.OrderBy(x => x))
            {
                LayerAssetDefinition layer = layers.FirstOrDefault(x => IsLayerKind(x, "Expression") &&
                    string.Equals(x.ExpressionId, expressionId, StringComparison.OrdinalIgnoreCase));
                checks.Add(Check($"表情レイヤー {expressionId}", HasAcceptedLayer(profile, layer),
                    BuildLayerDetails(profile, layer)));
            }
            HashSet<string> references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ConversationLine line in (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(x => x?.Lines != null).SelectMany(x => x.Lines).Where(x => x != null && !string.IsNullOrWhiteSpace(x.Expression)))
                references.Add(line.Expression.Trim());
            foreach (BattleResultEventEntry item in profile.BattleMessages?.ResultEvents ?? new System.Collections.ObjectModel.ObservableCollection<BattleResultEventEntry>())
                if (item != null && !string.IsNullOrWhiteSpace(item.ExpressionId)) references.Add(item.ExpressionId.Trim());
            foreach (string reference in references.OrderBy(x => x))
                checks.Add(Check($"表情参照 {reference}", definitionIds.Contains(reference),
                    definitionIds.Contains(reference) ? "登録済み表情を参照しています。" : "参照先の表情定義がありません。"));
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "表情", 8, Kind(complete, checks.Count),
                $"完成条件 {complete}/{checks.Count}。Neutral、表情レイヤー、会話・戦闘からの参照を確認します。", checks);
        }

        private static ProductionStatusCell EvaluateCostumes(
            HeroineProfile profile,
            IReadOnlyList<CostumeDefinition> costumes,
            IReadOnlyList<LayerAssetDefinition> layers)
        {
            HashSet<string> definitionIds = new HashSet<string>(costumes
                .Where(x => !string.IsNullOrWhiteSpace(x.CostumeId)).Select(x => x.CostumeId.Trim()), StringComparer.OrdinalIgnoreCase);
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>
            {
                Check("Default衣装定義", definitionIds.Contains("Default"),
                    definitionIds.Contains("Default") ? "Defaultを登録済みです。" : "Default衣装定義が必要です。")
            };
            foreach (string costumeId in definitionIds.OrderBy(x => x))
            {
                LayerAssetDefinition layer = layers.FirstOrDefault(x => IsLayerKind(x, "Costume") &&
                    string.Equals(x.CostumeId, costumeId, StringComparison.OrdinalIgnoreCase));
                checks.Add(Check($"衣装レイヤー {costumeId}", HasAcceptedLayer(profile, layer), BuildLayerDetails(profile, layer)));
            }
            HashSet<string> references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ConversationEntry entry in profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                if (entry?.Conditions != null && !string.IsNullOrWhiteSpace(entry.Conditions.CostumeId)) references.Add(entry.Conditions.CostumeId.Trim());
            foreach (BattleResultEventEntry item in profile.BattleMessages?.ResultEvents ?? new System.Collections.ObjectModel.ObservableCollection<BattleResultEventEntry>())
                foreach (string id in item?.UnlockedOutfitIds ?? Array.Empty<string>()) if (!string.IsNullOrWhiteSpace(id)) references.Add(id.Trim());
            foreach (string reference in references.OrderBy(x => x))
                checks.Add(Check($"衣装参照 {reference}", definitionIds.Contains(reference),
                    definitionIds.Contains(reference) ? "登録済み衣装を参照しています。" : "参照先の衣装定義がありません。"));
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "衣装", 8, Kind(complete, checks.Count),
                $"完成条件 {complete}/{checks.Count}。Default、衣装レイヤー、会話・戦闘からの参照を確認します。", checks);
        }

        private static ProductionStatusCell EvaluateBasicInformation(HeroineProfile profile)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>
            {
                ["HeroineId"] = profile.HeroineId,
                ["表示名"] = profile.DisplayName,
                ["性格"] = profile.Personality,
                ["口調"] = profile.SpeakingStyle
            };
            string[] missing = fields.Where(x => string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Key).ToArray();
            return Cell(profile, "基本情報", 0,
                missing.Length == 0 ? ProductionStatusKind.Complete :
                missing.Length == fields.Count ? ProductionStatusKind.Missing : ProductionStatusKind.Partial,
                missing.Length == 0 ? "必須4項目を入力済みです。" :
                $"入力済み {fields.Count - missing.Length}/{fields.Count}。不足: {string.Join(", ", missing)}",
                fields.Select(x => Check(x.Key, !string.IsNullOrWhiteSpace(x.Value),
                    string.IsNullOrWhiteSpace(x.Value) ? "未入力です。" : "入力済みです。")).ToArray());
        }

        private static ProductionStatusCell EvaluateBattleMessages(HeroineProfile profile)
        {
            List<BattleResultEventEntry> events = profile.BattleMessages?.ResultEvents?.Where(x => x != null).ToList()
                ?? new List<BattleResultEventEntry>();
            List<BattlePanelResultMessageEntry> panels = profile.BattleMessages?.PanelMessages?.Where(x => x != null).ToList()
                ?? new List<BattlePanelResultMessageEntry>();
            List<string> missing = RequiredResultTypes.Where(type => !events.Any(x =>
                string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)))
                .Select(type => "結果:" + type).ToList();
            missing.AddRange(RequiredPanelTypes.Where(type => !panels.Any(x =>
                string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)))
                .Select(type => "パネル:" + type));
            int completed = RequiredResultTypes.Length + RequiredPanelTypes.Length - missing.Count;
            ProductionStatusKind kind = missing.Count == 0 ? ProductionStatusKind.Complete :
                completed == 0 ? ProductionStatusKind.Missing : ProductionStatusKind.Partial;
            string details = missing.Count == 0
                ? $"必須結果イベント {RequiredResultTypes.Length} 件、パネル文 {RequiredPanelTypes.Length} 件を登録済みです。逃走は任意です。"
                : $"必須項目 {completed}/{RequiredResultTypes.Length + RequiredPanelTypes.Length}。不足: {string.Join(", ", missing)}。逃走は任意です。";
            List<ProductionStatusCheckItem> checks = RequiredResultTypes.Select(type => Check(
                "戦闘結果 " + type,
                events.Any(x => string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)),
                "対応する本文入りイベントが必要です。")).ToList();
            checks.AddRange(RequiredPanelTypes.Select(type => Check(
                "戦闘パネル " + type,
                panels.Any(x => string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)),
                "対応する本文入りパネル文が必要です。")));
            checks.Add(Check("逃走イベント", true, "任意項目のため完成判定には影響しません。"));
            return Cell(profile, "戦闘メッセージ", 2, kind, details, checks);
        }

        private static ProductionStatusCell EvaluateTrainingImages(HeroineProfile profile)
        {
            string[] trainingIds = profile.TrainingCatalog?.Items?
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.TrainingId))
                .Select(x => x.TrainingId.Trim()).Distinct(StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
            if (trainingIds.Length == 0)
            {
                return Cell(profile, "訓練画像", 4, ProductionStatusKind.Missing,
                    "登録済み訓練がありません。先にUnity訓練一覧を読み込んでください。",
                    new[] { Check("訓練一覧", false, "Unity訓練一覧を読み込んでください。") });
            }

            Dictionary<string, HeroineAsset> assets = (profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.AssetId))
                .GroupBy(x => x.AssetId, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.Last(), StringComparer.Ordinal);
            List<string> incomplete = new List<string>();
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>();
            int completeSlots = 0;
            foreach (string trainingId in trainingIds)
            {
                TrainingImageEntry entry = profile.TrainingImages?.Items?.FirstOrDefault(x =>
                    x != null && string.Equals(x.TrainingId, trainingId, StringComparison.Ordinal));
                string[] ids = entry == null ? Array.Empty<string>() : new[]
                {
                    entry.BeforeFirstStepImageAssetId, entry.AfterFirstStepImageAssetId,
                    entry.PlayerLpConsumedImageAssetId, entry.HeroineLpConsumedImageAssetId,
                    entry.SimultaneousLpConsumedImageAssetId
                };
                int accepted = ids.Count(id => !string.IsNullOrWhiteSpace(id) && assets.TryGetValue(id, out HeroineAsset asset) && asset.Status == AssetStatus.Accepted);
                completeSlots += accepted;
                if (accepted < 5) incomplete.Add($"{trainingId} {accepted}/5");
                string[] stateNames = { "開始前", "進行後", "主人公LP消費", "ヒロインLP消費", "同時LP消費" };
                for (int i = 0; i < stateNames.Length; i++)
                {
                    string assetId = i < ids.Length ? ids[i] : string.Empty;
                    bool acceptedSlot = !string.IsNullOrWhiteSpace(assetId) &&
                        assets.TryGetValue(assetId, out HeroineAsset asset) && asset.Status == AssetStatus.Accepted;
                    checks.Add(Check($"{trainingId} / {stateNames[i]}", acceptedSlot,
                        string.IsNullOrWhiteSpace(assetId) ? "画像AssetIdが未設定です。" :
                        acceptedSlot ? $"{assetId} はAcceptedです。" : $"{assetId} は未採用です。"));
                }
            }

            int totalSlots = trainingIds.Length * 5;
            ProductionStatusKind kind = completeSlots == totalSlots ? ProductionStatusKind.Complete :
                completeSlots == 0 ? ProductionStatusKind.Missing : ProductionStatusKind.Partial;
            string details = kind == ProductionStatusKind.Complete
                ? $"登録済み訓練 {trainingIds.Length} 件の全 {totalSlots} 枠がAcceptedです。"
                : $"Accepted {completeSlots}/{totalSlots} 枠。不足: {string.Join(", ", incomplete)}";
            return Cell(profile, "訓練画像", 4, kind, details, checks);
        }

        private static ProductionStatusCheckItem Check(string name, bool complete, string details) =>
            new ProductionStatusCheckItem { Name = name, IsComplete = complete, Details = details };

        private static ProductionStatusKind Kind(int complete, int total) =>
            complete == total ? ProductionStatusKind.Complete :
            complete == 0 ? ProductionStatusKind.Missing : ProductionStatusKind.Partial;

        private static bool IsLayerKind(LayerAssetDefinition layer, string kind) =>
            layer != null && string.Equals(layer.LayerKind, kind, StringComparison.OrdinalIgnoreCase);

        private static bool HasAcceptedLayer(HeroineProfile profile, LayerAssetDefinition layer) =>
            layer != null && !string.IsNullOrWhiteSpace(layer.AssetId) &&
            profile.Assets != null && profile.Assets.Any(asset => asset != null &&
                string.Equals(asset.AssetId, layer.AssetId, StringComparison.OrdinalIgnoreCase) &&
                asset.Status == AssetStatus.Accepted);

        private static string BuildLayerDetails(HeroineProfile profile, LayerAssetDefinition layer)
        {
            if (layer == null) return "対応するレイヤー定義がありません。";
            if (string.IsNullOrWhiteSpace(layer.AssetId)) return "レイヤーのAssetIdが空です。";
            return HasAcceptedLayer(profile, layer)
                ? $"{layer.AssetId} はAcceptedです。"
                : $"{layer.AssetId} に対応するAccepted画像がありません。";
        }

        private static ProductionStatusCell Cell(
            HeroineProfile profile,
            string categoryName,
            int tabIndex,
            ProductionStatusKind kind,
            string details,
            IReadOnlyList<ProductionStatusCheckItem> checks) =>
            new ProductionStatusCell
            {
                CategoryName = categoryName,
                CharacterId = profile.HeroineId ?? string.Empty,
                TargetTabIndex = tabIndex,
                Kind = kind,
                Details = details,
                Checks = checks
            };
    }
}
