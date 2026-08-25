using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationDraftQualityServiceTests
    {
        [TestMethod]
        public void Evaluate_ReportsDuplicateLeakForbiddenExpressionAndCategoryMismatch()
        {
            var existing = new ConversationEntry { Id = "Conv_Daily_01" };
            existing.Lines.Add(new ConversationLine { Text = "今日はいい天気ね。" });
            var target = new ConversationEntry
            {
                Id = "Conv_Food_02",
                Category = "Food",
                Lines = new ObservableCollection<ConversationLine>()
            };
            var lines = new List<ConversationDraftLine>
            {
                new ConversationDraftLine { Speaker = "Heroine", Text = "今日はいい天気ね。", ExpressionId = "Smile" },
                new ConversationDraftLine { Speaker = "Heroine", Text = "Conv_Food_02はプロンプトですわ。" },
                new ConversationDraftLine { Speaker = "Heroine", Text = "マジで最高ですわ。" }
            };

            int count = ConversationDraftQualityService.Evaluate(
                lines, new[] { existing, target }, target,
                new ConversationSituationPrompt { Category = "Daily" },
                new ConversationCharacterPrompt { AvoidExpressions = new List<string> { "マジで" } },
                new[] { "Smile" });

            Assert.IsTrue(count >= 5);
            StringAssert.Contains(lines[0].WarningText, "既存台詞と重複");
            StringAssert.Contains(lines[0].WarningText, "カテゴリ");
            StringAssert.Contains(lines[1].WarningText, "内部ID");
            StringAssert.Contains(lines[1].WarningText, "制作指示");
            StringAssert.Contains(lines[2].WarningText, "禁止表現");
        }

        [TestMethod]
        public void Evaluate_LeavesCleanDraftWithoutWarnings()
        {
            var target = new ConversationEntry { Id = "Conv_Daily_02", Category = "Daily" };
            var lines = new List<ConversationDraftLine>
            {
                new ConversationDraftLine { Speaker = "Heroine", Text = "少し風が涼しくなりましたわね。", ExpressionId = "Smile" },
                new ConversationDraftLine { Speaker = "Player", Text = "もう秋が近いのかもしれないね。" }
            };

            int count = ConversationDraftQualityService.Evaluate(
                lines, new[] { target }, target,
                new ConversationSituationPrompt { Category = "Daily" },
                new ConversationCharacterPrompt(), new[] { "Smile" });

            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, lines[0].WarningText);
            Assert.AreEqual(string.Empty, lines[1].WarningText);
        }
    }
}
