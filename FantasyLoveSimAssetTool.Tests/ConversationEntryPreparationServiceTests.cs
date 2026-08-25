using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationEntryPreparationServiceTests
    {
        [TestMethod]
        public void Create_UsesSituationCategoryAndFindsUnusedId()
        {
            var situation = new ConversationSituationPrompt
            {
                SituationId = "daily_relaxed",
                DisplayName = "日常：穏やかな時間",
                Category = "Daily",
                Instruction = "日常会話"
            };
            var existing = new List<ConversationEntry>
            {
                new ConversationEntry { Id = "Conv_Daily_01" },
                new ConversationEntry { Id = "Conv_Daily_03" }
            };

            ConversationEntry entry = ConversationEntryPreparationService.Create(situation, existing);

            Assert.AreEqual(ConversationDataKind.Conversations, entry.Kind);
            Assert.AreEqual("Conv_Daily_02", entry.Id);
            Assert.AreEqual("日常：穏やかな時間", entry.Title);
            Assert.AreEqual("Daily", entry.Category);
            Assert.AreEqual(1, entry.Lines.Count);
            Assert.AreEqual(string.Empty, entry.Lines[0].Text);
            Assert.AreEqual(9999, entry.Conditions.MaxAffection);
        }

        [TestMethod]
        public void Create_DoesNotModifyExistingEntriesOrPersistAnything()
        {
            var existing = new ConversationEntry { Id = "Conv_Love_01", Title = "existing" };
            var situation = new ConversationSituationPrompt
            {
                SituationId = "love",
                DisplayName = "恋愛：距離が近づく",
                Category = "Love",
                Instruction = "恋愛会話"
            };

            ConversationEntry created = ConversationEntryPreparationService.Create(
                situation, new[] { existing });

            Assert.AreEqual("existing", existing.Title);
            Assert.AreNotSame(existing, created);
            Assert.AreEqual("Conv_Love_02", created.Id);
        }
    }
}
