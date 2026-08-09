using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public static class MenuActionDefinitionService
    {
        private static readonly MenuActionDefinition[] StandardActions =
        {
            Create("Talk", "会話", 1, 10, "OpenConversationGenres"),
            Create("StatusDetail", "状態", 2, 12, "OpenStatusDetailPanel"),
            Create("StillGallery", "回想", 2, 13, "OpenStillGalleryPanel"),
            Create("MessageLog", "ログ", 2, 14, "OpenMessageLogPanel"),
            Create("Training", "訓練", 2, 17, "OpenTrainingPanel"),
            Create("Skill", "スキル", 2, 18, "OpenSkillPanel"),
            Create("Rest", "休む", 1, 20, "SimpleAction"),
            Create("Walk", "散歩", 1, 30, "SimpleAction"),
            Create("Tea", "お茶", 1, 40, "SimpleAction"),
            Create("Gift", "プレゼント", 2, 50, "SimpleAction"),
            Create("DressUp", "着せ替え", 3, 60, "OpenOutfitPanel"),
            Create("OutfitReaction", "衣装を見る", 3, 65, "OpenOutfitReactionPanel"),
            Create("Schedule", "スケジュール", 3, 70, "OpenSchedulePanel")
        };

        private static readonly HashSet<string> ExecutionTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "SimpleAction",
            "OpenConversationGenres",
            "OpenOutfitPanel",
            "OpenOutfitReactionPanel",
            "OpenSchedulePanel",
            "OpenStatusDetailPanel",
            "OpenStillGalleryPanel",
            "OpenMessageLogPanel",
            "OpenDebugBattlePanel",
            "OpenTrainingPanel",
            "OpenSkillPanel"
        };

        public static int AddMissingStandardActions(HeroineProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            profile.MenuActions ??= new ObservableCollection<MenuActionDefinition>();
            HashSet<string> existingIds = new HashSet<string>(
                profile.MenuActions.Where(x => x != null).Select(x => x.ActionId ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);
            int added = 0;
            foreach (MenuActionDefinition template in StandardActions)
            {
                if (existingIds.Contains(template.ActionId))
                {
                    continue;
                }

                profile.MenuActions.Add(Clone(template));
                existingIds.Add(template.ActionId);
                added++;
            }

            return added;
        }

        public static int ApplyStandardLayout(HeroineProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            AddMissingStandardActions(profile);
            int updated = 0;
            foreach (MenuActionDefinition template in StandardActions)
            {
                MenuActionDefinition action = profile.MenuActions.FirstOrDefault(x => x != null &&
                    string.Equals(x.ActionId, template.ActionId, StringComparison.OrdinalIgnoreCase));
                if (action == null)
                {
                    continue;
                }

                bool changed = action.DisplayName != template.DisplayName ||
                    action.DisplayColumn != template.DisplayColumn ||
                    action.SortOrder != template.SortOrder ||
                    action.ExecutionType != template.ExecutionType ||
                    !action.IsRequired;
                action.DisplayName = template.DisplayName;
                action.DisplayColumn = template.DisplayColumn;
                action.SortOrder = template.SortOrder;
                action.ExecutionType = template.ExecutionType;
                action.IsRequired = true;
                if (changed)
                {
                    updated++;
                }
            }

            return updated;
        }

        public static void Normalize(HeroineProfile profile)
        {
            profile.MenuActions ??= new ObservableCollection<MenuActionDefinition>();
            foreach (MenuActionDefinition action in profile.MenuActions.Where(x => x != null))
            {
                action.ActionId ??= string.Empty;
                action.DisplayName ??= string.Empty;
                action.ExecutionType = string.IsNullOrWhiteSpace(action.ExecutionType)
                    ? "SimpleAction"
                    : action.ExecutionType.Trim();
            }
        }

        public static IReadOnlyList<string> Validate(HeroineProfile profile)
        {
            ObservableCollection<MenuActionDefinition> actions = profile?.MenuActions
                ?? new ObservableCollection<MenuActionDefinition>();
            List<string> warnings = new List<string>();
            foreach (IGrouping<string, MenuActionDefinition> duplicate in actions
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.ActionId))
                .GroupBy(x => x.ActionId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1))
            {
                warnings.Add($"Menu Action `{duplicate.Key}` が重複しています。");
            }

            foreach (MenuActionDefinition required in StandardActions)
            {
                MenuActionDefinition actual = actions.FirstOrDefault(x => x != null
                    && string.Equals(x.ActionId, required.ActionId, StringComparison.OrdinalIgnoreCase));
                if (actual == null)
                {
                    warnings.Add($"必須の Menu Action `{required.ActionId}` がありません。");
                }
                else if (!string.Equals(actual.ExecutionType, required.ExecutionType, StringComparison.Ordinal))
                {
                    warnings.Add($"Menu Action `{required.ActionId}` の ExecutionType は `{required.ExecutionType}` が必要です。現在値: `{actual.ExecutionType}`");
                }
            }

            foreach (MenuActionDefinition action in actions.Where(x => x != null))
            {
                if (string.IsNullOrWhiteSpace(action.ActionId))
                {
                    warnings.Add("ActionId が空の Menu Action があります。");
                }
                if (string.IsNullOrWhiteSpace(action.DisplayName))
                {
                    warnings.Add($"Menu Action `{action.ActionId}` の表示名が空です。");
                }
                if (action.DisplayColumn < 0 || action.DisplayColumn > 3)
                {
                    warnings.Add($"Menu Action `{action.ActionId}` の表示列は 0～3 で指定してください。現在値: {action.DisplayColumn}");
                }
                if (!ExecutionTypes.Contains(action.ExecutionType ?? string.Empty))
                {
                    warnings.Add($"Menu Action `{action.ActionId}` の ExecutionType `{action.ExecutionType}` は未対応です。");
                }
            }

            foreach (IGrouping<int, MenuActionDefinition> duplicate in actions
                .Where(x => x != null && x.SortOrder > 0)
                .GroupBy(x => x.SortOrder)
                .Where(x => x.Count() > 1))
            {
                warnings.Add($"Menu Action の表示順 `{duplicate.Key}` が重複しています: {string.Join(", ", duplicate.Select(x => x.ActionId))}");
            }

            return warnings;
        }

        public static string BuildExportJson(HeroineProfile profile)
        {
            var export = new
            {
                schemaVersion = 1,
                heroineId = profile?.HeroineId ?? string.Empty,
                items = (profile?.MenuActions ?? new ObservableCollection<MenuActionDefinition>())
                    .Where(x => x != null)
                    .Select(x => new
                    {
                        actionId = x.ActionId,
                        displayName = x.DisplayName,
                        displayColumn = x.DisplayColumn,
                        sortOrder = x.SortOrder,
                        executionType = x.ExecutionType,
                        isEnabled = x.IsEnabled,
                        isRequired = x.IsRequired
                    }).ToList()
            };
            return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        }

        private static MenuActionDefinition Create(string id, string name, int column, int sortOrder, string executionType)
        {
            return new MenuActionDefinition
            {
                ActionId = id,
                DisplayName = name,
                DisplayColumn = column,
                SortOrder = sortOrder,
                ExecutionType = executionType
            };
        }

        private static MenuActionDefinition Clone(MenuActionDefinition source)
        {
            return new MenuActionDefinition
            {
                ActionId = source.ActionId,
                DisplayName = source.DisplayName,
                DisplayColumn = source.DisplayColumn,
                SortOrder = source.SortOrder,
                ExecutionType = source.ExecutionType,
                IsEnabled = source.IsEnabled,
                IsRequired = source.IsRequired
            };
        }
    }
}
