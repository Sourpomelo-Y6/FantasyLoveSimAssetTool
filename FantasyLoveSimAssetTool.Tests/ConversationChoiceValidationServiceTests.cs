using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationChoiceValidationServiceTests
    {
        [TestMethod]
        public void Evaluate_ReportsEmptyFieldsLengthAndAffectionRange()
        {
            var choice = new ConversationChoice
            {
                ChoiceText = new string('選', 41),
                ResponseText = string.Empty,
                AffectionChange = 10000
            };

            IReadOnlyList<string> warnings = ConversationChoiceValidationService.Evaluate(
                choice, new[] { choice });

            Assert.IsTrue(warnings.Any(value => value.Contains("40文字")));
            Assert.IsTrue(warnings.Any(value => value.Contains("返答が空")));
            Assert.IsTrue(warnings.Any(value => value.Contains("-9999～9999")));
        }

        [TestMethod]
        public void Evaluate_ReportsExactDuplicateIgnoringCaseAndWhitespace()
        {
            var first = new ConversationChoice { ChoiceText = "街へ行こう", ResponseText = "ええ。" };
            var second = new ConversationChoice { ChoiceText = "  街へ行こう  ", ResponseText = "そうね。" };
            var choices = new List<ConversationChoice> { first, second };

            Assert.IsTrue(ConversationChoiceValidationService.Evaluate(first, choices)
                .Any(value => value.Contains("重複")));
            Assert.IsTrue(ConversationChoiceValidationService.Evaluate(second, choices)
                .Any(value => value.Contains("重複")));
        }

        [TestMethod]
        public void Evaluate_ValidChoiceHasNoWarnings()
        {
            var choice = new ConversationChoice
            {
                ChoiceText = "一緒に行こう",
                ResponseText = "ええ、喜んで。",
                AffectionChange = 5
            };

            Assert.AreEqual(0, ConversationChoiceValidationService.Evaluate(
                choice, new[] { choice }).Count);
        }
    }
}
