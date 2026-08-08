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
            Create("Talk", "会話", 0, "OpenConversationGenres"),
            Create("Tea", "お茶", 0, "SimpleAction"),
            Create("Rest", "休む", 0, "SimpleAction"),
            Create("Walk", "散歩", 0, "SimpleAction"),
            Create("Gift", "プレゼント", 1, "SimpleAction"),
            Create("DressUp", "着せ替え", 1, "OpenOutfitPanel"),
            Create("OutfitReaction", "衣装を見る", 1, "OpenOutfitReactionPanel"),
            Create("Schedule", "スケジュール", 1, "OpenSchedulePanel"),
            Create("Training", "訓練", 2, "OpenTrainingPanel"),
            Create("Skill", "スキル", 2, "OpenSkillPanel"),
            Create("StatusDetail", "状態", 2, "OpenStatusDetailPanel"),
            Create("StillGallery", "回想", 2, "OpenStillGalleryPanel"),
            Create("MessageLog", "ログ", 3, "OpenMessageLogPanel")
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
                if (!ExecutionTypes.Contains(action.ExecutionType ?? string.Empty))
                {
                    warnings.Add($"Menu Action `{action.ActionId}` の ExecutionType `{action.ExecutionType}` は未対応です。");
                }
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
                        executionType = x.ExecutionType,
                        isEnabled = x.IsEnabled,
                        isRequired = x.IsRequired
                    }).ToList()
            };
            return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        }

        private static MenuActionDefinition Create(string id, string name, int column, string executionType)
        {
            return new MenuActionDefinition
            {
                ActionId = id,
                DisplayName = name,
                DisplayColumn = column,
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
                ExecutionType = source.ExecutionType,
                IsEnabled = source.IsEnabled,
                IsRequired = source.IsRequired
            };
        }
    }
}
