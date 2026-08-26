using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationChoiceResponseGenerationSessionTests
    {
        [TestMethod]
        public void TryAdopt_UpdatesResponseOnOriginalChoice()
        {
            var entry = new ConversationEntry();
            var choice = new ConversationChoice { ChoiceText = "一緒に行こう" };
            var session = new ConversationChoiceResponseGenerationSession(entry, choice);

            Assert.IsTrue(session.TryAdopt(entry, choice, "  ええ、喜んで。  "));
            Assert.AreEqual("ええ、喜んで。", choice.ResponseText);
            Assert.AreEqual("一緒に行こう", choice.ChoiceText);
        }

        [TestMethod]
        public void TryAdopt_RejectsChangedChoiceText()
        {
            var entry = new ConversationEntry();
            var choice = new ConversationChoice { ChoiceText = "一緒に行こう" };
            var session = new ConversationChoiceResponseGenerationSession(entry, choice);
            choice.ChoiceText = "ここで待とう";

            Assert.IsFalse(session.TryAdopt(entry, choice, "ええ、喜んで。"));
            Assert.AreEqual(string.Empty, choice.ResponseText);
        }
    }
}
