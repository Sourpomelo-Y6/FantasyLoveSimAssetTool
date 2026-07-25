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
            IEnumerable<LayerAssetDefinition> layers = null,
            Func<HeroineAsset, bool> acceptedAssetFileExists = null,
            ExportValidationResult exportValidation = null,
            IEnumerable<StillDefinition> stillDefinitions = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            List<ExpressionDefinition> expressionList = (expressions ?? Enumerable.Empty<ExpressionDefinition>()).Where(x => x != null).ToList();
            List<CostumeDefinition> costumeList = (costumes ?? Enumerable.Empty<CostumeDefinition>()).Where(x => x != null).ToList();
            List<LayerAssetDefinition> layerList = (layers ?? Enumerable.Empty<LayerAssetDefinition>()).Where(x => x != null).ToList();
            List<StillDefinition> stillList = (stillDefinitions ?? Enumerable.Empty<StillDefinition>()).Where(x => x != null).ToList();
            CharacterProductionStatusRow row = new CharacterProductionStatusRow
            {
                CharacterId = profile.HeroineId ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.HeroineId : profile.DisplayName,
                BasicInformation = EvaluateBasicInformation(profile),
                BattleMessages = EvaluateBattleMessages(profile),
                TrainingImages = EvaluateTrainingImages(profile),
                TrainingDialogues = EvaluateTrainingDialogues(profile),
                CharacterImages = EvaluateCharacterImages(profile, stillList, acceptedAssetFileExists),
                Conversations = EvaluateConversations(profile),
                Expressions = EvaluateExpressions(profile, expressionList, layerList),
                Costumes = EvaluateCostumes(profile, costumeList, layerList),
                BattleSkills = EvaluateBattleSkills(profile),
                SkillTree = EvaluateSkillTree(profile),
                Events = EvaluateEvents(profile, expressionList, costumeList),
                ActionReactions = EvaluateActionReactions(profile, expressionList, costumeList)
            };
            row.ExportReadiness = EvaluateExportReadiness(profile, row, acceptedAssetFileExists, exportValidation);
            return row;
        }

        private static ProductionStatusCell EvaluateActionReactions(
            HeroineProfile profile,
            IReadOnlyList<ExpressionDefinition> expressions,
            IReadOnlyList<CostumeDefinition> costumes)
        {
            List<ConversationEntry> entries = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(x => x != null && x.Kind == ConversationDataKind.ActionReactions).ToList();
            HashSet<string> expressionIds = new HashSet<string>(expressions.Where(x => !string.IsNullOrWhiteSpace(x.ExpressionId))
                .Select(x => x.ExpressionId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> costumeIds = new HashSet<string>(costumes.Where(x => !string.IsNullOrWhiteSpace(x.CostumeId))
                .Select(x => x.CostumeId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> acceptedAssetIds = new HashSet<string>((profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(x => x != null && x.Status == AssetStatus.Accepted && !string.IsNullOrWhiteSpace(x.AssetId))
                .Select(x => x.AssetId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> skillIds = new HashSet<string>((profile.BattleSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineBattleSkill>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.SkillId)).Select(x => x.SkillId.Trim()), StringComparer.OrdinalIgnoreCase);
            foreach (HeroineTrainingSkill skill in profile.HeroineSkillTree?.TrainingSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineTrainingSkill>())
                if (skill != null && !string.IsNullOrWhiteSpace(skill.SkillId)) skillIds.Add(skill.SkillId.Trim());

            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>();
            foreach (string actionId in ConversationValueCatalog.Actions)
            {
                ConversationEntry match = entries.FirstOrDefault(x => string.Equals(x.Conditions?.ActionId, actionId, StringComparison.OrdinalIgnoreCase));
                checks.Add(Check("主要行動 " + actionId, match != null,
                    match == null ? "対応する行動反応がありません。" : $"{match.Id} を登録済みです。",
                    ProductionStatusTargetKind.Conversation, match?.Id, 1, ConversationDataKind.ActionReactions));
                ConversationEntry fallback = entries.FirstOrDefault(x =>
                    string.Equals(x.Conditions?.ActionId, actionId, StringComparison.OrdinalIgnoreCase) &&
                    IsActionReactionFallback(x));
                checks.Add(Check(actionId + " フォールバック", fallback != null,
                    fallback == null
                        ? "priority 0・一度限りOFF・無条件の反応が必要です。"
                        : fallback.Id + " を無条件のフォールバックとして使用できます。",
                    ProductionStatusTargetKind.Conversation, fallback?.Id ?? match?.Id, 1,
                    ConversationDataKind.ActionReactions));
            }
            bool idsValid = entries.All(x => !string.IsNullOrWhiteSpace(x.Id)) && entries.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id.Trim(), StringComparer.OrdinalIgnoreCase).All(x => x.Count() == 1);
            checks.Add(Check("行動反応ID", idsValid, idsValid ? "空ID・重複IDはありません。" : "空IDまたは重複IDがあります。",
                ProductionStatusTargetKind.Conversation, entries.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Id))?.Id, 1,
                ConversationDataKind.ActionReactions));

            HashSet<string> duplicateIds = new HashSet<string>(entries.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id.Trim(), StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1).Select(x => x.Key),
                StringComparer.OrdinalIgnoreCase);
            HashSet<ConversationEntry> duplicateConditions = new HashSet<ConversationEntry>();
            foreach (IGrouping<string, ConversationEntry> group in entries.GroupBy(BuildActionReactionConditionKey, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
                foreach (ConversationEntry entry in group) duplicateConditions.Add(entry);
            foreach (ConversationEntry entry in entries)
            {
                string label = string.IsNullOrWhiteSpace(entry.Id) ? "ID未設定" : entry.Id.Trim();
                List<string> problems = new List<string>();
                if (string.IsNullOrWhiteSpace(entry.Id)) problems.Add("ID");
                else if (duplicateIds.Contains(entry.Id.Trim())) problems.Add("重複ID");
                if (string.IsNullOrWhiteSpace(entry.Conditions?.ActionId)) problems.Add("actionId");
                if (entry.Priority < 0) problems.Add("優先度");
                if (entry.Lines == null || entry.Lines.Count == 0 || entry.Lines.Any(x => x == null || string.IsNullOrWhiteSpace(x.Text))) problems.Add("台詞本文");
                if (entry.Conditions != null && entry.Conditions.MinAffection > entry.Conditions.MaxAffection) problems.Add("好感度範囲");
                if (duplicateConditions.Contains(entry)) problems.Add("同条件・同優先度の重複");
                foreach (ConversationLine line in entry.Lines ?? new System.Collections.ObjectModel.ObservableCollection<ConversationLine>())
                    if (line != null && !string.IsNullOrWhiteSpace(line.Expression) && !expressionIds.Contains(line.Expression.Trim())) problems.Add("表情:" + line.Expression.Trim());
                if (entry.Conditions != null && !string.IsNullOrWhiteSpace(entry.Conditions.CostumeId) && !costumeIds.Contains(entry.Conditions.CostumeId.Trim()))
                    problems.Add("衣装:" + entry.Conditions.CostumeId.Trim());
                foreach (string id in SplitIds(entry.ImageAssetIdsText)) if (!acceptedAssetIds.Contains(id)) problems.Add("画像:" + id);
                foreach (string id in RequiredSkillIdSyncService.NormalizeText(entry.Conditions?.RequiredSkillIdsText))
                    if (!skillIds.Contains(id)) problems.Add("スキル:" + id);
                checks.Add(Check("行動反応 " + label, problems.Count == 0,
                    problems.Count == 0 ? "本文・条件・参照は有効です。" : "要確認: " + string.Join(", ", problems.Distinct()),
                    ProductionStatusTargetKind.Conversation, entry.Id, 1, ConversationDataKind.ActionReactions));
            }
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "行動反応", 1, Kind(complete, checks.Count),
                $"完成条件 {complete}/{checks.Count}。主要行動、ID、本文、条件、優先度、参照先を確認します。", checks);
        }

        private static string BuildActionReactionConditionKey(ConversationEntry entry)
        {
            ConversationCondition condition = entry?.Conditions ?? new ConversationCondition();
            return string.Join("|", new[]
            {
                condition.ActionId, condition.LocationId, condition.MinAffection.ToString(), condition.MaxAffection.ToString(),
                condition.Weather, condition.Season, condition.TimeOfDay, condition.CostumeId, condition.RequiredItemId,
                condition.RequiredFlagIdsText, condition.RequiredSkillIdsText, condition.Once.ToString(),
                (entry?.Priority ?? 0).ToString()
            }.Select(x => (x ?? string.Empty).Trim()));
        }

        private static bool IsActionReactionFallback(ConversationEntry entry)
        {
            ConversationCondition condition = entry?.Conditions;
            return entry != null && entry.Priority == 0 && condition != null && !condition.Once &&
                condition.MinAffection <= 0 && condition.MaxAffection >= 9999 &&
                string.IsNullOrWhiteSpace(condition.LocationId) &&
                string.IsNullOrWhiteSpace(condition.Weather) &&
                string.IsNullOrWhiteSpace(condition.Season) &&
                string.IsNullOrWhiteSpace(condition.TimeOfDay) &&
                string.IsNullOrWhiteSpace(condition.CostumeId) &&
                string.IsNullOrWhiteSpace(condition.RequiredItemId) &&
                string.IsNullOrWhiteSpace(condition.RequiredFlagIdsText) &&
                string.IsNullOrWhiteSpace(condition.RequiredSkillIdsText);
        }

        private static ProductionStatusCell EvaluateCharacterImages(
            HeroineProfile profile,
            IReadOnlyList<StillDefinition> definitions,
            Func<HeroineAsset, bool> assetFileExists)
        {
            AssetUsage[] targetUsages = { AssetUsage.Sprites, AssetUsage.Battle, AssetUsage.Event, AssetUsage.Actions, AssetUsage.Ending };
            List<HeroineAsset> assets = (profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(x => x != null && targetUsages.Contains(x.Usage) && !string.IsNullOrWhiteSpace(x.AssetId)).ToList();
            Dictionary<string, HeroineAsset> assetById = assets.GroupBy(x => x.AssetId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Last(), StringComparer.OrdinalIgnoreCase);
            Dictionary<string, StillDefinition> definitionById = definitions
                .Where(x => targetUsages.Contains(x.Usage) && !string.IsNullOrWhiteSpace(x.AssetId))
                .GroupBy(x => x.AssetId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
            string[] coreIds =
            {
                "Heroine_Normal", "Battle_Heroine_Idle", "Battle_Heroine_Attack", "Battle_Heroine_Damage",
                "Battle_Heroine_Victory", "Battle_Heroine_Defeat"
            };
            HashSet<string> requiredIds = new HashSet<string>(coreIds.Where(definitionById.ContainsKey), StringComparer.OrdinalIgnoreCase);
            foreach (HeroineAsset asset in assets) requiredIds.Add(asset.AssetId.Trim());
            foreach (ConversationEntry entry in profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                foreach (string id in SplitIds(entry?.ImageAssetIdsText)) requiredIds.Add(id);
            foreach (BattleResultEventEntry entry in profile.BattleMessages?.ResultEvents ?? new System.Collections.ObjectModel.ObservableCollection<BattleResultEventEntry>())
                if (entry != null && !string.IsNullOrWhiteSpace(entry.StillId)) requiredIds.Add(entry.StillId.Trim());

            HashSet<string> allIds = new HashSet<string>(definitionById.Keys, StringComparer.OrdinalIgnoreCase);
            allIds.UnionWith(requiredIds);
            allIds.UnionWith(assetById.Keys);
            Func<HeroineAsset, bool> fileCheck = assetFileExists ?? (asset => !string.IsNullOrWhiteSpace(asset.StoredPath));
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>();
            int requiredCount = 0;
            int completedCount = 0;
            foreach (string id in allIds.OrderBy(x => GetImageUsageOrder(definitionById.TryGetValue(x, out StillDefinition d) ? d.Usage :
                assetById.TryGetValue(x, out HeroineAsset a) ? a.Usage : AssetUsage.Event)).ThenBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                bool required = requiredIds.Contains(id);
                assetById.TryGetValue(id, out HeroineAsset asset);
                definitionById.TryGetValue(id, out StillDefinition definition);
                AssetUsage usage = definition?.Usage ?? asset?.Usage ?? AssetUsage.Event;
                bool complete = asset != null && asset.Status == AssetStatus.Accepted && fileCheck(asset);
                if (required)
                {
                    requiredCount++;
                    if (complete) completedCount++;
                }
                string details = !required ? "現在は未参照の任意候補です。" : asset == null ? "画像Assetが未登録です。" :
                    asset.Status != AssetStatus.Accepted ? $"AssetStatusが {asset.Status} です。" :
                    !fileCheck(asset) ? "Acceptedですが実ファイルが見つかりません。" : "Accepted画像と実ファイルを確認しました。";
                ProductionStatusCheckItem check = Check($"{GetImageUsageLabel(usage)} / {id}", complete, details,
                    definition != null ? ProductionStatusTargetKind.StillDefinition : ProductionStatusTargetKind.Asset,
                    id, definition != null ? 7 : 3);
                check.IsApplicable = required;
                checks.Add(check);
            }
            ProductionStatusKind kind = requiredCount == 0 ? ProductionStatusKind.NotApplicable :
                completedCount == requiredCount ? ProductionStatusKind.Complete :
                completedCount == 0 ? ProductionStatusKind.Missing : ProductionStatusKind.Partial;
            return Cell(profile, "キャラクター画像", 7, kind,
                $"必須画像 {completedCount}/{requiredCount}。未参照の定義候補 {checks.Count(x => !x.IsApplicable)} 件は任意です。", checks);
        }

        private static int GetImageUsageOrder(AssetUsage usage) => usage == AssetUsage.Sprites ? 0 : usage == AssetUsage.Battle ? 1 :
            usage == AssetUsage.Event ? 2 : usage == AssetUsage.Actions ? 3 : usage == AssetUsage.Ending ? 4 : 5;

        private static string GetImageUsageLabel(AssetUsage usage) => usage == AssetUsage.Sprites ? "立ち絵" : usage == AssetUsage.Battle ? "戦闘" :
            usage == AssetUsage.Event ? "イベント" : usage == AssetUsage.Actions ? "行動" : usage == AssetUsage.Ending ? "エンディング" : usage.ToString();

        private static ProductionStatusCell EvaluateTrainingDialogues(HeroineProfile profile)
        {
            string[] states =
            {
                "SelectedBeforeFirstStep", "SelectedAfterFirstStep", "PlayerLpConsumed",
                "HeroineLpConsumed", "SimultaneousLpConsumed"
            };
            string[] trainingIds = profile.TrainingCatalog?.Items?
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.TrainingId))
                .Select(x => x.TrainingId.Trim()).Distinct(StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
            if (trainingIds.Length == 0)
            {
                return Cell(profile, "訓練セリフ", 4, ProductionStatusKind.Missing,
                    "登録済み訓練がありません。先にUnity訓練一覧を読み込んでください。",
                    new[] { Check("訓練一覧", false, "Unity訓練一覧を読み込んでください。") });
            }

            List<TrainingDialogueEntry> entries = (profile.TrainingDialogues?.Items ??
                new System.Collections.ObjectModel.ObservableCollection<TrainingDialogueEntry>()).Where(x => x != null).ToList();
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>();
            int completedSlots = 0;
            foreach (string trainingId in trainingIds)
            {
                foreach (string state in states)
                {
                    List<TrainingDialogueEntry> matches = entries.Where(x =>
                        string.Equals((x.TrainingId ?? string.Empty).Trim(), trainingId, StringComparison.Ordinal) &&
                        string.Equals(TrainingDialogueSyncService.NormalizeVisualState(x.VisualState), state, StringComparison.Ordinal)).ToList();
                    int messageCount = matches.SelectMany(x => x.Messages ?? new System.Collections.ObjectModel.ObservableCollection<TrainingDialogueMessage>())
                        .Count(x => x != null && !string.IsNullOrWhiteSpace(x.Text));
                    bool complete = matches.Count == 1 && messageCount > 0;
                    if (complete) completedSlots++;
                    ProductionStatusCheckItem check = Check($"{trainingId} / {state}", complete,
                        matches.Count == 0 ? "セリフ枠が未登録です。" :
                        matches.Count > 1 ? $"同じ枠が {matches.Count} 件重複しています。" :
                        messageCount == 0 ? "本文入りのセリフ候補がありません。" : $"本文入り候補 {messageCount} 件。",
                        ProductionStatusTargetKind.TrainingDialogue, trainingId, 4);
                    check.TargetSubId = state;
                    checks.Add(check);
                }
            }
            int totalSlots = trainingIds.Length * states.Length;
            ProductionStatusKind kind = completedSlots == totalSlots ? ProductionStatusKind.Complete :
                completedSlots == 0 ? ProductionStatusKind.Missing : ProductionStatusKind.Partial;
            return Cell(profile, "訓練セリフ", 4, kind,
                $"完成枠 {completedSlots}/{totalSlots}。登録済み訓練 {trainingIds.Length} 件 × 5状態を確認します。", checks);
        }

        private static ProductionStatusCell EvaluateEvents(
            HeroineProfile profile,
            IReadOnlyList<ExpressionDefinition> expressions,
            IReadOnlyList<CostumeDefinition> costumes)
        {
            List<ConversationEntry> events = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(x => x != null && (x.Kind == ConversationDataKind.GameEvents ||
                    x.Kind == ConversationDataKind.ScheduledEvents || x.Kind == ConversationDataKind.Endings)).ToList();
            HashSet<string> expressionIds = new HashSet<string>(expressions.Where(x => !string.IsNullOrWhiteSpace(x.ExpressionId))
                .Select(x => x.ExpressionId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> costumeIds = new HashSet<string>(costumes.Where(x => !string.IsNullOrWhiteSpace(x.CostumeId))
                .Select(x => x.CostumeId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> acceptedAssetIds = new HashSet<string>((profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(x => x != null && x.Status == AssetStatus.Accepted && !string.IsNullOrWhiteSpace(x.AssetId))
                .Select(x => x.AssetId.Trim()), StringComparer.OrdinalIgnoreCase);
            HashSet<string> skillIds = new HashSet<string>((profile.BattleSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineBattleSkill>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.SkillId)).Select(x => x.SkillId.Trim()), StringComparer.OrdinalIgnoreCase);
            foreach (HeroineTrainingSkill skill in profile.HeroineSkillTree?.TrainingSkills ?? new System.Collections.ObjectModel.ObservableCollection<HeroineTrainingSkill>())
                if (skill != null && !string.IsNullOrWhiteSpace(skill.SkillId)) skillIds.Add(skill.SkillId.Trim());

            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>();
            AddEventKindCheck(checks, events, ConversationDataKind.GameEvents, "ゲームイベント");
            AddEventKindCheck(checks, events, ConversationDataKind.ScheduledEvents, "予定イベント");
            AddEventKindCheck(checks, events, ConversationDataKind.Endings, "エンディング");
            bool idsValid = events.All(x => !string.IsNullOrWhiteSpace(x.Id)) && events.GroupBy(x => x.Kind).All(group =>
                group.GroupBy(x => (x.Id ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase).All(ids => ids.Count() == 1));
            checks.Add(Check("イベントID", idsValid, idsValid ? "空ID・同種別内の重複IDはありません。" : "空IDまたは同じ種別内の重複IDがあります。"));
            foreach (ConversationEntry entry in events)
            {
                string label = string.IsNullOrWhiteSpace(entry.Id) ? entry.Kind + "/ID未設定" : entry.Kind + "/" + entry.Id.Trim();
                List<string> problems = new List<string>();
                if (entry.Lines == null || entry.Lines.Count == 0 || entry.Lines.Any(x => x == null || string.IsNullOrWhiteSpace(x.Text))) problems.Add("台詞本文");
                if (entry.Conditions != null && entry.Conditions.MinAffection > entry.Conditions.MaxAffection) problems.Add("好感度範囲");
                if (entry.Conditions != null && entry.Conditions.Once && string.IsNullOrWhiteSpace(entry.Conditions.RequiredFlagIdsText)) problems.Add("Once用フラグ");
                if (entry.Kind == ConversationDataKind.GameEvents &&
                    (entry.AffectionChange < -9999 || entry.AffectionChange > 9999))
                    problems.Add("完了時好感度:" + entry.AffectionChange);
                string triggerType = entry.Conditions?.GameEventTriggerType?.Trim() ?? string.Empty;
                string triggerContextId = entry.Conditions?.TriggerContextId?.Trim() ?? string.Empty;
                if (entry.Kind == ConversationDataKind.GameEvents &&
                    RequiresGameEventTriggerContext(triggerType) &&
                    string.IsNullOrEmpty(triggerContextId))
                    problems.Add("発火対象ID");
                foreach (ConversationLine line in entry.Lines ?? new System.Collections.ObjectModel.ObservableCollection<ConversationLine>())
                    if (line != null && !string.IsNullOrWhiteSpace(line.Expression) && !expressionIds.Contains(line.Expression.Trim())) problems.Add("表情:" + line.Expression.Trim());
                if (entry.Conditions != null && !string.IsNullOrWhiteSpace(entry.Conditions.CostumeId) && !costumeIds.Contains(entry.Conditions.CostumeId.Trim()))
                    problems.Add("衣装:" + entry.Conditions.CostumeId.Trim());
                foreach (string assetId in SplitIds(entry.ImageAssetIdsText)) if (!acceptedAssetIds.Contains(assetId)) problems.Add("画像:" + assetId);
                foreach (string skillId in RequiredSkillIdSyncService.NormalizeText(entry.Conditions?.RequiredSkillIdsText))
                    if (!skillIds.Contains(skillId)) problems.Add("スキル:" + skillId);
                string completionEffect = entry.Kind == ConversationDataKind.GameEvents
                    ? " 発火 " + FormatGameEventTrigger(triggerType, triggerContextId) +
                        " / 完了時好感度 " + FormatSignedValue(entry.AffectionChange) + "。"
                    : string.Empty;
                checks.Add(Check(label, problems.Count == 0, problems.Count == 0
                    ? "本文・条件・参照は有効です。" + completionEffect
                    : "要確認: " + string.Join(", ", problems.Distinct()) + completionEffect,
                    ProductionStatusTargetKind.Conversation, entry.Id, 1, entry.Kind));
            }
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "イベント", 1, Kind(complete, checks.Count),
                $"完成条件 {complete}/{checks.Count}。種別、ID、本文、条件、参照先を確認します。", checks);
        }

        private static ProductionStatusCell EvaluateExportReadiness(
            HeroineProfile profile,
            CharacterProductionStatusRow row,
            Func<HeroineAsset, bool> acceptedAssetFileExists,
            ExportValidationResult exportValidation)
        {
            ProductionStatusCell[] categories = { row.BasicInformation, row.BattleMessages, row.TrainingImages, row.TrainingDialogues, row.CharacterImages, row.Conversations,
                row.Expressions, row.Costumes, row.BattleSkills, row.SkillTree, row.Events, row.ActionReactions };
            List<HeroineAsset> accepted = (profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(x => x != null && x.Status == AssetStatus.Accepted).ToList();
            Func<HeroineAsset, bool> fileCheck = acceptedAssetFileExists ?? (asset => !string.IsNullOrWhiteSpace(asset.StoredPath));
            List<HeroineAsset> missingFiles = accepted.Where(asset => !fileCheck(asset)).ToList();
            List<ProductionStatusCheckItem> checks = categories.Select(category => Check(
                category.CategoryName, category.Kind == ProductionStatusKind.Complete,
                category.Kind == ProductionStatusKind.Complete ? "完成判定です。" : category.Symbol + " " + category.Details,
                ProductionStatusTargetKind.None, null, category.TargetTabIndex)).ToList();
            if (exportValidation != null)
            {
                foreach (ExportValidationIssue issue in exportValidation.Issues)
                {
                    checks.Add(Check(issue.Severity + ": " + issue.Message,
                        issue.Severity == ExportValidationSeverity.Information,
                        issue.Message, issue.TargetKind, issue.TargetId, issue.TargetTabIndex, issue.ConversationKind));
                }
                int validationErrors = exportValidation.ErrorCount;
                int validationWarnings = exportValidation.WarningCount;
                ProductionStatusKind validationKind = validationErrors == 0 && validationWarnings == 0
                    ? ProductionStatusKind.Complete : ProductionStatusKind.Partial;
                return Cell(profile, "Export準備", 12, validationKind,
                    validationErrors == 0 && validationWarnings == 0 ? "共通Export検証でエラー・警告はありません。" :
                    $"共通Export検証: Error {validationErrors} 件、Warning {validationWarnings} 件。詳細から修正対象へ移動できます。", checks);
            }
            checks.Add(Check("Accepted画像の実ファイル", missingFiles.Count == 0,
                missingFiles.Count == 0 ? $"Accepted画像 {accepted.Count} 件の保存先を確認しました。" :
                $"{missingFiles.Count}/{accepted.Count} 件でファイルが見つかりません。"));
            foreach (HeroineAsset missingFile in missingFiles)
            {
                checks.Add(Check("画像ファイル " + missingFile.AssetId, false,
                    $"StoredPathの実ファイルが見つかりません: {missingFile.StoredPath}",
                    ProductionStatusTargetKind.Asset, missingFile.AssetId, 3));
            }
            int entryCount = profile.ConversationEntries?.Count ?? 0;
            checks.Add(Check("Export対象件数", true,
                $"画像 {accepted.Count} 件、会話・イベント {entryCount} 件、戦闘スキル {profile.BattleSkills?.Count ?? 0} 件が対象です。"));
            int errors = checks.Count(x => !x.IsComplete);
            return Cell(profile, "Export準備", 12, errors == 0 ? ProductionStatusKind.Complete : ProductionStatusKind.Partial,
                errors == 0 ? "読み取り専用検査でエラーはありません。Exportを実行できます。" : $"エラー {errors} 件。詳細を修正してからExportしてください。", checks);
        }

        private static void AddEventKindCheck(List<ProductionStatusCheckItem> checks, List<ConversationEntry> entries, ConversationDataKind kind, string label)
        {
            int count = entries.Count(x => x.Kind == kind);
            checks.Add(Check(label, count > 0, count > 0 ? $"{count} 件登録済みです。" : "1件以上登録してください。"));
        }

        private static IEnumerable<string> SplitIds(string text) => string.IsNullOrWhiteSpace(text)
            ? Enumerable.Empty<string>()
            : text.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0);

        private static string FormatSignedValue(int value)
        {
            return value > 0 ? "+" + value : value.ToString();
        }

        private static bool RequiresGameEventTriggerContext(string triggerType)
        {
            return string.Equals(triggerType, "ScheduledEventCompleted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(triggerType, "ActionCompleted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(triggerType, "LocationEntered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(triggerType, "QuestCompleted", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatGameEventTrigger(
            string triggerType,
            string triggerContextId)
        {
            string resolvedType = string.IsNullOrEmpty(triggerType)
                ? "未指定（従来カテゴリ）"
                : triggerType;
            return string.IsNullOrEmpty(triggerContextId)
                ? resolvedType
                : resolvedType + ":" + triggerContextId;
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
                    : "表示名、効果、対象、MP・威力・期間・確率・回数の値を確認してください。",
                    ProductionStatusTargetKind.BattleSkill, skill.SkillId));
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
            HashSet<string> eventIds = new HashSet<string>((profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(x => x != null && x.Kind == ConversationDataKind.GameEvents && !string.IsNullOrWhiteSpace(x.Id))
                .Select(x => x.Id.Trim()), StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ConversationEntry> eventsById = (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(x => x != null && x.Kind == ConversationDataKind.GameEvents && !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
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
                trainingSkillsValid ? $"訓練スキル {trainingSkills.Count} 件のIDと表示名は有効です。" : "訓練スキルに空ID、重複ID、表示名不足があります。",
                ProductionStatusTargetKind.TrainingSkill, trainingSkills.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.SkillId) || string.IsNullOrWhiteSpace(x.DisplayName))?.SkillId));
            string idPrefix = string.IsNullOrWhiteSpace(profile.HeroineId) ? string.Empty : profile.HeroineId.Trim() + "_";
            bool skillNamespacesValid = !string.IsNullOrEmpty(idPrefix) && trainingSkills.All(x =>
                !string.IsNullOrWhiteSpace(x.SkillId) && x.SkillId.StartsWith(idPrefix, StringComparison.Ordinal));
            bool nodeNamespacesValid = !string.IsNullOrEmpty(idPrefix) && nodes.All(x =>
                !string.IsNullOrWhiteSpace(x.NodeId) && x.NodeId.StartsWith(idPrefix, StringComparison.Ordinal));
            checks.Add(Check("訓練SkillId名前空間", skillNamespacesValid,
                skillNamespacesValid ? idPrefix + " で統一されています。" : "ヒロイン固有訓練SkillIdは " + idPrefix + " で始めてください。",
                ProductionStatusTargetKind.TrainingSkill, trainingSkills.FirstOrDefault(x =>
                    string.IsNullOrWhiteSpace(x.SkillId) || !x.SkillId.StartsWith(idPrefix, StringComparison.Ordinal))?.SkillId));
            checks.Add(Check("NodeId名前空間", nodeNamespacesValid,
                nodeNamespacesValid ? idPrefix + " で統一されています。" : "ヒロイン固有NodeIdは " + idPrefix + " で始めてください。",
                ProductionStatusTargetKind.SkillTreeNode, nodes.FirstOrDefault(x =>
                    string.IsNullOrWhiteSpace(x.NodeId) || !x.NodeId.StartsWith(idPrefix, StringComparison.Ordinal))?.NodeId));
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
                if (!string.IsNullOrWhiteSpace(node.UnlockEventId) && !eventIds.Contains(node.UnlockEventId.Trim()))
                    problems.Add("取得時Event:" + node.UnlockEventId.Trim());
                else if (!string.IsNullOrWhiteSpace(node.UnlockEventId) &&
                    eventsById.TryGetValue(node.UnlockEventId.Trim(), out ConversationEntry unlockEvent) &&
                    (unlockEvent.Conditions == null || !unlockEvent.Conditions.Once))
                    problems.Add("取得時EventのOnce:" + node.UnlockEventId.Trim());
                bool hasReward = !string.IsNullOrWhiteSpace(node.GrantedHeroineSkillId) ||
                    !string.IsNullOrWhiteSpace(node.TrainingSkillId) ||
                    !string.IsNullOrWhiteSpace(node.UnlockEventId) ||
                    (node.UnlockedTrainingIds?.Count ?? 0) > 0;
                if (!hasReward) problems.Add("付与内容なし");
                checks.Add(Check($"ノード {label}", problems.Count == 0,
                    problems.Count == 0 ? "前提と付与先の参照は有効です。" : "要確認: " + string.Join(", ", problems),
                    ProductionStatusTargetKind.SkillTreeNode, node.NodeId));
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
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>
            {
                Check("通常会話", hasNormal, hasNormal ? $"{normal.Count} 件登録済みです。" : "通常会話を1件以上登録してください。"),
                Check("開始時メッセージ", hasInitial, hasInitial ? "設定済みです。" : "InitialDialogueMessageが未設定です。"),
                Check("会話ID", idsValid, idsValid ? "空ID・重複IDはありません。" : "空IDまたは同じ種別内の重複IDがあります。"),
                Check("会話本文", textValid, textValid ? "全データに本文があります。" : "台詞行がない、または本文が空のデータがあります。")
            };
            bool categoriesValid = normal.All(x => ConversationValueCatalog.ConversationGenres.Contains(x.Category, StringComparer.OrdinalIgnoreCase));
            checks.Add(Check("通常会話category", categoriesValid,
                categoriesValid ? "Daily / Food / Adventure / Love のみを使用しています。" : "通常会話に空またはUnityでDailyへフォールバックする未知categoryがあります。",
                ProductionStatusTargetKind.Conversation,
                normal.FirstOrDefault(x => !ConversationValueCatalog.ConversationGenres.Contains(x.Category, StringComparer.OrdinalIgnoreCase))?.Id,
                1, ConversationDataKind.Conversations));
            foreach (string genre in ConversationValueCatalog.ConversationGenres)
            {
                ConversationEntry fallback = normal.FirstOrDefault(x =>
                    string.Equals(x.Category, genre, StringComparison.OrdinalIgnoreCase) && IsConversationFallback(x));
                checks.Add(Check(genre + " フォールバック", fallback != null,
                    fallback == null ? "priority 0・条件なし・once=falseの会話が必要です。" : fallback.Id + " を使用します。",
                    ProductionStatusTargetKind.Conversation, fallback?.Id, 1, ConversationDataKind.Conversations));
            }
            int complete = checks.Count(x => x.IsComplete);
            return Cell(profile, "会話データ", 1, Kind(complete, checks.Count),
                $"完成条件 {complete}/{checks.Count}。通常会話、4ジャンル、フォールバック、ID、本文を確認します。", checks);
        }

        private static bool IsConversationFallback(ConversationEntry entry)
        {
            ConversationCondition condition = entry?.Conditions;
            return entry != null && entry.Priority == 0 && condition != null && !condition.Once &&
                condition.MinAffection <= 0 && condition.MaxAffection >= 9999 &&
                string.IsNullOrWhiteSpace(condition.CostumeId) && string.IsNullOrWhiteSpace(condition.TimeOfDay) &&
                string.IsNullOrWhiteSpace(condition.Season) && string.IsNullOrWhiteSpace(condition.Weather);
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
                    BuildLayerDetails(profile, layer), layer == null ? ProductionStatusTargetKind.Expression : ProductionStatusTargetKind.LayerAsset,
                    layer == null ? expressionId : layer.AssetId));
            }
            HashSet<string> references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ConversationLine line in (profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                .Where(x => x?.Lines != null).SelectMany(x => x.Lines).Where(x => x != null && !string.IsNullOrWhiteSpace(x.Expression)))
                references.Add(line.Expression.Trim());
            foreach (BattleResultEventEntry item in profile.BattleMessages?.ResultEvents ?? new System.Collections.ObjectModel.ObservableCollection<BattleResultEventEntry>())
                if (item != null && !string.IsNullOrWhiteSpace(item.ExpressionId)) references.Add(item.ExpressionId.Trim());
            foreach (string reference in references.OrderBy(x => x))
                checks.Add(Check($"表情参照 {reference}", definitionIds.Contains(reference),
                    definitionIds.Contains(reference) ? "登録済み表情を参照しています。" : "参照先の表情定義がありません。",
                    ProductionStatusTargetKind.Expression, reference));
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
                checks.Add(Check($"衣装レイヤー {costumeId}", HasAcceptedLayer(profile, layer), BuildLayerDetails(profile, layer),
                    layer == null ? ProductionStatusTargetKind.Costume : ProductionStatusTargetKind.LayerAsset,
                    layer == null ? costumeId : layer.AssetId));
            }
            HashSet<string> references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ConversationEntry entry in profile.ConversationEntries ?? new System.Collections.ObjectModel.ObservableCollection<ConversationEntry>())
                if (entry?.Conditions != null && !string.IsNullOrWhiteSpace(entry.Conditions.CostumeId)) references.Add(entry.Conditions.CostumeId.Trim());
            foreach (BattleResultEventEntry item in profile.BattleMessages?.ResultEvents ?? new System.Collections.ObjectModel.ObservableCollection<BattleResultEventEntry>())
                foreach (string id in item?.UnlockedOutfitIds ?? Array.Empty<string>()) if (!string.IsNullOrWhiteSpace(id)) references.Add(id.Trim());
            foreach (string reference in references.OrderBy(x => x))
                checks.Add(Check($"衣装参照 {reference}", definitionIds.Contains(reference),
                    definitionIds.Contains(reference) ? "登録済み衣装を参照しています。" : "参照先の衣装定義がありません。",
                    ProductionStatusTargetKind.Costume, reference));
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
                        acceptedSlot ? $"{assetId} はAcceptedです。" : $"{assetId} は未採用です。",
                        ProductionStatusTargetKind.Asset, assetId, 3));
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

        private static ProductionStatusCheckItem Check(
            string name,
            bool complete,
            string details,
            ProductionStatusTargetKind targetKind = ProductionStatusTargetKind.None,
            string targetId = null,
            int? targetTabIndex = null,
            ConversationDataKind conversationKind = ConversationDataKind.Conversations) =>
            new ProductionStatusCheckItem
            {
                Name = name,
                IsComplete = complete,
                Details = details,
                TargetKind = targetKind,
                TargetId = targetId ?? string.Empty,
                TargetTabIndex = targetTabIndex ?? 0,
                ConversationKind = conversationKind
            };

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
            IReadOnlyList<ProductionStatusCheckItem> checks)
        {
            foreach (ProductionStatusCheckItem check in checks ?? Array.Empty<ProductionStatusCheckItem>())
            {
                check.CharacterId = profile.HeroineId ?? string.Empty;
                if (check.TargetTabIndex == 0) check.TargetTabIndex = tabIndex;
            }
            return new ProductionStatusCell
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
}
