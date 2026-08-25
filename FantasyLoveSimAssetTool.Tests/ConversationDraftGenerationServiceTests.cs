using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationDraftGenerationServiceTests
    {
        [TestMethod]
        public void BuildPrompt_IncludesLayeredPromptAndKeepsStructuralValuesReadOnly()
        {
            var context = new ConversationDraftGenerationContext
            {
                AdditionalPrompt = "【状況指示】\n静かな日常会話にする。\n【キャラクター固有指示】\n穏やかに話す。",
                ConversationKind = "Conversations",
                ConversationEntryId = "Daily01",
                ConversationCategory = "Daily",
                ConditionSummary = "場所=Room; 時間=Night",
                ExpressionIds = new[] { "Smile", "Shy" }
            };

            string prompt = ConversationDraftGenerationService.BuildPrompt(context);

            StringAssert.Contains(prompt, "静かな日常会話にする。");
            StringAssert.Contains(prompt, "会話ID: Daily01");
            StringAssert.Contains(prompt, "1～3行");
            StringAssert.Contains(prompt, "ID、条件、好感度、画像、選択肢は生成・変更しない");
            StringAssert.Contains(prompt, "Smile,Shy");
        }

        [TestMethod]
        public void ParseLines_LimitsToThreeAndRejectsUnknownExpressions()
        {
            string json = "{\"lines\":[" +
                "{\"speaker\":\"Player\",\"text\":\"行1\",\"expressionId\":\"Smile\"}," +
                "{\"speaker\":\"Heroine\",\"text\":\"行2\",\"expressionId\":\"Unknown\"}," +
                "{\"speaker\":\"Heroine\",\"text\":\"行3\",\"expressionId\":\"Shy\"}," +
                "{\"speaker\":\"Heroine\",\"text\":\"行4\",\"expressionId\":\"Smile\"}]}";

            IReadOnlyList<ConversationDraftLine> lines = ConversationDraftGenerationService.ParseLines(
                json, new[] { "Smile", "Shy" });

            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual(string.Empty, lines[0].ExpressionId);
            Assert.AreEqual(string.Empty, lines[1].ExpressionId);
            Assert.AreEqual("Shy", lines[2].ExpressionId);
        }

        [TestMethod]
        public void ParseLines_WhenJsonIsInvalid_ReportsParseFailure()
        {
            InvalidOperationException error = Assert.ThrowsException<InvalidOperationException>(() =>
                ConversationDraftGenerationService.ParseLines("not json", Array.Empty<string>()));

            StringAssert.Contains(error.Message, "会話下書きJSON");
        }
    }
}
