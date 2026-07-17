using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public static class HeroineSkillTreeSyncService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string BuildExportJson(HeroineProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            HeroineSkillTreeSettings normalized = Normalize(profile.HeroineSkillTree);
            return JsonSerializer.Serialize(new HeroineSkillsDataFile
            {
                SchemaVersion = 1,
                HeroineId = profile.HeroineId,
                TrainingSkills = normalized.TrainingSkills.ToArray(),
                Nodes = normalized.Nodes.ToArray()
            }, JsonOptions);
        }

        public static HeroineSkillsDataFile Deserialize(string json)
        {
            HeroineSkillsDataFile data = JsonSerializer.Deserialize<HeroineSkillsDataFile>(json, JsonOptions);
            if (data == null) throw new InvalidOperationException("heroine skills JSON を読み込めませんでした。");
            if (data.SchemaVersion != 1) throw new InvalidOperationException($"未対応の schemaVersion です: {data.SchemaVersion}");
            return data;
        }

        public static void ApplyImportedValues(HeroineProfile profile, HeroineSkillsDataFile data)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (!string.IsNullOrWhiteSpace(data.HeroineId) &&
                !string.Equals(profile.HeroineId, data.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("HeroineId が選択中のキャラクターと一致しません。");
            }

            profile.HeroineSkillTree ??= new HeroineSkillTreeSettings();
            // null は旧JSONで省略、空配列は明示的な削除として扱う。
            if (data.TrainingSkills != null)
            {
                profile.HeroineSkillTree.TrainingSkills = NormalizeSkills(data.TrainingSkills);
            }
            if (data.Nodes != null)
            {
                profile.HeroineSkillTree.Nodes = NormalizeNodes(data.Nodes);
            }
        }

        public static HeroineSkillTreeSettings Normalize(HeroineSkillTreeSettings settings)
        {
            settings ??= new HeroineSkillTreeSettings();
            settings.TrainingSkills = NormalizeSkills(settings.TrainingSkills);
            settings.Nodes = NormalizeNodes(settings.Nodes);
            return settings;
        }

        private static ObservableCollection<HeroineTrainingSkill> NormalizeSkills(IEnumerable<HeroineTrainingSkill> source)
        {
            List<HeroineTrainingSkill> result = new List<HeroineTrainingSkill>();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (HeroineTrainingSkill item in source ?? Enumerable.Empty<HeroineTrainingSkill>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.SkillId)) continue;
                string id = item.SkillId.Trim();
                if (!ids.Add(id)) continue;
                item.SkillId = id;
                item.DisplayName = item.DisplayName?.Trim() ?? string.Empty;
                item.Description ??= string.Empty;
                item.ApplicationScope = string.IsNullOrWhiteSpace(item.ApplicationScope) ? "AllTrainings" : item.ApplicationScope.Trim();
                item.ApplicationTargetId = item.ApplicationTargetId?.Trim() ?? string.Empty;
                item.PlayerHpCostReduction = Math.Max(0, item.PlayerHpCostReduction);
                item.HeroineHpCostReduction = Math.Max(0, item.HeroineHpCostReduction);
                result.Add(item);
            }
            return new ObservableCollection<HeroineTrainingSkill>(result);
        }

        private static ObservableCollection<HeroineSkillTreeNode> NormalizeNodes(IEnumerable<HeroineSkillTreeNode> source)
        {
            List<HeroineSkillTreeNode> result = new List<HeroineSkillTreeNode>();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (HeroineSkillTreeNode item in source ?? Enumerable.Empty<HeroineSkillTreeNode>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.NodeId)) continue;
                string id = item.NodeId.Trim();
                if (!ids.Add(id)) continue;
                item.NodeId = id;
                item.DisplayName = item.DisplayName?.Trim() ?? string.Empty;
                item.TrainingSkillId = item.TrainingSkillId?.Trim() ?? string.Empty;
                item.GrantedHeroineSkillId = item.GrantedHeroineSkillId?.Trim() ?? string.Empty;
                item.SkillPointCost = Math.Max(0, item.SkillPointCost);
                item.PrerequisiteNodeIds = NormalizeIds(item.PrerequisiteNodeIds);
                item.UnlockedTrainingIds = NormalizeIds(item.UnlockedTrainingIds);
                item.UnlockConditions ??= new ObservableCollection<HeroineSkillTreeCondition>();
                foreach (HeroineSkillTreeCondition condition in item.UnlockConditions.Where(x => x != null))
                {
                    condition.ConditionType = condition.ConditionType?.Trim() ?? "TrainingCount";
                    condition.Scope = condition.Scope?.Trim() ?? "Total";
                    condition.TargetId = condition.TargetId?.Trim() ?? string.Empty;
                    condition.RequiredValue = Math.Max(0, condition.RequiredValue);
                }
                item.UnlockConditions = new ObservableCollection<HeroineSkillTreeCondition>(item.UnlockConditions.Where(x => x != null));
                result.Add(item);
            }
            return new ObservableCollection<HeroineSkillTreeNode>(result);
        }

        private static ObservableCollection<string> NormalizeIds(IEnumerable<string> source)
        {
            return new ObservableCollection<string>((source ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));
        }
    }
}
