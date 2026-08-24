using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        public void ParseCandidates_WhenCandidatesDuplicate_ReturnsUsableUniqueCandidates()
        {
            var candidates = ShortTextGenerationService.ParseCandidates(
                "{\"candidates\":[{\"text\":\"同じ\"},{\"text\":\"同じ\"},{\"text\":\"別\"}]}");

            CollectionAssert.AreEqual(new[] { "同じ", "別" },
                new System.Collections.Generic.List<string>(candidates));
        }

        [TestMethod]
        public void ParseCandidates_WhenJsonIsMissing_ParsesNumberedPlainText()
        {
            var candidates = ShortTextGenerationService.ParseCandidates(
                "1. おはよう！\n2) よく眠れた？\n・今日も頑張ろうね。");

            CollectionAssert.AreEqual(new[] { "おはよう！", "よく眠れた？", "今日も頑張ろうね。" },
                new System.Collections.Generic.List<string>(candidates));
        }

        [TestMethod]
        public void BuildPrompt_ForMissingCandidates_IncludesCountAndExclusions()
        {
            var profile = new HeroineProfile { DisplayName = "リリア" };
            var target = new ShortTextGenerationTarget("MorningGreeting", "朝の挨拶", "朝の挨拶", 15, 50);

            string prompt = ShortTextGenerationService.BuildPrompt(profile, target, 1, new[] { "既存候補" });

            StringAssert.Contains(prompt, "1件作成");
            StringAssert.Contains(prompt, "除外: 既存候補");
        }

        [TestMethod]
        public void TextGenerationCandidate_ReportsLengthWarningWithoutBlockingAdoption()
        {
            var candidate = new TextGenerationCandidate("短い", 15, 50);

            Assert.AreEqual(2, candidate.CharacterCount);
            Assert.IsTrue(candidate.HasWarning);
            StringAssert.Contains(candidate.ValidationMessage, "短め");
        }

        [TestMethod]
        public async Task GenerateAsync_WhenResponseCannotBeParsed_PreservesRawResponse()
        {
            var service = new ShortTextGenerationService(new StubLlmClient("{\"candidates\":"));
            var target = new ShortTextGenerationTarget("MorningGreeting", "朝の挨拶", "朝の挨拶", 15, 50);

            ShortTextGenerationResult result = await service.GenerateAsync(
                new HeroineProfile { DisplayName = "リリア" }, target,
                new LocalAiSettings(), "共通指示");

            Assert.AreEqual("{\"candidates\":", result.RawResponse);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ParseError));
            Assert.AreEqual(0, result.Candidates.Count);
        }

        private sealed class StubLlmClient : ILocalLlmClient
        {
            private readonly string content;

            public StubLlmClient(string content) { this.content = content; }

            public Task<IReadOnlyList<string>> GetModelIdsAsync(string serverUrl, int timeoutSeconds,
                CancellationToken cancellationToken = default) =>
                Task.FromResult<IReadOnlyList<string>>(new[] { "test-model" });

            public Task<LocalLlmTestResult> SendTestAsync(string serverUrl, string modelId, string prompt,
                int timeoutSeconds, CancellationToken cancellationToken = default) =>
                Task.FromResult(new LocalLlmTestResult { ModelId = "test-model", Content = content });

            public Task<LocalLlmTestResult> GenerateAsync(string serverUrl, string modelId, string systemPrompt,
                string userPrompt, double temperature, int maxTokens, int timeoutSeconds,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(new LocalLlmTestResult { ModelId = "test-model", Content = content });
        }
    }
}
