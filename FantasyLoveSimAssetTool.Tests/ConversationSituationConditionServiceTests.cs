using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationSituationConditionServiceTests
    {
        [TestMethod]
        public void Apply_ChangesOnlySpecifiedValues()
        {
            var entry = new ConversationEntry { Priority = 100 };
            entry.Conditions.LocationId = "Town";
            entry.Conditions.Weather = "Sunny";
            var situation = new ConversationSituationPrompt
            {
                SuggestedConditions = new ConversationSituationConditionSuggestion
                {
                    TimeOfDay = "Night",
                    MinAffection = 80,
                    Priority = 300,
                    Once = true
                }
            };

            Assert.IsTrue(ConversationSituationConditionService.Apply(entry, situation));

            Assert.AreEqual("Town", entry.Conditions.LocationId);
            Assert.AreEqual("Sunny", entry.Conditions.Weather);
            Assert.AreEqual("Night", entry.Conditions.TimeOfDay);
            Assert.AreEqual(80, entry.Conditions.MinAffection);
            Assert.AreEqual(9999, entry.Conditions.MaxAffection);
            Assert.AreEqual(300, entry.Priority);
            Assert.IsTrue(entry.Conditions.Once);
        }

        [TestMethod]
        public void BuildSummary_ExplainsMissingAndConfiguredSuggestions()
        {
            Assert.IsTrue(ConversationSituationConditionService.BuildSummary(new ConversationSituationPrompt())
                .Contains("推奨条件がありません"));
            var situation = new ConversationSituationPrompt
            {
                SuggestedConditions = new ConversationSituationConditionSuggestion
                {
                    Weather = "Rainy", MinAffection = 30, MaxAffection = 79, Note = "雨の日向け"
                }
            };

            string summary = ConversationSituationConditionService.BuildSummary(situation);

            StringAssert.Contains(summary, "天候 Rainy");
            StringAssert.Contains(summary, "好感度 30～79");
            StringAssert.Contains(summary, "雨の日向け");
        }
    }
}
