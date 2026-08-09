using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class MenuActionDefinitionServiceTests
    {
        [TestMethod]
        public void AddMissingStandardActions_AddsCompleteMenuWithoutDuplicates()
        {
            HeroineProfile profile = new HeroineProfile();

            int firstAdded = MenuActionDefinitionService.AddMissingStandardActions(profile);
            int secondAdded = MenuActionDefinitionService.AddMissingStandardActions(profile);

            Assert.AreEqual(13, firstAdded);
            Assert.AreEqual(0, secondAdded);
            Assert.AreEqual(13, profile.MenuActions.Count);
            Assert.AreEqual(
                "OpenConversationGenres",
                profile.MenuActions.Single(x => x.ActionId == "Talk").ExecutionType);
            Assert.AreEqual(
                "OpenOutfitPanel",
                profile.MenuActions.Single(x => x.ActionId == "DressUp").ExecutionType);
            Assert.AreEqual(1, profile.MenuActions.Single(x => x.ActionId == "Talk").DisplayColumn);
            Assert.AreEqual(10, profile.MenuActions.Single(x => x.ActionId == "Talk").SortOrder);
            Assert.AreEqual(3, profile.MenuActions.Single(x => x.ActionId == "Schedule").DisplayColumn);
            Assert.AreEqual(70, profile.MenuActions.Single(x => x.ActionId == "Schedule").SortOrder);
        }

        [TestMethod]
        public void AddMissingStandardActions_PreservesExistingDefinition()
        {
            HeroineProfile profile = new HeroineProfile
            {
                MenuActions = new ObservableCollection<MenuActionDefinition>
                {
                    new MenuActionDefinition
                    {
                        ActionId = "Talk",
                        DisplayName = "Custom Talk",
                        ExecutionType = "SimpleAction"
                    }
                }
            };

            int added = MenuActionDefinitionService.AddMissingStandardActions(profile);

            Assert.AreEqual(12, added);
            Assert.AreEqual("Custom Talk", profile.MenuActions.Single(x => x.ActionId == "Talk").DisplayName);
        }

        [TestMethod]
        public void ApplyStandardLayout_RepairsExistingLayoutAndPreservesCustomActions()
        {
            HeroineProfile profile = new HeroineProfile
            {
                MenuActions = new ObservableCollection<MenuActionDefinition>
                {
                    new MenuActionDefinition { ActionId = "Talk", DisplayName = "Broken", DisplayColumn = 0, SortOrder = 99, ExecutionType = "SimpleAction" },
                    new MenuActionDefinition { ActionId = "Custom", DisplayName = "Custom", DisplayColumn = 3, SortOrder = 500 }
                }
            };

            int updated = MenuActionDefinitionService.ApplyStandardLayout(profile);

            MenuActionDefinition talk = profile.MenuActions.Single(x => x.ActionId == "Talk");
            Assert.IsTrue(updated > 0);
            Assert.AreEqual("会話", talk.DisplayName);
            Assert.AreEqual(1, talk.DisplayColumn);
            Assert.AreEqual(10, talk.SortOrder);
            Assert.AreEqual("OpenConversationGenres", talk.ExecutionType);
            Assert.AreEqual(500, profile.MenuActions.Single(x => x.ActionId == "Custom").SortOrder);
        }

        [TestMethod]
        public void Validate_ReportsMissingAndIncorrectNavigationActions()
        {
            HeroineProfile profile = new HeroineProfile
            {
                MenuActions = new ObservableCollection<MenuActionDefinition>
                {
                    new MenuActionDefinition { ActionId = "Talk", ExecutionType = "SimpleAction" }
                }
            };

            var warnings = MenuActionDefinitionService.Validate(profile);

            Assert.IsTrue(warnings.Any(x => x.Contains("Talk") && x.Contains("OpenConversationGenres")));
            Assert.IsTrue(warnings.Any(x => x.Contains("Schedule") && x.Contains("ありません")));
        }

        [TestMethod]
        public void BuildExportJson_UsesExecutionTypeNames()
        {
            HeroineProfile profile = new HeroineProfile { HeroineId = "Heroine3" };
            MenuActionDefinitionService.AddMissingStandardActions(profile);

            using JsonDocument json = JsonDocument.Parse(MenuActionDefinitionService.BuildExportJson(profile));

            Assert.AreEqual("Heroine3", json.RootElement.GetProperty("heroineId").GetString());
            JsonElement talk = json.RootElement.GetProperty("items").EnumerateArray()
                .Single(x => x.GetProperty("actionId").GetString() == "Talk");
            Assert.AreEqual("OpenConversationGenres", talk.GetProperty("executionType").GetString());
            Assert.AreEqual(10, talk.GetProperty("sortOrder").GetInt32());
        }

        [TestMethod]
        public void MergeFromUnity_UpdatesMenuFieldsWithoutRemovingExistingDefinitions()
        {
            HeroineProfile profile = new HeroineProfile();
            MenuActionDefinitionService.AddMissingStandardActions(profile);
            int originalCount = profile.MenuActions.Count;

            MenuActionImportSummary summary = MenuActionDefinitionService.MergeFromUnity(
                profile,
                new[]
                {
                    new FromUnityActionDataItem
                    {
                        Id = "Talk",
                        DisplayName = "会話（Unity）",
                        ExecutionType = "OpenConversationGenres",
                        DisplayColumn = "Left",
                        SortOrder = 15,
                        IsEnabled = false
                    },
                    new FromUnityActionDataItem
                    {
                        Id = "DebugBattle",
                        DisplayName = "戦闘テスト",
                        ExecutionType = "OpenDebugBattlePanel",
                        DisplayColumn = "2",
                        SortOrder = 90,
                        IsEnabled = true
                    }
                });

            MenuActionDefinition talk = profile.MenuActions.Single(x => x.ActionId == "Talk");
            MenuActionDefinition debug = profile.MenuActions.Single(x => x.ActionId == "DebugBattle");
            Assert.AreEqual(1, summary.UpdatedCount);
            Assert.AreEqual(1, summary.AddedCount);
            Assert.AreEqual(originalCount + 1, profile.MenuActions.Count);
            Assert.AreEqual("会話（Unity）", talk.DisplayName);
            Assert.AreEqual(1, talk.DisplayColumn);
            Assert.AreEqual(15, talk.SortOrder);
            Assert.IsFalse(talk.IsEnabled);
            Assert.AreEqual("OpenDebugBattlePanel", debug.ExecutionType);
            Assert.AreEqual(2, debug.DisplayColumn);
            Assert.IsFalse(debug.IsRequired);
        }

        [TestMethod]
        public void MergeFromUnity_OmittedFieldsPreserveExistingValuesAndInvalidValuesWarn()
        {
            HeroineProfile profile = new HeroineProfile();
            MenuActionDefinitionService.AddMissingStandardActions(profile);
            MenuActionDefinition talk = profile.MenuActions.Single(x => x.ActionId == "Talk");

            MenuActionImportSummary summary = MenuActionDefinitionService.MergeFromUnity(
                profile,
                new[]
                {
                    new FromUnityActionDataItem
                    {
                        Id = "Talk",
                        DisplayColumn = "Outside",
                        ExecutionType = "UnknownExecution"
                    }
                });

            Assert.AreEqual(1, summary.UnchangedCount);
            Assert.AreEqual(2, summary.Warnings.Count);
            Assert.AreEqual(1, talk.DisplayColumn);
            Assert.AreEqual(10, talk.SortOrder);
            Assert.IsTrue(talk.IsEnabled);
            Assert.AreEqual("OpenConversationGenres", talk.ExecutionType);
        }
    }
}
