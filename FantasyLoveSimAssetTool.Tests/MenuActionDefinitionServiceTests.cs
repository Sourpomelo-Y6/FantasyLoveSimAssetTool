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
    }
}
