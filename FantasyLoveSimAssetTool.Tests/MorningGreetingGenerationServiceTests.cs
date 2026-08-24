using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class MorningGreetingGenerationServiceTests
    {
        [TestMethod]
        public void BuildPrompt_IncludesCharacterVoiceSettings()
        {
            var profile = new HeroineProfile
            {
                DisplayName = "リリア",
                Personality = "明るく努力家",
                SpeakingStyle = "親しみやすい口調",
                FirstPerson = "私",
                SecondPerson = "あなた"
            };

            string prompt = MorningGreetingGenerationService.BuildPrompt(profile);

            StringAssert.Contains(prompt, "名前: リリア");
            StringAssert.Contains(prompt, "一人称: 私");
            StringAssert.Contains(prompt, "二人称: あなた");
            StringAssert.Contains(prompt, "3件");
            Assert.IsFalse(prompt.Contains("好きなもの"));
            Assert.IsFalse(prompt.Contains("既存の夜の挨拶"));
        }

        [TestMethod]
        public void ParseCandidates_ParsesObjectCandidatesInsideCodeFence()
        {
            var candidates = MorningGreetingGenerationService.ParseCandidates(
                "```json\n{\"candidates\":[{\"text\":\"おはよう！\"},{\"text\":\"よく眠れた？\"},{\"text\":\"今日も頑張ろうね。\"}]}\n```");

            CollectionAssert.AreEqual(
                new[] { "おはよう！", "よく眠れた？", "今日も頑張ろうね。" },
                new System.Collections.Generic.List<string>(candidates));
        }

        [TestMethod]
        public void ParseCandidates_WhenCandidatesDuplicate_ThrowsReadableMessage()
        {
            InvalidOperationException error = Assert.ThrowsException<InvalidOperationException>(() =>
                MorningGreetingGenerationService.ParseCandidates(
                    "{\"candidates\":[{\"text\":\"おはよう\"},{\"text\":\"おはよう\"},{\"text\":\"こんにちは\"}]}"));

            StringAssert.Contains(error.Message, "3件必要");
        }
    }
}
