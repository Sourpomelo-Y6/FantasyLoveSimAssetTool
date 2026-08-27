using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class TrainingDialogueGenerationSessionTests
    {
        [TestMethod]
        public void TryAdd_WhenSourceIsCurrent_AddsMessageWithoutVoiceId()
        {
            var entry = new TrainingDialogueEntry();
            var session = new TrainingDialogueGenerationSession(
                entry, null, "training", "SelectedBeforeFirstStep");

            TrainingDialogueMessage added = session.TryAdd(
                entry, "training", "SelectedBeforeFirstStep", "  始めましょう。 ");

            Assert.IsNotNull(added);
            Assert.AreEqual("始めましょう。", added.Text);
            Assert.AreEqual(string.Empty, added.VoiceId);
            Assert.AreSame(added, entry.Messages[0]);
        }

        [TestMethod]
        public void TryReplace_WhenSourceIsCurrent_PreservesVoiceId()
        {
            var message = new TrainingDialogueMessage { Text = "旧", VoiceId = "voice_001" };
            var entry = new TrainingDialogueEntry();
            entry.Messages.Add(message);
            var session = new TrainingDialogueGenerationSession(
                entry, message, "training", "HeroineLpConsumed");

            bool replaced = session.TryReplace(
                entry, message, "training", "HeroineLpConsumed", "新しいセリフ");

            Assert.IsTrue(replaced);
            Assert.AreEqual("新しいセリフ", message.Text);
            Assert.AreEqual("voice_001", message.VoiceId);
        }

        [TestMethod]
        public void TryAdd_WhenVisualStateChanged_DoesNotModifyEntry()
        {
            var entry = new TrainingDialogueEntry();
            var session = new TrainingDialogueGenerationSession(
                entry, null, "training", "SelectedBeforeFirstStep");

            TrainingDialogueMessage added = session.TryAdd(
                entry, "training", "SelectedAfterFirstStep", "候補");

            Assert.IsNull(added);
            Assert.AreEqual(0, entry.Messages.Count);
        }

        [TestMethod]
        public void TryReplace_WhenSelectedMessageChanged_DoesNotModifyEitherMessage()
        {
            var original = new TrainingDialogueMessage { Text = "元" };
            var another = new TrainingDialogueMessage { Text = "別" };
            var entry = new TrainingDialogueEntry();
            entry.Messages.Add(original);
            entry.Messages.Add(another);
            var session = new TrainingDialogueGenerationSession(
                entry, original, "training", "SelectedBeforeFirstStep");

            bool replaced = session.TryReplace(
                entry, another, "training", "SelectedBeforeFirstStep", "候補");

            Assert.IsFalse(replaced);
            Assert.AreEqual("元", original.Text);
            Assert.AreEqual("別", another.Text);
        }
    }
}
