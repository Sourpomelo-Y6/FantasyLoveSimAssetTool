using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class TrainingDialogueGenerationServiceTests
    {
        [TestMethod]
        public void BuildPrompt_IncludesCharacterTrainingAndUserContext()
        {
            var profile = new HeroineProfile
            {
                DisplayName = "クーデリア",
                Personality = "負けず嫌い",
                SpeakingStyle = "上品",
                FirstPerson = "わたくし",
                SecondPerson = "あなた"
            };
            var context = new TrainingDialogueGenerationContext
            {
                TrainingId = "sword_training",
                TrainingDisplayName = "剣術訓練",
                TrainingCategory = "combat",
                VisualState = "HeroineLpConsumed",
                PlayerVisible = true,
                HeroineVisible = true,
                AdditionalInstruction = "悔しさを隠す",
                ExistingMessages = new[] { "まだ、終わりではありませんわ。" }
            };

            string prompt = TrainingDialogueGenerationService.BuildPrompt(profile, context);

            StringAssert.Contains(prompt, "名前: クーデリア");
            StringAssert.Contains(prompt, "性格: 負けず嫌い");
            StringAssert.Contains(prompt, "訓練名: 剣術訓練");
            StringAssert.Contains(prompt, "表示状態: ヒロインがLPを消費して疲労");
            StringAssert.Contains(prompt, "主人公=表示、ヒロイン=表示");
            StringAssert.Contains(prompt, "悔しさを隠す");
            StringAssert.Contains(prompt, "既存候補（重複禁止）: まだ、終わりではありませんわ。");
            StringAssert.Contains(prompt, "Voice ID");
        }

        [TestMethod]
        public void BuildPrompt_WithoutTrainingId_Throws()
        {
            Assert.ThrowsException<System.InvalidOperationException>(() =>
                TrainingDialogueGenerationService.BuildPrompt(
                    new HeroineProfile(),
                    new TrainingDialogueGenerationContext { VisualState = "SelectedBeforeFirstStep" }));
        }

        [DataTestMethod]
        [DataRow("SelectedBeforeFirstStep", "訓練開始前")]
        [DataRow("SelectedAfterFirstStep", "訓練実行後・継続中")]
        [DataRow("PlayerLpConsumed", "主人公がLPを消費して疲労")]
        [DataRow("HeroineLpConsumed", "ヒロインがLPを消費して疲労")]
        [DataRow("SimultaneousLpConsumed", "主人公とヒロインが同時にLPを消費して疲労")]
        public void FormatVisualState_ReturnsMeaning(string state, string expected)
        {
            Assert.AreEqual(expected, TrainingDialogueGenerationService.FormatVisualState(state));
        }

        [TestMethod]
        public async Task GenerateAsync_ReturnsAtMostThreeCandidates()
        {
            var service = new TrainingDialogueGenerationService(new StubLlmClient(
                "{\"candidates\":[{\"text\":\"一\"},{\"text\":\"二\"},{\"text\":\"三\"},{\"text\":\"四\"}]}"));

            TrainingDialogueGenerationResult result = await service.GenerateAsync(
                new HeroineProfile(), new LocalAiSettings(), "共通指示",
                new TrainingDialogueGenerationContext
                {
                    TrainingId = "training",
                    VisualState = "SelectedBeforeFirstStep"
                });

            Assert.AreEqual(3, result.Candidates.Count);
            Assert.AreEqual("test-model", result.ModelId);
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
