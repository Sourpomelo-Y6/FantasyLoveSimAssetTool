using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class ConversationPromptServiceTests
    {
        [TestMethod]
        public void BundledDefinitions_LoadSixteenSituationsAndKuderiaPrompt()
        {
            var service = new ConversationPromptService(AppContext.BaseDirectory);

            IReadOnlyList<ConversationSituationPrompt> situations = service.LoadSituations();
            ConversationCharacterPrompt character = service.LoadCharacterPrompt("Heroine3");

            Assert.AreEqual(16, situations.Count);
            CollectionAssert.AreEquivalent(
                new[] { "Daily", "Adventure", "Food", "Love" },
                situations.Select(value => value.Category).Distinct().ToArray());
            Assert.IsTrue(situations.Any(value => value.SituationId == "daily_bad_weather_indoors"));
            Assert.IsTrue(situations.Any(value => value.SituationId == "adventure_safety_first"));
            Assert.IsTrue(situations.Any(value => value.SituationId == "food_learn_preferences"));
            Assert.IsTrue(situations.Any(value => value.SituationId == "love_honest_confession"));
            Assert.IsNotNull(character);
            Assert.AreEqual("クーデリア", character.DisplayName);
            Assert.AreEqual("わたし", character.FirstPerson);
            Assert.AreEqual("あなた", character.SecondPerson);
            Assert.AreEqual(3, character.ReferenceLines.Count);
        }

        [TestMethod]
        public void BuildAdditionalPrompt_PutsSituationBeforeCharacterAndLimitsReferences()
        {
            var situation = new ConversationSituationPrompt
            {
                SituationId = "daily",
                DisplayName = "日常",
                Instruction = "静かな日常会話を作る。",
                RequiredElements = new List<string> { "時間を反映" }
            };
            var character = new ConversationCharacterPrompt
            {
                HeroineId = "Heroine3",
                DisplayName = "クーデリア",
                Summary = "穏やかで誠実。",
                FirstPerson = "わたし",
                SecondPerson = "あなた",
                ReferenceLines = new List<string> { "例1", "例2", "例3", "例4" }
            };

            string prompt = ConversationPromptService.BuildAdditionalPrompt(situation, character);

            Assert.IsTrue(prompt.IndexOf("【状況指示】", StringComparison.Ordinal) <
                prompt.IndexOf("【キャラクター固有指示】", StringComparison.Ordinal));
            StringAssert.Contains(prompt, "静かな日常会話を作る。");
            StringAssert.Contains(prompt, "名前: クーデリア");
            StringAssert.Contains(prompt, "参考: 例1 / 例2 / 例3");
            Assert.IsFalse(prompt.Contains("例4"));
            StringAssert.Contains(prompt, "そのまま再出力せず");
        }

        [TestMethod]
        public void LoadCharacterPrompt_DoesNotAllowPathTraversal()
        {
            var service = new ConversationPromptService(Path.GetTempPath());

            Assert.IsNull(service.LoadCharacterPrompt("../Heroine3"));
            Assert.IsNull(service.LoadCharacterPrompt("..\\Heroine3"));
        }

        [TestMethod]
        public void LoadSituations_WhenJsonIsInvalid_ReturnsEmptyList()
        {
            string root = Path.Combine(Path.GetTempPath(), "conversation-prompt-test-" + Guid.NewGuid());
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "PromptTemplates"));
                File.WriteAllText(Path.Combine(root, "PromptTemplates", "conversation-situations.json"), "{");

                var service = new ConversationPromptService(root);

                Assert.AreEqual(0, service.LoadSituations().Count);
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
