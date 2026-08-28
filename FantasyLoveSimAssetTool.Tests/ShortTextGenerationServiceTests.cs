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
        public void BuildPrompt_ForOutfitMessage_IncludesOnlySelectedOutfitId()
        {
            var profile = new HeroineProfile { DisplayName = "リリア" };
            var target = new ShortTextGenerationTarget(
                "OutfitChangedMessage", "衣装：変更完了", "着替え後の台詞", 10, 60,
                requiredContext: "OutfitMessage");

            string prompt = ShortTextGenerationService.BuildPrompt(profile, target, context:
                new ShortTextGenerationContext { OutfitId = "SummerDress", ReactionType = "Ignored" });

            StringAssert.Contains(prompt, "衣装ID: SummerDress");
            Assert.IsFalse(prompt.Contains("反応種類"));
        }

        [TestMethod]
        public void BuildPrompt_ForOutfitMessage_IncludesConstrainedExpressionIds()
        {
            var profile = new HeroineProfile { DisplayName = "リリア" };
            var target = new ShortTextGenerationTarget(
                "OutfitChangedMessage", "衣装：変更完了", "着替え後の台詞", 10, 60,
                requiredContext: "OutfitMessage");

            string prompt = ShortTextGenerationService.BuildPrompt(
                profile, target, context: new ShortTextGenerationContext { OutfitId = "SummerDress" },
                expressionIds: new[] { "Smile", "Shy" });

            StringAssert.Contains(prompt, "Smile,Shy");
            StringAssert.Contains(prompt, "expressionId");
        }

        [TestMethod]
        public void ParseCandidateItems_DiscardsExpressionOutsideAllowedIds()
        {
            IReadOnlyList<ShortTextGeneratedCandidate> candidates = ShortTextGenerationService.ParseCandidateItems(
                "{\"candidates\":[{\"text\":\"似合うね\",\"expressionId\":\"Smile\"}," +
                "{\"text\":\"どうかな\",\"expressionId\":\"Unknown\"}]}",
                new[] { "Smile", "Shy" });

            Assert.AreEqual("Smile", candidates[0].ExpressionId);
            Assert.AreEqual(string.Empty, candidates[1].ExpressionId);
        }

        [TestMethod]
        public void BuildPrompt_ForOutfitReaction_RequiresReactionType()
        {
            var profile = new HeroineProfile { DisplayName = "リリア" };
            var target = new ShortTextGenerationTarget(
                "OutfitReactionMessage", "衣装：反応", "衣装への反応", 10, 60,
                requiredContext: "OutfitReaction");

            InvalidOperationException error = Assert.ThrowsException<InvalidOperationException>(() =>
                ShortTextGenerationService.BuildPrompt(profile, target,
                    context: new ShortTextGenerationContext()));

            StringAssert.Contains(error.Message, "ReactionType");
        }

        [TestMethod]
        public void BuildPrompt_ForSkillTextIncludesOnlyCompactTaskContext()
        {
            var profile = new HeroineProfile { DisplayName = "リリア", Personality = "努力家" };
            var target = new ShortTextGenerationTarget(
                "TrainingSkillDescription", "訓練スキル説明", "訓練効果の説明", 15, 80,
                requiredContext: "TrainingSkill");

            string prompt = ShortTextGenerationService.BuildPrompt(profile, target, context:
                new ShortTextGenerationContext
                {
                    TaskContext = "SkillId=Lilia_Training; Scope=Training; TargetId=PracticeA; AffectionModifier=2"
                });

            StringAssert.Contains(prompt, "対象設定: SkillId=Lilia_Training");
            StringAssert.Contains(prompt, "Scope=Training");
            StringAssert.Contains(prompt, "15～80文字の文章");
            Assert.IsFalse(prompt.Contains("expressionId"));
        }

        [TestMethod]
        public void BuildPrompt_ForSkillTextRequiresSelectedSkillContext()
        {
            var target = new ShortTextGenerationTarget(
                "BattleSkillDisplayName", "戦闘スキル名", "戦闘スキル名", 2, 18,
                requiredContext: "BattleSkill");

            InvalidOperationException error = Assert.ThrowsException<InvalidOperationException>(() =>
                ShortTextGenerationService.BuildPrompt(new HeroineProfile(), target,
                    context: new ShortTextGenerationContext()));

            StringAssert.Contains(error.Message, "生成対象の行");
        }

        [DataTestMethod]
        [DataRow("BattleResultEvent", "戦闘後イベント本文")]
        [DataRow("BattlePanelMessage", "パネル結果文")]
        [DataRow("SoloReturnReaction", "単独帰還反応")]
        public void BuildPrompt_ForBattleMessageIncludesCompactSelectedRowContext(
            string requiredContext, string displayName)
        {
            var profile = new HeroineProfile
            {
                DisplayName = "クーデリア",
                Personality = "誇り高い",
                SpeakingStyle = "丁寧"
            };
            var target = new ShortTextGenerationTarget(
                requiredContext + "Message", displayName, displayName + "を作る", 5, 120,
                requiredContext: requiredContext);

            string prompt = ShortTextGenerationService.BuildPrompt(profile, target, context:
                new ShortTextGenerationContext
                {
                    TaskContext = "結果種別=Victory; BattleContextId=Forest"
                });

            StringAssert.Contains(prompt, "結果種別=Victory");
            StringAssert.Contains(prompt, "BattleContextId=Forest");
            StringAssert.Contains(prompt, "クーデリア");
        }

        [TestMethod]
        public void BuildPrompt_ForConversationLineIncludesCompactContextAndConstrainedExpressions()
        {
            var profile = new HeroineProfile
            {
                DisplayName = "リリア",
                Personality = "努力家",
                SpeakingStyle = "丁寧",
                FirstPerson = "私",
                SecondPerson = "あなた"
            };
            var target = new ShortTextGenerationTarget(
                "ConversationLineText", "選択中の台詞本文", "選択中の会話行に入る自然な台詞", 5, 160,
                requiredContext: "ConversationLine");
            var context = new ShortTextGenerationContext
            {
                ConversationKind = "GameEvents",
                ConversationEntryId = "ForestDate",
                ConversationCategory = "Date",
                ConversationSpeaker = "Heroine",
                PreviousConversationLines = "Player: 森へ行こう / Heroine: はい、楽しみです",
                ConversationConditions = "場所=Forest; 時間=Noon; 衣装=Casual"
            };

            string prompt = ShortTextGenerationService.BuildPrompt(
                profile, target, context: context, expressionIds: new[] { "Smile", "Shy" });

            StringAssert.Contains(prompt, "会話種別: GameEvents");
            StringAssert.Contains(prompt, "会話ID: ForestDate");
            StringAssert.Contains(prompt, "話者: Heroine");
            StringAssert.Contains(prompt, "直前の台詞: Player: 森へ行こう / Heroine: はい、楽しみです");
            StringAssert.Contains(prompt, "主要条件: 場所=Forest; 時間=Noon; 衣装=Casual");
            StringAssert.Contains(prompt, "Smile,Shy");
            StringAssert.Contains(prompt, "expressionId");
        }

        [TestMethod]
        public void BuildPrompt_ForConversationLineRequiresSelectedConversationContext()
        {
            var target = new ShortTextGenerationTarget(
                "ConversationLineText", "選択中の台詞本文", "選択中の会話行に入る自然な台詞", 5, 160,
                requiredContext: "ConversationLine");

            InvalidOperationException error = Assert.ThrowsException<InvalidOperationException>(() =>
                ShortTextGenerationService.BuildPrompt(new HeroineProfile(), target,
                    context: new ShortTextGenerationContext()));

            StringAssert.Contains(error.Message, "会話項目と台詞行");
        }

        [TestMethod]
        public void BuildPrompt_ForConversationLinePlacesAdditionalPromptBeforeCurrentContext()
        {
            var target = new ShortTextGenerationTarget(
                "ConversationLineText", "選択中の台詞本文", "自然な台詞", 5, 160,
                requiredContext: "ConversationLine");
            var context = new ShortTextGenerationContext
            {
                ConversationKind = "Conversations",
                ConversationEntryId = "Daily01",
                ConversationAdditionalPrompt = "【状況指示】\n穏やかな日常会話にする。\n【キャラクター固有指示】\n落ち着いた口調にする。"
            };

            string prompt = ShortTextGenerationService.BuildPrompt(
                new HeroineProfile { DisplayName = "クーデリア" }, target, context: context);

            Assert.IsTrue(prompt.IndexOf("【状況指示】", StringComparison.Ordinal) <
                prompt.IndexOf("会話種別: Conversations", StringComparison.Ordinal));
            StringAssert.Contains(prompt, "会話ID: Daily01");
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
        public void TextGenerationCandidate_WithExpression_EnablesOptionalAdoption()
        {
            var candidate = new TextGenerationCandidate("似合うね", 2, 50, "Smile");

            Assert.IsTrue(candidate.HasExpressionSuggestion);
            Assert.IsTrue(candidate.UseExpressionSuggestion);
            Assert.AreEqual("Smile", candidate.ExpressionId);
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
