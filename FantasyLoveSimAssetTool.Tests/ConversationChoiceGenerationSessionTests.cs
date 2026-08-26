using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationChoiceGenerationSessionTests
    {
        [TestMethod]
        public void TryAdopt_UpdatesOnlyOriginalChoice()
        {
            var entry = new ConversationEntry();
            var choice = new ConversationChoice();
            entry.Choices.Add(choice);
            var session = new ConversationChoiceGenerationSession(entry, choice);
            string changedProperty = null;
            choice.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            Assert.IsTrue(session.TryAdopt(entry, choice, "  慎重に進もう  "));
            Assert.AreEqual("慎重に進もう", choice.ChoiceText);
            Assert.AreEqual(nameof(ConversationChoice.ChoiceText), changedProperty);
        }

        [TestMethod]
        public void TryAdopt_AllowsAnotherChoiceInSameEntryAndRejectsDifferentEntry()
        {
            var entry = new ConversationEntry();
            var choice = new ConversationChoice();
            var anotherChoice = new ConversationChoice();
            entry.Choices.Add(choice);
            entry.Choices.Add(anotherChoice);
            var session = new ConversationChoiceGenerationSession(entry, choice);

            Assert.IsFalse(session.TryAdopt(new ConversationEntry(), choice, "別の会話"));
            Assert.IsTrue(session.TryAdopt(entry, anotherChoice, "別の選択肢"));
            Assert.AreEqual(string.Empty, choice.ChoiceText);
            Assert.AreEqual("別の選択肢", anotherChoice.ChoiceText);
        }

        [TestMethod]
        public void TryAdopt_RejectsChoiceOutsideSourceEntry()
        {
            var entry = new ConversationEntry();
            var sourceChoice = new ConversationChoice();
            entry.Choices.Add(sourceChoice);
            var session = new ConversationChoiceGenerationSession(entry, sourceChoice);

            Assert.IsFalse(session.TryAdopt(entry, new ConversationChoice(), "対象外"));
        }
    }
}
