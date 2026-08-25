using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationDraftSessionTests
    {
        [TestMethod]
        public void TryApply_AppendAddsDraftWithoutChangingExistingLine()
        {
            var entry = new ConversationEntry();
            entry.Lines.Add(new ConversationLine { Text = "existing" });
            var session = new ConversationDraftSession(entry, "prompt");

            bool applied = session.TryApply(entry, "prompt", CreateDraft(), false);

            Assert.IsTrue(applied);
            CollectionAssert.AreEqual(new[] { "existing", "draft1", "draft2" },
                entry.Lines.Select(value => value.Text).ToArray());
        }

        [TestMethod]
        public void TryApply_ReplaceChangesLinesButNotOtherEntryData()
        {
            var entry = new ConversationEntry { Id = "Event01", Priority = 80 };
            entry.Conditions.LocationId = "Forest";
            entry.Lines.Add(new ConversationLine { Text = "existing" });
            var session = new ConversationDraftSession(entry, "prompt");

            bool applied = session.TryApply(entry, "prompt", CreateDraft(), true);

            Assert.IsTrue(applied);
            CollectionAssert.AreEqual(new[] { "draft1", "draft2" },
                entry.Lines.Select(value => value.Text).ToArray());
            Assert.AreEqual("Event01", entry.Id);
            Assert.AreEqual(80, entry.Priority);
            Assert.AreEqual("Forest", entry.Conditions.LocationId);
        }

        [TestMethod]
        public void TryApply_WhenEntryOrPromptChanged_DoesNotModifyLines()
        {
            var entry = new ConversationEntry();
            entry.Lines.Add(new ConversationLine { Text = "existing" });
            var other = new ConversationEntry();
            var session = new ConversationDraftSession(entry, "prompt");

            Assert.IsFalse(session.TryApply(other, "prompt", CreateDraft(), false));
            Assert.IsFalse(session.TryApply(entry, "changed", CreateDraft(), false));
            Assert.AreEqual(1, entry.Lines.Count);
            Assert.AreEqual("existing", entry.Lines[0].Text);
        }

        private static IEnumerable<ConversationDraftLine> CreateDraft() => new[]
        {
            new ConversationDraftLine { Speaker = "Player", Text = "draft1" },
            new ConversationDraftLine { Speaker = "Heroine", Text = "draft2", ExpressionId = "Smile" }
        };
    }
}
