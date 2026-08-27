using FantasyLoveSimAssetTool.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class TrainingAssetWorkItemTests
    {
        [TestMethod]
        public void Parse_TrainingIdContainingUnderscores_SeparatesVisualState()
        {
            (string trainingId, string visualState) = TrainingAssetWorkItem.Parse(
                "Training_sword_basic_SelectedAfterFirstStep");

            Assert.AreEqual("sword_basic", trainingId);
            Assert.AreEqual("SelectedAfterFirstStep", visualState);
        }

        [TestMethod]
        public void DialogueProgressSummary_ReportsMissingDialogueAndVoiceId()
        {
            var entry = new TrainingDialogueEntry();
            using var item = new TrainingAssetWorkItem(
                new HeroineAsset { AssetId = "Training_sword_HeroineLpConsumed" }, "剣術", entry);

            Assert.IsTrue(item.IsDialogueMissing);
            Assert.AreEqual("セリフ未入力", item.DialogueProgressSummary);

            entry.Messages.Add(new TrainingDialogueMessage { Text = "まだ続けられます。" });

            Assert.IsFalse(item.IsDialogueMissing);
            Assert.AreEqual(1, item.DialogueMessageCount);
            Assert.AreEqual(1, item.MissingVoiceIdCount);
            StringAssert.Contains(item.DialogueProgressSummary, "Voice未設定 1件");
        }

        [TestMethod]
        public void DialogueProgressSummary_UpdatesWhenVoiceIdIsEntered()
        {
            var message = new TrainingDialogueMessage { Text = "始めましょう。" };
            var entry = new TrainingDialogueEntry();
            entry.Messages.Add(message);
            using var item = new TrainingAssetWorkItem(
                new HeroineAsset { AssetId = "Training_sword_SelectedBeforeFirstStep" }, "剣術", entry);
            int changed = 0;
            item.ProgressChanged += (_, __) => changed++;

            message.VoiceId = "voice_001";

            Assert.AreEqual(0, item.MissingVoiceIdCount);
            StringAssert.Contains(item.DialogueProgressSummary, "Voice設定済み");
            Assert.AreEqual(1, changed);
        }
    }
}
