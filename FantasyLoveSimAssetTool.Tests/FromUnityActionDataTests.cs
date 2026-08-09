using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class FromUnityActionDataTests
    {
        [TestMethod]
        public void Deserialize_PreservesActionReactionConditions()
        {
            const string json = @"{
                ""schemaVersion"": 1,
                ""items"": [{
                    ""id"": ""Tea"",
                    ""reactions"": [{
                        ""id"": ""Reaction_Tea_Special_01"",
                        ""priority"": 10,
                        ""conditions"": {
                            ""once"": true,
                            ""minAffection"": 100,
                            ""maxAffection"": 9999,
                            ""requiredFlagIds"": [""Event_01""],
                            ""requiredSkillIds"": [""Consideration""]
                        }
                    }]
                }]
            }";

            FromUnityActionDataFile data = JsonSerializer.Deserialize<FromUnityActionDataFile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            FromUnityActionReaction reaction = data.Items[0].Reactions[0];

            Assert.AreEqual("Reaction_Tea_Special_01", reaction.Id);
            Assert.AreEqual(10, reaction.Priority);
            Assert.IsTrue(reaction.Conditions.Once);
            CollectionAssert.AreEqual(new[] { "Event_01" }, reaction.Conditions.RequiredFlagIds);
            CollectionAssert.AreEqual(new[] { "Consideration" }, reaction.Conditions.RequiredSkillIds);
        }

        [TestMethod]
        public void Deserialize_PreservesMenuPresentationFields()
        {
            const string json = @"{
                ""schemaVersion"": 1,
                ""items"": [{
                    ""id"": ""Schedule"",
                    ""displayName"": ""スケジュール"",
                    ""executionType"": ""OpenSchedulePanel"",
                    ""displayColumn"": ""Right"",
                    ""sortOrder"": 70,
                    ""isEnabled"": true
                }]
            }";

            FromUnityActionDataFile data = JsonSerializer.Deserialize<FromUnityActionDataFile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            FromUnityActionDataItem action = data.Items[0];

            Assert.AreEqual("OpenSchedulePanel", action.ExecutionType);
            Assert.AreEqual("Right", action.DisplayColumn);
            Assert.AreEqual(70, action.SortOrder);
            Assert.AreEqual(true, action.IsEnabled);
        }

        [TestMethod]
        public void UnityImportAndToolExport_RoundTripsMenuPresentationFields()
        {
            const string fromUnityJson = @"{
                ""schemaVersion"": 1,
                ""heroineId"": ""Heroine3"",
                ""items"": [{
                    ""id"": ""Schedule"",
                    ""displayName"": ""予定設定"",
                    ""executionType"": ""OpenSchedulePanel"",
                    ""displayColumn"": ""Right"",
                    ""sortOrder"": 70,
                    ""isEnabled"": true
                }]
            }";
            FromUnityActionDataFile imported = JsonSerializer.Deserialize<FromUnityActionDataFile>(
                fromUnityJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            HeroineProfile profile = new HeroineProfile { HeroineId = "Heroine3" };

            MenuActionDefinitionService.MergeFromUnity(profile, imported.Items);
            using JsonDocument exported = JsonDocument.Parse(
                MenuActionDefinitionService.BuildExportJson(profile));
            JsonElement schedule = exported.RootElement.GetProperty("items").EnumerateArray()
                .Single(x => x.GetProperty("actionId").GetString() == "Schedule");

            Assert.AreEqual("予定設定", schedule.GetProperty("displayName").GetString());
            Assert.AreEqual(3, schedule.GetProperty("displayColumn").GetInt32());
            Assert.AreEqual(70, schedule.GetProperty("sortOrder").GetInt32());
            Assert.AreEqual("OpenSchedulePanel", schedule.GetProperty("executionType").GetString());
            Assert.IsTrue(schedule.GetProperty("isEnabled").GetBoolean());
        }
    }
}
