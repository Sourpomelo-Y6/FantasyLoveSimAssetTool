using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ShortTextGenerationServiceTests
    {
        [TestMethod]
        public void BuildPrompt_IncludesTargetAndMinimalVoiceSettings()
        {
            var profile = new HeroineProfile
            {
                DisplayName = "リリア", Personality = "明るく努力家",
                SpeakingStyle = "親しみやすい口調", FirstPerson = "私", SecondPerson = "あなた",
                Likes = "紅茶"
            };
            var target = new ShortTextGenerationTarget("MorningGreeting", "朝の挨拶", "朝の挨拶", 15, 50);

            string prompt = ShortTextGenerationService.BuildPrompt(profile, target);

            StringAssert.Contains(prompt, "朝の挨拶");
            StringAssert.Contains(prompt, "名前: リリア");
            StringAssert.Contains(prompt, "一人称: 私");
            Assert.IsFalse(prompt.Contains("好きなもの"));
        }

        [TestMethod]
        public void BuildPrompt_ForNextAction_IncludesActionPolicy()
        {
            var profile = new HeroineProfile { DisplayName = "リリア", ActionReactionPolicy = "選択を優しく促す" };
            var target = new ShortTextGenerationTarget("NextActionPrompt", "次の行動", "次の行動を促す台詞", 10, 50, true);

            string prompt = ShortTextGenerationService.BuildPrompt(profile, target);

            StringAssert.Contains(prompt, "行動反応方針: 選択を優しく促す");
        }

        [TestMethod]
        public void ParseCandidates_ParsesObjectCandidatesInsideCodeFence()
        {
            var candidates = ShortTextGenerationService.ParseCandidates(
                "```json\n{\"candidates\":[{\"text\":\"おはよう！\"},{\"text\":\"よく眠れた？\"},{\"text\":\"今日も頑張ろうね。\"}]}\n```");

            CollectionAssert.AreEqual(new[] { "おはよう！", "よく眠れた？", "今日も頑張ろうね。" },
                new System.Collections.Generic.List<string>(candidates));
        }

        [TestMethod]
        public void ParseCandidates_WhenCandidatesDuplicate_ThrowsReadableMessage()
        {
            InvalidOperationException error = Assert.ThrowsException<InvalidOperationException>(() =>
                ShortTextGenerationService.ParseCandidates(
                    "{\"candidates\":[{\"text\":\"同じ\"},{\"text\":\"同じ\"},{\"text\":\"別\"}]}"));
            StringAssert.Contains(error.Message, "3件必要");
        }
    }
}
