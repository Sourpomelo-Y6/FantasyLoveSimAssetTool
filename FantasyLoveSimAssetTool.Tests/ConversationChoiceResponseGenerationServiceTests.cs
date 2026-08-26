using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationChoiceResponseGenerationServiceTests
    {
        [TestMethod]
        public void BuildPrompt_IncludesSelectedChoiceAndRestrictsOutput()
        {
            string prompt = ConversationChoiceResponseGenerationService.BuildPrompt(
                new ConversationChoiceResponseGenerationContext
                {
                    CharacterPrompt = "上品で丁寧な口調。",
                    ConversationKind = "Conversations",
                    ConversationEntryId = "Conv_Test",
                    ConversationCategory = "日常",
                    PreviousLine = "今日はどうするの？",
                    ChoiceText = "一緒に街へ行こう",
                    AdditionalInstruction = "少し照れながら、嬉しさを隠す",
                    ExistingResponses = new List<string> { "ええ、参りましょう。" }
                });

            StringAssert.Contains(prompt, "プレイヤーが選んだ選択肢: 一緒に街へ行こう");
            StringAssert.Contains(prompt, "既存返答（重複禁止）: ええ、参りましょう。");
            StringAssert.Contains(prompt, "【ユーザーの追加指定・返答案】");
            StringAssert.Contains(prompt, "少し照れながら、嬉しさを隠す");
            StringAssert.Contains(prompt, "キャラクター設定に沿った自然なヒロインの返答へ添削");
            StringAssert.Contains(prompt, "ヒロインの直接の返答");
            StringAssert.Contains(prompt, "プレイヤー側の台詞、選択肢、好感度、条件、ID、分岐構造は生成・変更しない");
        }

        [TestMethod]
        public void BuildPrompt_RequiresChoiceText()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
                ConversationChoiceResponseGenerationService.BuildPrompt(
                    new ConversationChoiceResponseGenerationContext()));
        }
    }
}
