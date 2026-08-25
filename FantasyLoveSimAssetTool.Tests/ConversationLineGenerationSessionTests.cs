using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.IO;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationLineGenerationSessionTests
    {
        [TestMethod]
        public void IsCurrent_WhenConversationEntryChanges_ReturnsFalse()
        {
            ConversationEntry source = CreateEntry("source", "first");
            var session = new ConversationLineGenerationSession(source, source.Lines[0]);
            ConversationEntry other = CreateEntry("other", "second");

            Assert.IsFalse(session.IsCurrent(other, other.Lines[0]));
            Assert.AreEqual("first", source.Lines[0].Text);
        }

        [TestMethod]
        public void IsCurrent_WhenSelectedLineChanges_ReturnsFalse()
        {
            ConversationEntry entry = CreateEntry("entry", "first", "second");
            var session = new ConversationLineGenerationSession(entry, entry.Lines[0]);

            Assert.IsFalse(session.IsCurrent(entry, entry.Lines[1]));
        }

        [TestMethod]
        public void CreatingSessionAndContext_DoesNotChangeCurrentText()
        {
            ConversationEntry entry = CreateEntry("entry", string.Empty);

            var session = new ConversationLineGenerationSession(entry, entry.Lines[0]);
            ShortTextGenerationContext context = ConversationLineGenerationSession.CreateContext(entry, entry.Lines[0]);

            Assert.IsTrue(session.IsCurrent(entry, entry.Lines[0]));
            Assert.AreEqual(string.Empty, entry.Lines[0].Text);
            Assert.AreEqual(string.Empty, context.PreviousConversationLines);
            Assert.AreEqual("Conversations", context.ConversationKind);
        }

        [TestMethod]
        public void TryAdopt_ChangesOnlyCapturedLine()
        {
            ConversationEntry entry = CreateEntry("entry", "first", "second");
            var session = new ConversationLineGenerationSession(entry, entry.Lines[1]);

            bool adopted = session.TryAdopt(entry, entry.Lines[1], " generated ");

            Assert.IsTrue(adopted);
            Assert.AreEqual("first", entry.Lines[0].Text);
            Assert.AreEqual("generated", entry.Lines[1].Text);
        }

        [TestMethod]
        public void TryAdopt_WhenSelectionChanged_DoesNotOverwriteEitherLine()
        {
            ConversationEntry entry = CreateEntry("entry", "first", "second");
            var session = new ConversationLineGenerationSession(entry, entry.Lines[0]);

            bool adopted = session.TryAdopt(entry, entry.Lines[1], "generated");

            Assert.IsFalse(adopted);
            Assert.AreEqual("first", entry.Lines[0].Text);
            Assert.AreEqual("second", entry.Lines[1].Text);
        }

        [TestMethod]
        public void CreateContext_IncludesAtMostTwoPreviousNonEmptyLines()
        {
            ConversationEntry entry = CreateEntry("entry", "old", "previous one", "previous two", "current");
            entry.Lines[0].Speaker = "A";
            entry.Lines[1].Speaker = "B";
            entry.Lines[2].Speaker = "C";

            ShortTextGenerationContext context = ConversationLineGenerationSession.CreateContext(entry, entry.Lines[3]);

            Assert.AreEqual("B: previous one / C: previous two", context.PreviousConversationLines);
            Assert.IsFalse(context.PreviousConversationLines.Contains("old"));
        }

        [TestMethod]
        public void TryAdopt_DoesNotWriteConversationFileBeforeSave()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "saved value");
                ConversationEntry entry = CreateEntry("entry", "original");
                var session = new ConversationLineGenerationSession(entry, entry.Lines[0]);

                Assert.IsTrue(session.TryAdopt(entry, entry.Lines[0], "generated"));

                Assert.AreEqual("saved value", File.ReadAllText(path));
                Assert.AreEqual("generated", entry.Lines[0].Text);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static ConversationEntry CreateEntry(string id, params string[] texts)
        {
            var entry = new ConversationEntry
            {
                Id = id,
                Kind = ConversationDataKind.Conversations,
                Category = "Test",
                Lines = new ObservableCollection<ConversationLine>()
            };
            foreach (string text in texts)
                entry.Lines.Add(new ConversationLine { Speaker = "Heroine", Text = text });
            return entry;
        }
    }
}
