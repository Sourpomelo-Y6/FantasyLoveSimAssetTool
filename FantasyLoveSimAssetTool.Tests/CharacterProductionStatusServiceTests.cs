using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class CharacterProductionStatusServiceTests
    {
        [TestMethod]
        public void Evaluate_CompleteProfile_ReturnsCompleteForInitialThreeCategories()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Complete, row.BasicInformation.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.BattleMessages.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.TrainingImages.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.TrainingDialogues.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.CharacterImages.Kind);
            Assert.AreEqual(4, row.BasicInformation.Checks.Count);
            Assert.AreEqual(7, row.BattleMessages.Checks.Count);
            Assert.AreEqual(5, row.TrainingImages.Checks.Count);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Conversations.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Expressions.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Costumes.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.BattleSkills.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.SkillTree.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Events.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.ActionReactions.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.MenuActions.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.ExportReadiness.Kind);
            Assert.IsFalse(row.HasIncomplete);
        }

        [TestMethod]
        public void Evaluate_PartialProfile_ExplainsMissingFieldsAndSlots()
        {
            HeroineProfile profile = CompleteProfile();
            profile.Personality = string.Empty;
            profile.BattleMessages.ResultEvents.RemoveAt(0);
            profile.Assets[0].Status = AssetStatus.Pending;

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.BasicInformation.Kind);
            StringAssert.Contains(row.BasicInformation.Details, "性格");
            Assert.AreEqual(ProductionStatusKind.Partial, row.BattleMessages.Kind);
            StringAssert.Contains(row.BattleMessages.Details, "SoloVictory");
            Assert.AreEqual(ProductionStatusKind.Partial, row.TrainingImages.Kind);
            StringAssert.Contains(row.TrainingImages.Details, "4/5");
            Assert.IsTrue(row.TrainingImages.Checks.Any(x => !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_EmptyProfile_ReturnsMissingWithoutThrowing()
        {
            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(new HeroineProfile());

            Assert.AreEqual("×", row.BasicInformation.Symbol);
            Assert.AreEqual(ProductionStatusKind.Missing, row.BattleMessages.Kind);
            Assert.AreEqual(ProductionStatusKind.Missing, row.TrainingImages.Kind);
            Assert.AreEqual(ProductionStatusKind.Missing, row.MenuActions.Kind);
        }

        [TestMethod]
        public void EvaluateMenuActions_InvalidLayoutIsNavigable()
        {
            HeroineProfile profile = CompleteProfile();
            MenuActionDefinition talk = profile.MenuActions.First(x => x.ActionId == "Talk");
            talk.DisplayColumn = 9;
            talk.SortOrder = profile.MenuActions.First(x => x.ActionId == "Rest").SortOrder;

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Missing, row.MenuActions.Kind);
            Assert.AreEqual(18, row.MenuActions.TargetTabIndex);
            Assert.IsTrue(row.MenuActions.Checks.Any(x => x.Details.Contains("表示列")));
            Assert.IsTrue(row.MenuActions.Checks.Any(x => x.Details.Contains("表示順")));
        }

        [TestMethod]
        public void EvaluateVoice_WithoutUnityProject_IsNotApplicable()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(
                profile,
                audioItems: null,
                isAudioProjectConfigured: false);

            Assert.AreEqual(ProductionStatusKind.NotApplicable, row.Voice.Kind);
            Assert.AreEqual("―", row.Voice.Symbol);
            StringAssert.Contains(row.Voice.Details, "未選択");
        }

        [TestMethod]
        public void EvaluateVoice_DetectsMissingReferencedAndUnusedFiles()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationLine line = profile.ConversationEntries
                .SelectMany(entry => entry.Lines)
                .First();
            line.VoiceId = "Event/Required01";
            AudioLibraryItem missing = new AudioLibraryItem
            {
                Category = "VOICE",
                HeroineId = profile.HeroineId,
                LogicalId = profile.HeroineId + "/Event/Required01",
                ExpectedPath = "Required01.*",
                IsAvailable = false,
                ReferenceCount = 1
            };
            AudioLibraryItem unused = new AudioLibraryItem
            {
                Category = "VOICE",
                HeroineId = profile.HeroineId,
                LogicalId = profile.HeroineId + "/Event/Unused01",
                FilePath = "Unused01.ogg",
                IsAvailable = true,
                ReferenceCount = 0
            };

            CharacterProductionStatusRow missingRow =
                EvaluateWithDefinitions(
                    profile,
                    audioItems: new[] { missing, unused },
                    isAudioProjectConfigured: true);

            Assert.AreEqual(ProductionStatusKind.Missing, missingRow.Voice.Kind);
            Assert.IsTrue(missingRow.Voice.Checks.Any(check =>
                !check.IsComplete && check.TargetKind == ProductionStatusTargetKind.Audio));
            Assert.IsTrue(missingRow.Voice.Checks.Any(check =>
                check.IsWarning && check.Symbol == "△"));

            missing.IsAvailable = true;
            missing.FilePath = "Required01.ogg";
            CharacterProductionStatusRow warningRow =
                EvaluateWithDefinitions(
                    profile,
                    audioItems: new[] { missing, unused },
                    isAudioProjectConfigured: true);

            Assert.AreEqual(ProductionStatusKind.Partial, warningRow.Voice.Kind);
            Assert.IsFalse(warningRow.HasIncomplete);
        }

        [TestMethod]
        public void Evaluate_UnknownExpressionAndCostumeReferences_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            profile.ConversationEntries[0].Lines[0].Expression = "UnknownExpression";
            profile.ConversationEntries[0].Conditions.CostumeId = "UnknownCostume";

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.Expressions.Kind);
            Assert.IsTrue(row.Expressions.Checks.Any(x => x.Name.Contains("UnknownExpression") && !x.IsComplete));
            Assert.AreEqual(ProductionStatusKind.Partial, row.Costumes.Kind);
            Assert.IsTrue(row.Costumes.Checks.Any(x => x.Name.Contains("UnknownCostume") && !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_OutfitExpressionProblems_AppearInExpressionsWithOutfitNavigation()
        {
            HeroineProfile profile = CompleteProfile();
            profile.OutfitMessageOverrides.Add(new OutfitMessageOverride
            {
                OutfitId = "Formal",
                LockedExpressionId = string.Empty,
                ChangedExpressionId = "Unknown"
            });
            profile.OutfitReactionMessageOverrides.Add(new OutfitReactionMessageOverride
            {
                ReactionType = "Praise",
                ExpressionId = "Neutral"
            });

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.Expressions.Kind);
            ProductionStatusCheckItem missing = row.Expressions.Checks.First(check =>
                check.TargetKind == ProductionStatusTargetKind.OutfitMessage &&
                check.Name.Contains("未解放"));
            Assert.IsFalse(missing.IsComplete);
            Assert.AreEqual("Formal", missing.TargetId);
            Assert.AreEqual(0, missing.TargetTabIndex);
            Assert.IsTrue(row.Expressions.Checks.Any(check =>
                check.TargetKind == ProductionStatusTargetKind.OutfitReactionMessage && check.IsComplete));
        }

        [TestMethod]
        public void Evaluate_BrokenSkillTreeReferences_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            profile.HeroineSkillTree.Nodes[1].PrerequisiteNodeIds.Add("MissingNode");
            profile.HeroineSkillTree.Nodes[1].GrantedHeroineSkillId = "MissingBattleSkill";
            profile.HeroineSkillTree.Nodes[1].UnlockedTrainingIds.Add("MissingTraining");

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.SkillTree.Kind);
            Assert.IsTrue(row.SkillTree.Checks.Any(x => x.Details.Contains("MissingNode") && !x.IsComplete));
            Assert.IsTrue(row.SkillTree.Checks.Any(x => x.Details.Contains("MissingBattleSkill") && !x.IsComplete));
            Assert.IsTrue(row.SkillTree.Checks.Any(x => x.Details.Contains("MissingTraining") && !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_SkillTreeAcquisitionEventRequiresExistingOnceEvent()
        {
            HeroineProfile profile = CompleteProfile();
            HeroineSkillTreeNode node = profile.HeroineSkillTree.Nodes[0];
            node.UnlockEventId = "GameEvents01";

            CharacterProductionStatusRow notOnce = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, notOnce.SkillTree.Kind);
            Assert.IsTrue(notOnce.SkillTree.Checks.Any(x =>
                !x.IsComplete && x.Details.Contains("取得時EventのOnce:GameEvents01")));

            profile.ConversationEntries.First(x => x.Id == "GameEvents01").Conditions.Once = true;
            CharacterProductionStatusRow valid = EvaluateWithDefinitions(profile);

            Assert.IsTrue(valid.SkillTree.Checks.All(x =>
                x.IsComplete || !x.Details.Contains("取得時Event")));
        }

        [TestMethod]
        public void Evaluate_BrokenEventReferences_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationEntry gameEvent = profile.ConversationEntries.First(x => x.Kind == ConversationDataKind.GameEvents);
            gameEvent.Conditions.Once = true;
            gameEvent.Conditions.CostumeId = "MissingCostume";
            gameEvent.ImageAssetIdsText = "MissingImage";
            gameEvent.Conditions.RequiredSkillIdsText = "MissingSkill";

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.Events.Kind);
            Assert.IsTrue(row.Events.Checks.Any(x => x.Details.Contains("Once用フラグ") && !x.IsComplete));
            Assert.IsTrue(row.Events.Checks.Any(x => x.Details.Contains("MissingImage") && !x.IsComplete));
            Assert.AreEqual(ProductionStatusKind.Partial, row.ExportReadiness.Kind);
        }

        [TestMethod]
        public void Evaluate_MissingAcceptedImageFile_ReturnsPartialExportReadiness()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(
                profile,
                new[] { new ExpressionDefinition { ExpressionId = "Neutral" } },
                new[] { new CostumeDefinition { CostumeId = "Default" } },
                new[]
                {
                    new LayerAssetDefinition { AssetId = "Expression_Neutral", LayerKind = "Expression", ExpressionId = "Neutral" },
                    new LayerAssetDefinition { AssetId = "Costume_Default", LayerKind = "Costume", CostumeId = "Default" }
                },
                asset => asset.AssetId != "A1");

            Assert.AreEqual(ProductionStatusKind.Partial, row.ExportReadiness.Kind);
            Assert.IsTrue(row.ExportReadiness.Checks.Any(x => x.Details.Contains("A1") && !x.IsComplete));
        }

        [TestMethod]
        public void Evaluate_EightLayerHeadAndCostumeSatisfyProductionStatus()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(
                profile,
                new[] { new ExpressionDefinition { ExpressionId = "Neutral" } },
                new[] { new CostumeDefinition { CostumeId = "Default" } },
                new[]
                {
                    new LayerAssetDefinition { AssetId = "Expression_Neutral", LayerKind = "HeadExpression", ExpressionId = "Neutral" },
                    new LayerAssetDefinition { AssetId = "Costume_Default", LayerKind = "CostumeBody", CostumeId = "Default" }
                },
                asset => true);

            Assert.AreEqual(ProductionStatusKind.Complete, row.Expressions.Kind);
            Assert.AreEqual(ProductionStatusKind.Complete, row.Costumes.Kind);
        }

        [TestMethod]
        public void Evaluate_DetailChecksContainNavigationTargets()
        {
            HeroineProfile profile = CompleteProfile();

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            ProductionStatusCheckItem gameEvent = row.Events.Checks.First(x => x.Name.Contains("GameEvents01"));
            Assert.AreEqual(ProductionStatusTargetKind.Conversation, gameEvent.TargetKind);
            Assert.AreEqual(ConversationDataKind.GameEvents, gameEvent.ConversationKind);
            Assert.AreEqual("GameEvents01", gameEvent.TargetId);
            Assert.AreEqual(1, gameEvent.TargetTabIndex);
            StringAssert.Contains(gameEvent.Details, "完了時好感度 0");

            ProductionStatusCheckItem battleSkill = row.BattleSkills.Checks.First(x => x.Name.Contains("BattleSkillA"));
            Assert.AreEqual(ProductionStatusTargetKind.BattleSkill, battleSkill.TargetKind);
            Assert.AreEqual("BattleSkillA", battleSkill.TargetId);

            ProductionStatusCheckItem trainingDialogue = row.TrainingDialogues.Checks.First();
            Assert.AreEqual(ProductionStatusTargetKind.TrainingDialogue, trainingDialogue.TargetKind);
            Assert.AreEqual("TrainingA", trainingDialogue.TargetId);
            Assert.AreEqual("SelectedBeforeFirstStep", trainingDialogue.TargetSubId);
            Assert.AreEqual(4, trainingDialogue.TargetTabIndex);
        }

        [TestMethod]
        public void Evaluate_GameEventShowsCompletionAffectionAndRejectsOutOfRangeValue()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationEntry gameEvent =
                profile.ConversationEntries.First(x => x.Kind == ConversationDataKind.GameEvents);
            gameEvent.AffectionChange = 30;

            CharacterProductionStatusRow valid = EvaluateWithDefinitions(profile);
            ProductionStatusCheckItem validCheck =
                valid.Events.Checks.First(x => x.Name.Contains(gameEvent.Id));
            Assert.IsTrue(validCheck.IsComplete);
            StringAssert.Contains(validCheck.Details, "完了時好感度 +30");

            gameEvent.AffectionChange = 10000;
            CharacterProductionStatusRow invalid = EvaluateWithDefinitions(profile);
            ProductionStatusCheckItem invalidCheck =
                invalid.Events.Checks.First(x => x.Name.Contains(gameEvent.Id));
            Assert.IsFalse(invalidCheck.IsComplete);
            StringAssert.Contains(invalidCheck.Details, "完了時好感度:10000");
        }

        [TestMethod]
        public void Evaluate_ContextGameEventRequiresTargetAndShowsTriggerDetails()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationEntry gameEvent =
                profile.ConversationEntries.First(x => x.Kind == ConversationDataKind.GameEvents);
            gameEvent.Conditions.GameEventTriggerType = "ScheduledEventCompleted";

            CharacterProductionStatusRow missingTarget = EvaluateWithDefinitions(profile);
            ProductionStatusCheckItem missingTargetCheck =
                missingTarget.Events.Checks.First(x => x.Name.Contains(gameEvent.Id));
            Assert.IsFalse(missingTargetCheck.IsComplete);
            StringAssert.Contains(missingTargetCheck.Details, "発火対象ID");

            gameEvent.Conditions.TriggerContextId = "Forest";
            CharacterProductionStatusRow valid = EvaluateWithDefinitions(profile);
            ProductionStatusCheckItem validCheck =
                valid.Events.Checks.First(x => x.Name.Contains(gameEvent.Id));
            Assert.IsTrue(validCheck.IsComplete);
            StringAssert.Contains(
                validCheck.Details,
                "ScheduledEventCompleted:Forest");
        }

        [TestMethod]
        public void Evaluate_MissingTrainingDialogueText_ReturnsPartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            profile.TrainingDialogues.Items[0].Messages[0].Text = string.Empty;

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.TrainingDialogues.Kind);
            Assert.IsTrue(row.TrainingDialogues.Checks.Any(x => !x.IsComplete && x.Details.Contains("本文")));
        }

        [TestMethod]
        public void Evaluate_CharacterImages_DistinguishesRequiredAndOptionalDefinitions()
        {
            HeroineProfile profile = CompleteProfile();
            profile.Assets.Add(new HeroineAsset
            {
                AssetId = "Heroine_Normal",
                Usage = AssetUsage.Sprites,
                Status = AssetStatus.Pending
            });
            StillDefinition[] stills =
            {
                new StillDefinition { AssetId = "Heroine_Normal", Usage = AssetUsage.Sprites },
                new StillDefinition { AssetId = "OptionalEnding", Usage = AssetUsage.Ending }
            };

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(
                profile,
                new[] { new ExpressionDefinition { ExpressionId = "Neutral" } },
                new[] { new CostumeDefinition { CostumeId = "Default" } },
                new[]
                {
                    new LayerAssetDefinition { AssetId = "Expression_Neutral", LayerKind = "Expression", ExpressionId = "Neutral" },
                    new LayerAssetDefinition { AssetId = "Costume_Default", LayerKind = "Costume", CostumeId = "Default" }
                },
                asset => !string.IsNullOrWhiteSpace(asset.StoredPath),
                null,
                stills);

            Assert.AreEqual(ProductionStatusKind.Partial, row.CharacterImages.Kind);
            ProductionStatusCheckItem required = row.CharacterImages.Checks.First(x => x.TargetId == "Heroine_Normal");
            ProductionStatusCheckItem optional = row.CharacterImages.Checks.First(x => x.TargetId == "OptionalEnding");
            Assert.AreEqual("×", required.Symbol);
            Assert.AreEqual("―", optional.Symbol);
            Assert.AreEqual(ProductionStatusTargetKind.StillDefinition, required.TargetKind);
            Assert.AreEqual(7, required.TargetTabIndex);
        }

        [TestMethod]
        public void Evaluate_InvalidActionReaction_ReturnsNavigablePartialDetails()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationEntry gift = profile.ConversationEntries.First(x =>
                x.Kind == ConversationDataKind.ActionReactions && x.Conditions.ActionId == "Gift");
            gift.Lines[0].Text = string.Empty;
            gift.ImageAssetIdsText = "MissingActionImage";
            gift.Priority = -1;

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.ActionReactions.Kind);
            ProductionStatusCheckItem check = row.ActionReactions.Checks.First(x => x.Name.Contains(gift.Id));
            Assert.IsFalse(check.IsComplete);
            StringAssert.Contains(check.Details, "台詞本文");
            StringAssert.Contains(check.Details, "MissingActionImage");
            Assert.AreEqual(ProductionStatusTargetKind.Conversation, check.TargetKind);
            Assert.AreEqual(ConversationDataKind.ActionReactions, check.ConversationKind);
            Assert.AreEqual(gift.Id, check.TargetId);
        }

        [TestMethod]
        public void Evaluate_ActionReactionWithoutUnconditionalFallback_ReturnsPartial()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationEntry tea = profile.ConversationEntries.First(x =>
                x.Kind == ConversationDataKind.ActionReactions && x.Conditions.ActionId == "Tea");
            tea.Priority = 10;
            tea.Conditions.Once = true;

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.ActionReactions.Kind);
            ProductionStatusCheckItem fallback = row.ActionReactions.Checks.First(x => x.Name == "Tea フォールバック");
            Assert.IsFalse(fallback.IsComplete);
            StringAssert.Contains(fallback.Details, "priority 0");
        }

        [TestMethod]
        public void Evaluate_UnknownConversationCategoryAndMissingFallback_ReturnsPartial()
        {
            HeroineProfile profile = CompleteProfile();
            ConversationEntry foodFallback = profile.ConversationEntries.First(x =>
                x.Kind == ConversationDataKind.Conversations && x.Category == "Food" && x.Priority == 0);
            foodFallback.Category = "LocationTalk";

            CharacterProductionStatusRow row = EvaluateWithDefinitions(profile);

            Assert.AreEqual(ProductionStatusKind.Partial, row.Conversations.Kind);
            Assert.IsTrue(row.Conversations.Checks.Any(x => x.Name == "通常会話category" && !x.IsComplete));
            Assert.IsTrue(row.Conversations.Checks.Any(x => x.Name == "Food フォールバック" && !x.IsComplete));
        }

        [TestMethod]
        public void EvaluateTrainingConditions_ReportsInvalidPrerequisiteAndUnlockNode()
        {
            HeroineProfile profile = CompleteProfile();
            TrainingCatalogItem item = profile.TrainingCatalog.Items[0];
            item.RequiredCompletedTrainingIds.Add("MissingTraining");
            item.UnlockNodeIds.Add("MissingNode");

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(profile);

            Assert.AreEqual(ProductionStatusKind.Missing, row.TrainingConditions.Kind);
            StringAssert.Contains(row.TrainingConditions.Checks.Single().Details, "MissingTraining");
            StringAssert.Contains(row.TrainingConditions.Checks.Single().Details, "MissingNode");
        }

        private static HeroineProfile CompleteProfile()
        {
            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = "TestHeroine",
                DisplayName = "テストヒロイン",
                Personality = "強気",
                SpeakingStyle = "丁寧",
                InitialDialogueMessage = "こんにちは",
                TrainingCatalog = new TrainingCatalogSettings
                {
                    Items = new ObservableCollection<TrainingCatalogItem>
                    {
                        new TrainingCatalogItem { TrainingId = "TrainingA", DisplayName = "訓練A" }
                    }
                },
                TrainingImages = new TrainingImageSettings
                {
                    Items = new ObservableCollection<TrainingImageEntry>
                    {
                        new TrainingImageEntry
                        {
                            TrainingId = "TrainingA",
                            BeforeFirstStepImageAssetId = "A1",
                            AfterFirstStepImageAssetId = "A2",
                            PlayerLpConsumedImageAssetId = "A3",
                            HeroineLpConsumedImageAssetId = "A4",
                            SimultaneousLpConsumedImageAssetId = "A5"
                        }
                    }
                }
            };
            foreach (string type in new[] { "SoloVictory", "DuoVictory", "SoloDefeat", "DuoDefeat" })
            {
                profile.BattleMessages.ResultEvents.Add(new BattleResultEventEntry
                {
                    EventId = type,
                    ResultType = type,
                    Message = "message"
                });
            }
            foreach (string type in new[] { "Victory", "Defeat" })
            {
                profile.BattleMessages.PanelMessages.Add(new BattlePanelResultMessageEntry
                {
                    MessageId = type,
                    ResultType = type,
                    Message = "message"
                });
            }
            profile.BattleSkills.Add(new HeroineBattleSkill
            {
                SkillId = "BattleSkillA",
                DisplayName = "攻撃",
                EffectType = "Damage",
                Target = "Enemy",
                Cost = 2,
                Power = 10,
                UseChancePercent = 50,
                MaxUsesPerBattle = 2
            });
            profile.HeroineSkillTree.TrainingSkills.Add(new HeroineTrainingSkill
            {
                SkillId = "TestHeroine_TrainingSkillA",
                DisplayName = "訓練補助"
            });
            profile.HeroineSkillTree.Nodes.Add(new HeroineSkillTreeNode
            {
                NodeId = "TestHeroine_Root",
                DisplayName = "ルート",
                GrantedHeroineSkillId = "BattleSkillA"
            });
            profile.HeroineSkillTree.Nodes.Add(new HeroineSkillTreeNode
            {
                NodeId = "TestHeroine_TrainingNode",
                DisplayName = "訓練ノード",
                TrainingSkillId = "TestHeroine_TrainingSkillA",
                PrerequisiteNodeIds = new ObservableCollection<string> { "TestHeroine_Root" },
                UnlockedTrainingIds = new ObservableCollection<string> { "TrainingA" }
            });
            foreach (string id in new[] { "A1", "A2", "A3", "A4", "A5" })
            {
                profile.Assets.Add(new HeroineAsset { AssetId = id, Status = AssetStatus.Accepted, StoredPath = id + ".png" });
            }
            profile.Assets.Add(new HeroineAsset { AssetId = "Expression_Neutral", Status = AssetStatus.Accepted, StoredPath = "Expression_Neutral.png" });
            profile.Assets.Add(new HeroineAsset { AssetId = "Costume_Default", Status = AssetStatus.Accepted, StoredPath = "Costume_Default.png" });
            foreach (string state in new[]
            {
                "SelectedBeforeFirstStep", "SelectedAfterFirstStep", "PlayerLpConsumed",
                "HeroineLpConsumed", "SimultaneousLpConsumed"
            })
            {
                profile.TrainingDialogues.Items.Add(new TrainingDialogueEntry
                {
                    TrainingId = "TrainingA",
                    VisualState = state,
                    Messages = new ObservableCollection<TrainingDialogueMessage>
                    {
                        new TrainingDialogueMessage { Text = state + "のセリフ" }
                    }
                });
            }
            profile.ConversationEntries.Add(new ConversationEntry
            {
                Id = "Conversation01",
                Kind = ConversationDataKind.Conversations,
                Category = "Daily",
                Lines = new ObservableCollection<ConversationLine>
                {
                    new ConversationLine { Text = "こんにちは", Expression = "Neutral" }
                }
            });
            foreach (string genre in ConversationValueCatalog.ConversationGenres)
            {
                profile.ConversationEntries.Add(new ConversationEntry
                {
                    Id = "Conv_" + genre + "_Fallback_01",
                    Kind = ConversationDataKind.Conversations,
                    Category = genre,
                    Priority = 0,
                    Conditions = new ConversationCondition { MinAffection = 0, MaxAffection = 9999 },
                    Lines = new ObservableCollection<ConversationLine>
                    {
                        new ConversationLine { Text = genre + "の基本会話", Expression = "Neutral" }
                    }
                });
            }
            foreach (ConversationDataKind kind in new[]
            {
                ConversationDataKind.GameEvents,
                ConversationDataKind.ScheduledEvents,
                ConversationDataKind.Endings
            })
            {
                profile.ConversationEntries.Add(new ConversationEntry
                {
                    Id = kind + "01",
                    Kind = kind,
                    Lines = new ObservableCollection<ConversationLine>
                    {
                        new ConversationLine { Text = kind + "の本文", Expression = "Neutral" }
                    }
                });
            }
            foreach (string actionId in ConversationValueCatalog.Actions)
            {
                ConversationEntry reaction = new ConversationEntry
                {
                    Id = "Reaction_" + actionId + "_01",
                    Kind = ConversationDataKind.ActionReactions,
                    Priority = 0,
                    Conditions = new ConversationCondition { MinAffection = 0, MaxAffection = 9999 },
                    Lines = new ObservableCollection<ConversationLine>
                    {
                        new ConversationLine { Text = actionId + "への反応", Expression = "Neutral" }
                    }
                };
                reaction.Conditions.ActionId = actionId;
                profile.ConversationEntries.Add(reaction);
            }
            MenuActionDefinitionService.AddMissingStandardActions(profile);
            return profile;
        }

        private static CharacterProductionStatusRow EvaluateWithDefinitions(
            HeroineProfile profile,
            System.Collections.Generic.IEnumerable<AudioLibraryItem> audioItems = null,
            bool isAudioProjectConfigured = false) =>
            CharacterProductionStatusService.Evaluate(
                profile,
                new[] { new ExpressionDefinition { ExpressionId = "Neutral" } },
                new[] { new CostumeDefinition { CostumeId = "Default" } },
                new[]
                {
                    new LayerAssetDefinition { AssetId = "Expression_Neutral", LayerKind = "Expression", ExpressionId = "Neutral" },
                    new LayerAssetDefinition { AssetId = "Costume_Default", LayerKind = "Costume", CostumeId = "Default" }
                },
                audioItems: audioItems,
                isAudioProjectConfigured: isAudioProjectConfigured);
    }
}
