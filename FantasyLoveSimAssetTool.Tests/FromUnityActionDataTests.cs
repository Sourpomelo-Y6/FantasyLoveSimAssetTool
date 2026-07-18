using FantasyLoveSimAssetTool.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
