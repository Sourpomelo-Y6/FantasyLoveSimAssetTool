using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationChoiceGenerationServiceTests
    {
        [TestMethod]
        public void BuildPrompt_ContainsContextDirectionAndSafetyLimits()
        {
            string prompt = ConversationChoiceGenerationService.BuildPrompt(
                new ConversationChoiceGenerationContext
                {
                    CharacterPrompt = "丁寧な口調で話す。",
                    ConversationKind = "Conversations",
                    ConversationEntryId = "Conv_Test",
                    ConversationCategory = "日常",
                    PreviousLine = "今日はどこへ行こうか？",
                    Direction = "慎重",
                    AdditionalInstruction = "森、危険を避けたい",
                    ExistingChoices = new List<string> { "街へ行こう" }
                });

            StringAssert.Contains(prompt, "直前の台詞: 今日はどこへ行こうか？");
            StringAssert.Contains(prompt, "方向性: 慎重");
            StringAssert.Contains(prompt, "既存選択肢（重複禁止）: 街へ行こう");
            StringAssert.Contains(prompt, "【ユーザーの追加指定・入力案】");
            StringAssert.Contains(prompt, "森、危険を避けたい");
            StringAssert.Contains(prompt, "意図と意味を保ち、プレイヤーの自然な選択肢文へ添削");
            StringAssert.Contains(prompt, "生成するのはヒロインの台詞ではなく、プレイヤーが選んでヒロインへ伝える発言または行動");
            StringAssert.Contains(prompt, "生成文の話者や口調へ適用しない");
            StringAssert.Contains(prompt, "各候補の主語と発話者はプレイヤー");
            StringAssert.Contains(prompt, "返答、好感度、遷移先、条件、ID、分岐構造は生成・変更しない");
        }

        [TestMethod]
        public void BuildPrompt_RequiresPreviousLine()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
                ConversationChoiceGenerationService.BuildPrompt(new ConversationChoiceGenerationContext()));
        }

        [TestMethod]
        public void BuildSystemInstruction_OverridesCharacterVoiceForPlayerChoices()
        {
            string instruction = ConversationChoiceGenerationService.BuildSystemInstruction(
                "キャラクターの口調を守ってください。");

            StringAssert.Contains(instruction, "プレイヤーが選ぶ発言または行動");
            StringAssert.Contains(instruction, "口調指定は会話相手の理解だけに使い、出力文には適用しない");
            StringAssert.Contains(instruction, "ヒロインの発言、返答、独白は出力せず");
        }
    }
}
