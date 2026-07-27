using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class HeroineProfileRoundTripTests
    {
        private string workspaceRoot;
        private CharacterProjectService projectService;

        [TestInitialize]
        public void Initialize()
        {
            workspaceRoot = Path.Combine(Path.GetTempPath(), "FantasyLoveSimAssetToolTests", Guid.NewGuid().ToString("N"));
            projectService = new CharacterProjectService(workspaceRoot);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(workspaceRoot))
            {
                Directory.Delete(workspaceRoot, true);
            }
        }

        [TestMethod]
        public void SaveAndLoad_PreservesUnityProfileFields()
        {
            HeroineProfile source = CreateCompleteProfile();

            projectService.SaveProfile(source);
            HeroineProfile loaded = projectService.LoadProfile(source.HeroineId);

            AssertCommonDialogue(source, loaded);
            AssertResourcePaths(source, loaded);
            Assert.AreEqual(1, loaded.OutfitMessageOverrides.Count);
            Assert.AreEqual("Formal", loaded.OutfitMessageOverrides[0].OutfitId);
            Assert.AreEqual("まだ選べません", loaded.OutfitMessageOverrides[0].LockedMessage);
            Assert.AreEqual("似合っています", loaded.OutfitMessageOverrides[0].ChangedMessage);
            Assert.AreEqual(1, loaded.OutfitReactionMessageOverrides.Count);
            Assert.AreEqual("Like", loaded.OutfitReactionMessageOverrides[0].ReactionType);
            Assert.AreEqual("素敵ですね", loaded.OutfitReactionMessageOverrides[0].Message);
            Assert.IsTrue(loaded.BattleSkillsSpecified);
            Assert.AreEqual("heroine_heal", loaded.BattleSkills[0].SkillId);
            Assert.AreEqual(12, loaded.ConversationEntries[0].AffectionChange);
            Assert.AreEqual(
                "ScheduledEventCompleted",
                loaded.ConversationEntries[0].Conditions.GameEventTriggerType);
            Assert.AreEqual(
                "Forest",
                loaded.ConversationEntries[0].Conditions.TriggerContextId);
            Assert.AreEqual(
                "Event/CompletionReward01",
                loaded.ConversationEntries[0].Lines[0].VoiceId);
        }

        [TestMethod]
        public void ExportHeroine_WritesAllUnityProfileFields()
        {
            HeroineProfile source = CreateCompleteProfile();
            projectService.SaveProfile(source);

            new ExportService(projectService).ExportHeroine(source);

            string exportPath = Path.Combine(
                workspaceRoot,
                "Export",
                source.HeroineId,
                "Data",
                "heroine_profile_export.json");
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(exportPath));
            JsonElement root = document.RootElement;

            Assert.AreEqual(source.InitialDialogueMessage, root.GetProperty("initialDialogueMessage").GetString());
            Assert.AreEqual(source.GameStartFollowUpMessage, root.GetProperty("gameStartFollowUpMessage").GetString());
            Assert.AreEqual("Formal", root.GetProperty("outfitMessageOverrides")[0].GetProperty("outfitId").GetString());
            Assert.AreEqual("Like", root.GetProperty("outfitReactionMessageOverrides")[0].GetProperty("reactionType").GetString());
            Assert.AreEqual("heroine_heal", root.GetProperty("battleSkills")[0].GetProperty("skillId").GetString());
            Assert.AreEqual(source.ConversationResourcePath, root.GetProperty("conversationResourcePath").GetString());
            Assert.AreEqual(source.EndingResourcePath, root.GetProperty("endingResourcePath").GetString());

            string gameEventPath = Path.Combine(
                workspaceRoot,
                "Export",
                source.HeroineId,
                "Data",
                "game_events_export.json");
            using JsonDocument gameEvents = JsonDocument.Parse(File.ReadAllText(gameEventPath));
            Assert.AreEqual(
                12,
                gameEvents.RootElement.GetProperty("items")[0].GetProperty("affectionChange").GetInt32());
            JsonElement conditions =
                gameEvents.RootElement.GetProperty("items")[0].GetProperty("conditions");
            Assert.AreEqual(
                "ScheduledEventCompleted",
                conditions.GetProperty("triggerType").GetString());
            Assert.AreEqual(
                "Event/CompletionReward01",
                gameEvents.RootElement.GetProperty("items")[0]
                    .GetProperty("lines")[0]
                    .GetProperty("voiceId")
                    .GetString());
            Assert.AreEqual("Forest", conditions.GetProperty("triggerContextId").GetString());
        }

        [TestMethod]
        public void LoadProfile_OldJsonWithoutUnityFieldsUsesSafeValuesAndDefaultPaths()
        {
            const string heroineId = "LegacyHeroine";
            string profileDirectory = Path.Combine(workspaceRoot, "Characters", heroineId);
            Directory.CreateDirectory(profileDirectory);
            File.WriteAllText(
                Path.Combine(profileDirectory, "profile.json"),
                "{\"HeroineId\":\"LegacyHeroine\",\"DisplayName\":\"旧ヒロイン\"}");

            HeroineProfile loaded = projectService.LoadProfile(heroineId);

            Assert.AreEqual(string.Empty, loaded.InitialDialogueMessage);
            Assert.AreEqual(string.Empty, loaded.GameStartFollowUpMessage);
            Assert.AreEqual(0, loaded.OutfitMessageOverrides.Count);
            Assert.AreEqual(0, loaded.OutfitReactionMessageOverrides.Count);
            Assert.AreEqual(0, loaded.BattleSkills.Count);
            Assert.IsFalse(loaded.BattleSkillsSpecified);
            Assert.AreEqual("Heroines/LegacyHeroine/Conversations", loaded.ConversationResourcePath);
            Assert.AreEqual("Heroines/LegacyHeroine/Endings", loaded.EndingResourcePath);
        }

        private static HeroineProfile CreateCompleteProfile()
        {
            return new HeroineProfile
            {
                HeroineId = "TestHeroine",
                DisplayName = "テストヒロイン",
                InitialDialogueMessage = "何を話しましょうか？",
                NextActionPrompt = "次はどうしますか？",
                MorningGreeting = "おはようございます",
                GoodNightGreeting = "おやすみなさい",
                GameStartFallbackMessage = "物語を始めます",
                GameStartFollowUpMessage = "よろしくお願いします",
                OutfitMessageOverrides = new ObservableCollection<OutfitMessageOverride>
                {
                    new OutfitMessageOverride
                    {
                        OutfitId = "Formal",
                        LockedMessage = "まだ選べません",
                        ChangedMessage = "似合っています"
                    }
                },
                OutfitReactionMessageOverrides = new ObservableCollection<OutfitReactionMessageOverride>
                {
                    new OutfitReactionMessageOverride { ReactionType = "Like", Message = "素敵ですね" }
                },
                BattleSkillsSpecified = true,
                BattleSkills = new ObservableCollection<HeroineBattleSkill>
                {
                    new HeroineBattleSkill
                    {
                        SkillId = "heroine_heal",
                        DisplayName = "癒やし",
                        EffectType = "Heal",
                        Target = "LowestHpAlly",
                        Cost = 3,
                        Power = 12
                    }
                },
                ConversationEntries = new ObservableCollection<ConversationEntry>
                {
                    new ConversationEntry
                    {
                        Kind = ConversationDataKind.GameEvents,
                        Id = "CompletionRewardEvent",
                        Title = "完了報酬イベント",
                        Category = "Manual",
                        AffectionChange = 12,
                        Conditions = new ConversationCondition
                        {
                            GameEventTriggerType = "ScheduledEventCompleted",
                            TriggerContextId = "Forest"
                        },
                        Lines = new ObservableCollection<ConversationLine>
                        {
                            new ConversationLine
                            {
                                Speaker = "Heroine",
                                Text = "完了です。",
                                VoiceId = "Event/CompletionReward01"
                            }
                        }
                    }
                },
                ConversationResourcePath = "Heroines/TestHeroine/Conversations",
                GameEventResourcePath = "Heroines/TestHeroine/GameEvents",
                ActionResourcePath = "Heroines/TestHeroine/Actions",
                ScheduledEventResourcePath = "Heroines/TestHeroine/ScheduledEvents",
                BattleResultEventResourcePath = "Heroines/TestHeroine/BattleResultEvents",
                BattlePanelResultMessageResourcePath = "Heroines/TestHeroine/BattlePanelResultMessages",
                EndingResourcePath = "Heroines/TestHeroine/Endings"
            };
        }

        private static void AssertCommonDialogue(HeroineProfile expected, HeroineProfile actual)
        {
            Assert.AreEqual(expected.InitialDialogueMessage, actual.InitialDialogueMessage);
            Assert.AreEqual(expected.NextActionPrompt, actual.NextActionPrompt);
            Assert.AreEqual(expected.MorningGreeting, actual.MorningGreeting);
            Assert.AreEqual(expected.GoodNightGreeting, actual.GoodNightGreeting);
            Assert.AreEqual(expected.GameStartFallbackMessage, actual.GameStartFallbackMessage);
            Assert.AreEqual(expected.GameStartFollowUpMessage, actual.GameStartFollowUpMessage);
        }

        private static void AssertResourcePaths(HeroineProfile expected, HeroineProfile actual)
        {
            Assert.AreEqual(expected.ConversationResourcePath, actual.ConversationResourcePath);
            Assert.AreEqual(expected.GameEventResourcePath, actual.GameEventResourcePath);
            Assert.AreEqual(expected.ActionResourcePath, actual.ActionResourcePath);
            Assert.AreEqual(expected.ScheduledEventResourcePath, actual.ScheduledEventResourcePath);
            Assert.AreEqual(expected.BattleResultEventResourcePath, actual.BattleResultEventResourcePath);
            Assert.AreEqual(expected.BattlePanelResultMessageResourcePath, actual.BattlePanelResultMessageResourcePath);
            Assert.AreEqual(expected.EndingResourcePath, actual.EndingResourcePath);
        }
    }
}
