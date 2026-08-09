using FantasyLoveSimAssetTool.Common;
using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.ViewModels
{
    public class MainWindowModel : ObservableObject
    {
        private const int LayerPreviewTabIndex = 11;

        private readonly CharacterProjectService characterProjectService;
        private readonly EnemyProjectService enemyProjectService;
        private readonly PromptRecordService promptRecordService;
        private readonly PromptTemplateService promptTemplateService;
        private readonly DefinitionCatalogService definitionCatalogService;
        private StillDefinitionService stillDefinitionService;
        private readonly ImageInspectionService imageInspectionService;
        private readonly ComfySettingsService comfySettingsService;
        private readonly ComfyWorkflowService comfyWorkflowService;
        private readonly ComfyClientService comfyClientService;
        private readonly ExportService exportService;
        private readonly EnemyExportService enemyExportService;
        private readonly PlayerProjectService playerProjectService;
        private readonly PlayerExportService playerExportService;
        private readonly AudioLibraryService audioLibraryService;
        private readonly AudioPreviewService audioPreviewService;
        private readonly List<AudioLibraryItem> allAudioLibraryItems =
            new List<AudioLibraryItem>();
        private OutfitMessageOverride selectedOutfitMessageOverride;
        private OutfitReactionMessageOverride selectedOutfitReactionMessageOverride;
        private TrainingImageEntry selectedTrainingImageEntry;
        private HeroineAsset selectedTrainingAsset;
        private TrainingDialogueEntry selectedTrainingDialogueEntry;
        private TrainingDialogueMessage selectedTrainingDialogueMessage;
        private string heroineIdInput;
        private string displayNameInput;
        private string enemyIdInput;
        private string enemyDisplayNameInput;
        private string enemyTypeInput;
        private string enemyAssetIdInput;
        private string enemyImageSourcePathInput;
        private string enemyAssetPromptInput;
        private AssetStatus selectedEnemyAssetStatus;
        private EnemyProfile selectedEnemyProfile;
        private EnemyAsset selectedEnemyAsset;
        private string selectedEnemyAssetImagePath;
        private string selectedEnemyAssetImageMessage;
        private PlayerProfile playerProfile;
        private PlayerAsset selectedPlayerAsset;
        private string playerAssetIdInput;
        private string playerImageSourcePathInput;
        private string playerAssetPromptInput;
        private AssetStatus selectedPlayerAssetStatus;
        private string selectedPlayerAssetImagePath;
        private string selectedPlayerAssetImageMessage;
        private string assetIdInput;
        private string imageSourcePathInput;
        private AssetUsage selectedAssetUsage;
        private AssetStatus selectedAssetStatus;
        private string selectedAssetStatusFilter;
        private HeroineAsset selectedAsset;
        private AssetUsage selectedPromptTemplateUsage;
        private PromptTemplate selectedPromptTemplate;
        private StillDefinition selectedStillDefinition;
        private string selectedStillUsageFilter;
        private ConversationDataKind selectedConversationDataKind;
        private ConversationEntry selectedConversationEntry;
        private ConversationLine selectedConversationLine;
        private ConversationChoice selectedConversationChoice;
        private string conversationSearchText;
        private string selectedConversationCategoryFilter;
        private string selectedConversationImageFilter;
        private bool showOnlyConversationWarnings;
        private bool showOnlyMatchingGameEvents;
        private string gameEventTestLocationId;
        private string gameEventTestAffection;
        private string gameEventTestWeather;
        private string gameEventTestSeason;
        private string gameEventTestTimeOfDay;
        private string gameEventTestActionId;
        private string gameEventTestCostumeId;
        private string gameEventTestItemId;
        private string gameEventTestFlagIdsText;
        private string selectedConversationCategorySuggestion;
        private string selectedConversationLocationSuggestion;
        private string selectedConversationActionSuggestion;
        private string selectedConversationWeatherSuggestion;
        private string selectedConversationSeasonSuggestion;
        private string selectedConversationTimeOfDaySuggestion;
        private string selectedConversationCostumeSuggestion;
        private string selectedConversationExpressionSuggestion;
        private HeroineAsset selectedConversationImageAsset;
        private ExpressionDefinition selectedExpressionDefinition;
        private CostumeDefinition selectedCostumeDefinition;
        private LayerAssetDefinition selectedLayerAssetDefinition;
        private string definitionCatalogValidationMessage;
        private string selectedLayerPreviewBaseBodyId;
        private string selectedLayerPreviewCostumeId;
        private string selectedLayerPreviewExpressionId;
        private LayerPreviewItem selectedLayerPreviewItem;
        private string layerPreviewMessage;
        private string selectedOutfitCostumeBodyAssetId;
        private string selectedOutfitBackAccessoryAssetId;
        private string selectedOutfitFrontAccessoryAssetId;
        private string outfitCompositionMessage;
        private HeadPartWorkspaceItem selectedHeadPartWorkspaceItem;
        private string headPartWorkspaceMessage;
        private string profilePreviewImagePath;
        private string profilePreviewMessage;
        private PromptRecord currentPromptRecord;
        private HeroineProfile selectedProfile;
        private ExportReport lastExportReport;
        private EnemyExportReport lastEnemyExportReport;
        private PlayerExportReport lastPlayerExportReport;
        private string selectedAssetImagePath;
        private string selectedAssetImageMessage;
        private string selectedStillAssetStatusText;
        private string selectedStillImageStatusText;
        private string selectedStillPromptStatusText;
        private string selectedStillImagePath;
        private string selectedStillImageMessage;
        private ComfySettings comfySettings;
        private string comfySettingsSummary;
        private string comfyWorkflowTemplateEditorText;
        private string currentComfyWorkflowPreview;
        private string currentComfyPromptId;
        private string currentComfyResultSummary;
        private string currentComfyPreviewImagePath;
        private string currentComfyPreviewImageMessage;
        private ComfyOutputImage currentComfyOutputImage;
        private PromptRecord currentComfySubmittedPromptRecord;
        private string currentComfyWorkflowJson;
        private string currentComfyProgressSummary;
        private CancellationTokenSource comfyPollingCancellation;
        private bool hasComfyInterruptRequested;
        private bool isComfySubmitting;
        private bool isComfyCheckingResult;
        private bool isComfyFetchingImage;
        private bool isComfyWaitingResult;
        private bool isComfyInterrupting;
        private string statusMessage;
        private string lastBattleMessageImportReport;
        private int selectedMainTabIndex;
        private bool showOnlyIncompleteProductionStatus;
        private string unityProjectPath;
        private string audioLibrarySearchText;
        private string bgmSeAudioSearchText;
        private string voiceAudioSearchText;
        private string selectedAudioCategory;
        private bool showOnlySelectedHeroineAudio;
        private bool showOnlyUnusedVoice;
        private string audioLibrarySummary;
        private string bgmSeAudioSummary;
        private string voiceAudioSummary;
        private AudioLibraryItem selectedAudioLibraryItem;
        private string selectedVoiceUsage;
        private string voiceRegistrationId;
        private string selectedVoiceAssignmentTarget;
        private BattleResultEventEntry selectedBattleResultEvent;
        private BattlePanelResultMessageEntry selectedBattlePanelMessage;
        private HeroineBattleSkill selectedProductionBattleSkill;
        private HeroineTrainingSkill selectedProductionTrainingSkill;
        private HeroineSkillTreeNode selectedProductionSkillTreeNode;
        private MenuActionDefinition selectedMenuAction;
        private bool isBattleSkillEditorExpanded;
        private bool isSkillTreeEditorExpanded;

        public ObservableCollection<HeroineProfile> Profiles { get; }

        public ObservableCollection<AudioLibraryItem> AudioLibraryItems { get; }

        public ObservableCollection<AudioLibraryItem> BgmSeAudioItems { get; }

        public ObservableCollection<AudioLibraryItem> VoiceAudioItems { get; }

        public ObservableCollection<string> AudioCategoryOptions { get; }

        public ObservableCollection<string> AvailableVoiceIds { get; }

        public ObservableCollection<string> VoiceUsageOptions { get; }

        public ObservableCollection<string> VoiceAssignmentTargetOptions { get; }

        public ObservableCollection<KeyValuePair<int, string>> MenuActionColumnOptions { get; }

        public ObservableCollection<string> MenuActionExecutionTypes { get; }

        public ObservableCollection<string> MenuActionWarnings { get; }

        public MenuActionDefinition SelectedMenuAction
        {
            get => selectedMenuAction;
            set
            {
                if (selectedMenuAction == value) return;
                selectedMenuAction = value;
                OnPropertyChanged(nameof(SelectedMenuAction));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public HeroineBattleSkill SelectedProductionBattleSkill
        {
            get => selectedProductionBattleSkill;
            set { if (selectedProductionBattleSkill != value) { selectedProductionBattleSkill = value; OnPropertyChanged(nameof(SelectedProductionBattleSkill)); } }
        }

        public HeroineTrainingSkill SelectedProductionTrainingSkill
        {
            get => selectedProductionTrainingSkill;
            set { if (selectedProductionTrainingSkill != value) { selectedProductionTrainingSkill = value; OnPropertyChanged(nameof(SelectedProductionTrainingSkill)); } }
        }

        public HeroineSkillTreeNode SelectedProductionSkillTreeNode
        {
            get => selectedProductionSkillTreeNode;
            set { if (selectedProductionSkillTreeNode != value) { selectedProductionSkillTreeNode = value; OnPropertyChanged(nameof(SelectedProductionSkillTreeNode)); } }
        }

        public bool IsBattleSkillEditorExpanded
        {
            get => isBattleSkillEditorExpanded;
            set { if (isBattleSkillEditorExpanded != value) { isBattleSkillEditorExpanded = value; OnPropertyChanged(nameof(IsBattleSkillEditorExpanded)); } }
        }

        public bool IsSkillTreeEditorExpanded
        {
            get => isSkillTreeEditorExpanded;
            set { if (isSkillTreeEditorExpanded != value) { isSkillTreeEditorExpanded = value; OnPropertyChanged(nameof(IsSkillTreeEditorExpanded)); } }
        }

        public ObservableCollection<ProductionStatusCell> ProductionStatusCategories { get; }

        public ObservableCollection<EnemyProfile> EnemyProfiles { get; }

        public ObservableCollection<AssetUsage> AssetUsages { get; }

        public ObservableCollection<AssetStatus> AssetStatuses { get; }

        public ObservableCollection<string> AssetStatusFilters { get; }

        public ObservableCollection<HeroineAsset> FilteredAssets { get; }

        public ObservableCollection<HeroineAsset> AcceptedAssets { get; }

        public ObservableCollection<HeroineAsset> TrainingAssets { get; }

        public ObservableCollection<PromptTemplate> AvailablePromptTemplates { get; }

        public ObservableCollection<StillDefinition> StillDefinitions { get; }

        public ObservableCollection<StillDefinition> FilteredStillDefinitions { get; }

        public ObservableCollection<string> StillUsageFilters { get; }

        public ObservableCollection<StillStatus> StillStatuses { get; }

        public ObservableCollection<ConversationDataKind> ConversationDataKinds { get; }

        public ObservableCollection<ConversationEntry> FilteredConversationEntries { get; }

        public ObservableCollection<string> ConversationCategorySuggestions { get; }

        public ObservableCollection<string> ConversationCategoryFilters { get; }

        public ObservableCollection<string> ConversationImageFilters { get; }

        public ObservableCollection<string> ConversationLocationSuggestions { get; }

        public ObservableCollection<string> ConversationActionSuggestions { get; }

        public ObservableCollection<string> ConversationWeatherSuggestions { get; }

        public ObservableCollection<string> ConversationSeasonSuggestions { get; }

        public ObservableCollection<string> ConversationTimeOfDaySuggestions { get; }

        public ObservableCollection<string> ConversationCostumeSuggestions { get; }

        public ObservableCollection<string> ConversationExpressionSuggestions { get; }

        public ObservableCollection<ExpressionDefinition> ExpressionDefinitions { get; }

        public ObservableCollection<CostumeDefinition> CostumeDefinitions { get; }

        public ObservableCollection<LayerAssetDefinition> LayerAssetDefinitions { get; }

        public ObservableCollection<string> LayerKindOptions { get; }

        public ObservableCollection<string> ExpressionIdOptions { get; }

        public ObservableCollection<string> CostumeIdOptions { get; }

        public ObservableCollection<string> BattleResultTypeOptions { get; }

        public ObservableCollection<string> BattlePanelResultTypeOptions { get; }

        public ObservableCollection<string> BattleSpeakerTypeOptions { get; }

        public ObservableCollection<string> BattleVisualModeOptions { get; }
        public ObservableCollection<string> EndingVisualModeOptions { get; }

        public ObservableCollection<string> BattleStillIdOptions { get; }

        public ObservableCollection<string> LayerPreviewBaseBodyOptions { get; }

        public ObservableCollection<string> LayerPreviewCostumeOptions { get; }

        public ObservableCollection<string> LayerPreviewExpressionOptions { get; }

        public ObservableCollection<LayerPreviewItem> LayerPreviewItems { get; }

        public ObservableCollection<LayerPreviewItem> ProfilePreviewItems { get; }

        public ObservableCollection<string> OutfitLayerAssetOptions { get; }

        public ObservableCollection<LayerPreviewItem> OutfitCompositionPreviewItems { get; }

        public ObservableCollection<HeadPartWorkspaceItem> HeadPartWorkspaceItems { get; }

        public ObservableCollection<LayerPreviewItem> HeadPartPreviewItems { get; }

        public HeadPartWorkspaceItem SelectedHeadPartWorkspaceItem
        {
            get => selectedHeadPartWorkspaceItem;
            set
            {
                if (selectedHeadPartWorkspaceItem == value) return;
                if (selectedHeadPartWorkspaceItem != null)
                {
                    selectedHeadPartWorkspaceItem.PropertyChanged -= SelectedHeadPartWorkspaceItemPropertyChanged;
                }
                selectedHeadPartWorkspaceItem = value;
                if (selectedHeadPartWorkspaceItem != null)
                {
                    selectedHeadPartWorkspaceItem.PropertyChanged += SelectedHeadPartWorkspaceItemPropertyChanged;
                }
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string HeadPartWorkspaceMessage
        {
            get => headPartWorkspaceMessage;
            set
            {
                if (headPartWorkspaceMessage == value) return;
                headPartWorkspaceMessage = value;
                OnPropertyChanged();
            }
        }

        public string StillPromptPreview
        {
            get
            {
                if (SelectedProfile == null || SelectedStillDefinition == null)
                {
                    return string.Empty;
                }

                return BuildStillPositivePrompt(SelectedProfile, SelectedStillDefinition);
            }
        }

        public string WorkspacePath
        {
            get { return characterProjectService.WorkspaceRoot; }
        }

        public string ExportPath
        {
            get { return exportService.ExportDirectory; }
        }

        public string HeroineIdInput
        {
            get { return heroineIdInput; }
            set
            {
                if (heroineIdInput == value) { return; }
                heroineIdInput = value;
                OnPropertyChanged(nameof(HeroineIdInput));
            }
        }

        public string DisplayNameInput
        {
            get { return displayNameInput; }
            set
            {
                if (displayNameInput == value) { return; }
                displayNameInput = value;
                OnPropertyChanged(nameof(DisplayNameInput));
            }
        }

        public string EnemyIdInput
        {
            get { return enemyIdInput; }
            set
            {
                if (enemyIdInput == value) { return; }
                enemyIdInput = value;
                OnPropertyChanged(nameof(EnemyIdInput));
            }
        }

        public string EnemyDisplayNameInput
        {
            get { return enemyDisplayNameInput; }
            set
            {
                if (enemyDisplayNameInput == value) { return; }
                enemyDisplayNameInput = value;
                OnPropertyChanged(nameof(EnemyDisplayNameInput));
            }
        }

        public string EnemyTypeInput
        {
            get { return enemyTypeInput; }
            set
            {
                if (enemyTypeInput == value) { return; }
                enemyTypeInput = value;
                OnPropertyChanged(nameof(EnemyTypeInput));
            }
        }

        public string EnemyAssetIdInput
        {
            get { return enemyAssetIdInput; }
            set
            {
                if (enemyAssetIdInput == value) { return; }
                enemyAssetIdInput = value;
                OnPropertyChanged(nameof(EnemyAssetIdInput));
            }
        }

        public string EnemyImageSourcePathInput
        {
            get { return enemyImageSourcePathInput; }
            set
            {
                if (enemyImageSourcePathInput == value) { return; }
                enemyImageSourcePathInput = value;
                OnPropertyChanged(nameof(EnemyImageSourcePathInput));
            }
        }

        public AssetStatus SelectedEnemyAssetStatus
        {
            get { return selectedEnemyAssetStatus; }
            set
            {
                if (selectedEnemyAssetStatus == value) { return; }
                selectedEnemyAssetStatus = value;
                OnPropertyChanged(nameof(SelectedEnemyAssetStatus));
            }
        }

        public string EnemyAssetPromptInput
        {
            get { return enemyAssetPromptInput; }
            set
            {
                if (enemyAssetPromptInput == value) { return; }
                enemyAssetPromptInput = value;
                OnPropertyChanged(nameof(EnemyAssetPromptInput));
                OnPropertyChanged(nameof(EnemyPromptPreview));
                ClearEnemyComfyWork();
            }
        }

        public string EnemyPromptPreview
        {
            get
            {
                if (SelectedEnemyProfile == null)
                {
                    return string.Empty;
                }

                return BuildEnemyPositivePrompt(SelectedEnemyProfile, EnemyAssetPromptInput);
            }
        }

        public PlayerProfile PlayerProfile
        {
            get { return playerProfile; }
            set
            {
                if (playerProfile == value) { return; }
                if (playerProfile != null)
                {
                    playerProfile.PropertyChanged -= PlayerProfile_PropertyChanged;
                }

                playerProfile = value;
                if (playerProfile != null)
                {
                    playerProfile.PropertyChanged += PlayerProfile_PropertyChanged;
                }

                OnPropertyChanged(nameof(PlayerProfile));
                OnPropertyChanged(nameof(PlayerPromptPreview));
                SelectedPlayerAsset = playerProfile?.Assets?.FirstOrDefault();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string PlayerAssetIdInput
        {
            get { return playerAssetIdInput; }
            set
            {
                if (playerAssetIdInput == value) { return; }
                playerAssetIdInput = value;
                OnPropertyChanged(nameof(PlayerAssetIdInput));
            }
        }

        public string PlayerImageSourcePathInput
        {
            get { return playerImageSourcePathInput; }
            set
            {
                if (playerImageSourcePathInput == value) { return; }
                playerImageSourcePathInput = value;
                OnPropertyChanged(nameof(PlayerImageSourcePathInput));
            }
        }

        public string PlayerAssetPromptInput
        {
            get { return playerAssetPromptInput; }
            set
            {
                if (playerAssetPromptInput == value) { return; }
                playerAssetPromptInput = value;
                OnPropertyChanged(nameof(PlayerAssetPromptInput));
                OnPropertyChanged(nameof(PlayerPromptPreview));
                ClearPlayerComfyWork();
            }
        }

        public AssetStatus SelectedPlayerAssetStatus
        {
            get { return selectedPlayerAssetStatus; }
            set
            {
                if (selectedPlayerAssetStatus == value) { return; }
                selectedPlayerAssetStatus = value;
                OnPropertyChanged(nameof(SelectedPlayerAssetStatus));
            }
        }

        public string PlayerPromptPreview
        {
            get
            {
                if (PlayerProfile == null)
                {
                    return string.Empty;
                }

                return BuildPlayerPositivePrompt(PlayerProfile, PlayerAssetPromptInput);
            }
        }

        public string AssetIdInput
        {
            get { return assetIdInput; }
            set
            {
                if (assetIdInput == value) { return; }
                assetIdInput = value;
                OnPropertyChanged(nameof(AssetIdInput));
            }
        }

        public string ImageSourcePathInput
        {
            get { return imageSourcePathInput; }
            set
            {
                if (imageSourcePathInput == value) { return; }
                imageSourcePathInput = value;
                OnPropertyChanged(nameof(ImageSourcePathInput));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public AssetUsage SelectedAssetUsage
        {
            get { return selectedAssetUsage; }
            set
            {
                if (selectedAssetUsage == value) { return; }
                selectedAssetUsage = value;
                OnPropertyChanged(nameof(SelectedAssetUsage));
            }
        }

        public AssetStatus SelectedAssetStatus
        {
            get { return selectedAssetStatus; }
            set
            {
                if (selectedAssetStatus == value) { return; }
                selectedAssetStatus = value;
                OnPropertyChanged(nameof(SelectedAssetStatus));
            }
        }

        public string SelectedAssetStatusFilter
        {
            get { return selectedAssetStatusFilter; }
            set
            {
                if (selectedAssetStatusFilter == value) { return; }
                selectedAssetStatusFilter = value;
                OnPropertyChanged(nameof(SelectedAssetStatusFilter));
                RefreshFilteredAssets();
            }
        }

        public HeroineAsset SelectedAsset
        {
            get { return selectedAsset; }
            set
            {
                if (selectedAsset == value) { return; }
                selectedAsset = value;
                OnPropertyChanged(nameof(SelectedAsset));
                if (value != null && value.Usage == AssetUsage.Training && selectedTrainingAsset != value)
                {
                    selectedTrainingAsset = value;
                    OnPropertyChanged(nameof(SelectedTrainingAsset));
                }
                else if ((value == null || value.Usage != AssetUsage.Training) && selectedTrainingAsset != null)
                {
                    selectedTrainingAsset = null;
                    OnPropertyChanged(nameof(SelectedTrainingAsset));
                }
                PopulateSelectedAssetEditor();
                RefreshSelectedAssetImagePath();
                if (selectedAsset != null)
                {
                    SelectedPromptTemplateUsage = selectedAsset.Usage;
                }
                else
                {
                    RefreshPromptTemplates();
                }
                LoadPromptForSelectedAsset();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void PopulateSelectedAssetEditor()
        {
            if (SelectedAsset == null)
            {
                AssetIdInput = string.Empty;
                ImageSourcePathInput = string.Empty;
                return;
            }

            AssetIdInput = SelectedAsset.AssetId ?? string.Empty;
            SelectedAssetUsage = SelectedAsset.Usage;
            SelectedAssetStatus = SelectedAsset.Status;
            ImageSourcePathInput = ResolveSelectedAssetSourcePath(SelectedAsset);
        }

        private string ResolveSelectedAssetSourcePath(HeroineAsset asset)
        {
            if (asset == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(asset.SourcePath) && File.Exists(asset.SourcePath))
            {
                return asset.SourcePath;
            }

            if (SelectedProfile != null && !string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                string storedPath = Path.Combine(
                    characterProjectService.GetCharacterDirectory(SelectedProfile.HeroineId),
                    asset.StoredPath);
                if (File.Exists(storedPath))
                {
                    return storedPath;
                }
            }

            return asset.SourcePath ?? string.Empty;
        }

        public TrainingImageEntry SelectedTrainingImageEntry
        {
            get { return selectedTrainingImageEntry; }
            set
            {
                if (selectedTrainingImageEntry == value) { return; }
                selectedTrainingImageEntry = value;
                OnPropertyChanged(nameof(SelectedTrainingImageEntry));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public HeroineAsset SelectedTrainingAsset
        {
            get { return selectedTrainingAsset; }
            set
            {
                if (selectedTrainingAsset == value) { return; }
                selectedTrainingAsset = value;
                OnPropertyChanged(nameof(SelectedTrainingAsset));
                if (value != null && SelectedAsset != value)
                {
                    SelectedAsset = value;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public TrainingDialogueEntry SelectedTrainingDialogueEntry
        {
            get { return selectedTrainingDialogueEntry; }
            private set
            {
                if (selectedTrainingDialogueEntry == value) { return; }
                selectedTrainingDialogueEntry = value;
                OnPropertyChanged(nameof(SelectedTrainingDialogueEntry));
                SelectedTrainingDialogueMessage = value?.Messages?.FirstOrDefault();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public TrainingDialogueMessage SelectedTrainingDialogueMessage
        {
            get { return selectedTrainingDialogueMessage; }
            set
            {
                if (selectedTrainingDialogueMessage == value) { return; }
                selectedTrainingDialogueMessage = value;
                OnPropertyChanged(nameof(SelectedTrainingDialogueMessage));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public AssetUsage SelectedPromptTemplateUsage
        {
            get { return selectedPromptTemplateUsage; }
            set
            {
                if (selectedPromptTemplateUsage == value) { return; }
                selectedPromptTemplateUsage = value;
                OnPropertyChanged(nameof(SelectedPromptTemplateUsage));
                RefreshPromptTemplates();
            }
        }

        public PromptTemplate SelectedPromptTemplate
        {
            get { return selectedPromptTemplate; }
            set
            {
                if (selectedPromptTemplate == value) { return; }
                selectedPromptTemplate = value;
                OnPropertyChanged(nameof(SelectedPromptTemplate));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public StillDefinition SelectedStillDefinition
        {
            get { return selectedStillDefinition; }
            set
            {
                if (selectedStillDefinition == value) { return; }
                if (selectedStillDefinition != null)
                {
                    selectedStillDefinition.PropertyChanged -= SelectedStillDefinitionPropertyChanged;
                }

                selectedStillDefinition = value;
                if (selectedStillDefinition != null)
                {
                    selectedStillDefinition.PropertyChanged += SelectedStillDefinitionPropertyChanged;
                }

                ApplySelectedStillWorkItem();
                OnPropertyChanged(nameof(SelectedStillDefinition));
                OnPropertyChanged(nameof(StillPromptPreview));
                RequestComfyPollingCancellation();
                IsComfyWaitingResult = false;
                CurrentComfyWorkflowPreview = string.Empty;
                CurrentComfyPromptId = string.Empty;
                CurrentComfyResultSummary = string.Empty;
                ClearComfyPreviewImage();
                hasComfyInterruptRequested = false;
                RefreshSelectedStillStatus();
                RefreshProductionStatus();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedStillUsageFilter
        {
            get { return selectedStillUsageFilter; }
            set
            {
                if (selectedStillUsageFilter == value) { return; }
                selectedStillUsageFilter = value;
                OnPropertyChanged(nameof(SelectedStillUsageFilter));
                RefreshFilteredStillDefinitions();
            }
        }

        public ConversationDataKind SelectedConversationDataKind
        {
            get { return selectedConversationDataKind; }
            set
            {
                if (selectedConversationDataKind == value) { return; }
                selectedConversationDataKind = value;
                OnPropertyChanged(nameof(SelectedConversationDataKind));
                if (SelectedVoiceAssignmentTarget == "選択中の会話・イベント行")
                {
                    ApplyConversationVoiceUsageSuggestion();
                }
                RefreshConversationCategorySuggestions();
                RefreshConversationActionSuggestions();
                RefreshFilteredConversationEntries();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ConversationEntry SelectedConversationEntry
        {
            get { return selectedConversationEntry; }
            set
            {
                if (selectedConversationEntry == value) { return; }
                selectedConversationEntry = value;
                SelectedConversationLine = selectedConversationEntry == null || selectedConversationEntry.Lines == null
                    ? null
                    : selectedConversationEntry.Lines.FirstOrDefault();
                SelectedConversationChoice = selectedConversationEntry == null || selectedConversationEntry.Choices == null
                    ? null
                    : selectedConversationEntry.Choices.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedConversationEntry));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ConversationSearchText
        {
            get { return conversationSearchText; }
            set
            {
                if (conversationSearchText == value) { return; }
                conversationSearchText = value;
                OnPropertyChanged(nameof(ConversationSearchText));
                RefreshFilteredConversationEntries();
            }
        }

        public string SelectedConversationCategoryFilter
        {
            get { return selectedConversationCategoryFilter; }
            set
            {
                if (selectedConversationCategoryFilter == value) { return; }
                selectedConversationCategoryFilter = value;
                OnPropertyChanged(nameof(SelectedConversationCategoryFilter));
                RefreshFilteredConversationEntries();
            }
        }

        public string SelectedConversationImageFilter
        {
            get { return selectedConversationImageFilter; }
            set
            {
                if (selectedConversationImageFilter == value) { return; }
                selectedConversationImageFilter = value;
                OnPropertyChanged(nameof(SelectedConversationImageFilter));
                RefreshFilteredConversationEntries();
            }
        }

        public bool ShowOnlyConversationWarnings
        {
            get { return showOnlyConversationWarnings; }
            set
            {
                if (showOnlyConversationWarnings == value) { return; }
                showOnlyConversationWarnings = value;
                OnPropertyChanged(nameof(ShowOnlyConversationWarnings));
                RefreshFilteredConversationEntries();
            }
        }

        public bool ShowOnlyMatchingGameEvents
        {
            get { return showOnlyMatchingGameEvents; }
            set
            {
                if (showOnlyMatchingGameEvents == value) { return; }
                showOnlyMatchingGameEvents = value;
                OnPropertyChanged(nameof(ShowOnlyMatchingGameEvents));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestLocationId
        {
            get { return gameEventTestLocationId; }
            set
            {
                if (gameEventTestLocationId == value) { return; }
                gameEventTestLocationId = value;
                OnPropertyChanged(nameof(GameEventTestLocationId));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestAffection
        {
            get { return gameEventTestAffection; }
            set
            {
                if (gameEventTestAffection == value) { return; }
                gameEventTestAffection = value;
                OnPropertyChanged(nameof(GameEventTestAffection));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestWeather
        {
            get { return gameEventTestWeather; }
            set
            {
                if (gameEventTestWeather == value) { return; }
                gameEventTestWeather = value;
                OnPropertyChanged(nameof(GameEventTestWeather));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestSeason
        {
            get { return gameEventTestSeason; }
            set
            {
                if (gameEventTestSeason == value) { return; }
                gameEventTestSeason = value;
                OnPropertyChanged(nameof(GameEventTestSeason));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestTimeOfDay
        {
            get { return gameEventTestTimeOfDay; }
            set
            {
                if (gameEventTestTimeOfDay == value) { return; }
                gameEventTestTimeOfDay = value;
                OnPropertyChanged(nameof(GameEventTestTimeOfDay));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestActionId
        {
            get { return gameEventTestActionId; }
            set
            {
                if (gameEventTestActionId == value) { return; }
                gameEventTestActionId = value;
                OnPropertyChanged(nameof(GameEventTestActionId));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestCostumeId
        {
            get { return gameEventTestCostumeId; }
            set
            {
                if (gameEventTestCostumeId == value) { return; }
                gameEventTestCostumeId = value;
                OnPropertyChanged(nameof(GameEventTestCostumeId));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestItemId
        {
            get { return gameEventTestItemId; }
            set
            {
                if (gameEventTestItemId == value) { return; }
                gameEventTestItemId = value;
                OnPropertyChanged(nameof(GameEventTestItemId));
                RefreshFilteredConversationEntries();
            }
        }

        public string GameEventTestFlagIdsText
        {
            get { return gameEventTestFlagIdsText; }
            set
            {
                if (gameEventTestFlagIdsText == value) { return; }
                gameEventTestFlagIdsText = value;
                OnPropertyChanged(nameof(GameEventTestFlagIdsText));
                RefreshFilteredConversationEntries();
            }
        }

        public ConversationLine SelectedConversationLine
        {
            get { return selectedConversationLine; }
            set
            {
                if (selectedConversationLine == value) { return; }
                selectedConversationLine = value;
                OnPropertyChanged(nameof(SelectedConversationLine));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ConversationChoice SelectedConversationChoice
        {
            get { return selectedConversationChoice; }
            set
            {
                if (selectedConversationChoice == value) { return; }
                selectedConversationChoice = value;
                OnPropertyChanged(nameof(SelectedConversationChoice));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedConversationCategorySuggestion
        {
            get { return selectedConversationCategorySuggestion; }
            set
            {
                if (selectedConversationCategorySuggestion == value) { return; }
                selectedConversationCategorySuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationCategorySuggestion));
            }
        }

        public string SelectedConversationLocationSuggestion
        {
            get { return selectedConversationLocationSuggestion; }
            set
            {
                if (selectedConversationLocationSuggestion == value) { return; }
                selectedConversationLocationSuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationLocationSuggestion));
            }
        }

        public string SelectedConversationActionSuggestion
        {
            get { return selectedConversationActionSuggestion; }
            set
            {
                if (selectedConversationActionSuggestion == value) { return; }
                selectedConversationActionSuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationActionSuggestion));
            }
        }

        public string SelectedConversationWeatherSuggestion
        {
            get { return selectedConversationWeatherSuggestion; }
            set
            {
                if (selectedConversationWeatherSuggestion == value) { return; }
                selectedConversationWeatherSuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationWeatherSuggestion));
            }
        }

        public string SelectedConversationSeasonSuggestion
        {
            get { return selectedConversationSeasonSuggestion; }
            set
            {
                if (selectedConversationSeasonSuggestion == value) { return; }
                selectedConversationSeasonSuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationSeasonSuggestion));
            }
        }

        public string SelectedConversationTimeOfDaySuggestion
        {
            get { return selectedConversationTimeOfDaySuggestion; }
            set
            {
                if (selectedConversationTimeOfDaySuggestion == value) { return; }
                selectedConversationTimeOfDaySuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationTimeOfDaySuggestion));
            }
        }

        public string SelectedConversationCostumeSuggestion
        {
            get { return selectedConversationCostumeSuggestion; }
            set
            {
                if (selectedConversationCostumeSuggestion == value) { return; }
                selectedConversationCostumeSuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationCostumeSuggestion));
            }
        }

        public string SelectedConversationExpressionSuggestion
        {
            get { return selectedConversationExpressionSuggestion; }
            set
            {
                if (selectedConversationExpressionSuggestion == value) { return; }
                selectedConversationExpressionSuggestion = value;
                OnPropertyChanged(nameof(SelectedConversationExpressionSuggestion));
            }
        }

        public HeroineAsset SelectedConversationImageAsset
        {
            get { return selectedConversationImageAsset; }
            set
            {
                if (selectedConversationImageAsset == value) { return; }
                selectedConversationImageAsset = value;
                OnPropertyChanged(nameof(SelectedConversationImageAsset));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ExpressionDefinition SelectedExpressionDefinition
        {
            get { return selectedExpressionDefinition; }
            set
            {
                if (selectedExpressionDefinition == value) { return; }
                selectedExpressionDefinition = value;
                OnPropertyChanged(nameof(SelectedExpressionDefinition));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public CostumeDefinition SelectedCostumeDefinition
        {
            get { return selectedCostumeDefinition; }
            set
            {
                if (selectedCostumeDefinition == value) { return; }
                selectedCostumeDefinition = value;
                OnPropertyChanged(nameof(SelectedCostumeDefinition));
                RefreshOutfitCompositionEditor();
                RefreshHeadPartWorkspace();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public LayerAssetDefinition SelectedLayerAssetDefinition
        {
            get { return selectedLayerAssetDefinition; }
            set
            {
                if (selectedLayerAssetDefinition == value) { return; }
                selectedLayerAssetDefinition = value;
                OnPropertyChanged(nameof(SelectedLayerAssetDefinition));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string DefinitionCatalogValidationMessage
        {
            get { return definitionCatalogValidationMessage; }
            set
            {
                if (definitionCatalogValidationMessage == value) { return; }
                definitionCatalogValidationMessage = value;
                OnPropertyChanged(nameof(DefinitionCatalogValidationMessage));
            }
        }

        public string SelectedLayerPreviewBaseBodyId
        {
            get { return selectedLayerPreviewBaseBodyId; }
            set
            {
                if (selectedLayerPreviewBaseBodyId == value) { return; }
                selectedLayerPreviewBaseBodyId = value;
                OnPropertyChanged(nameof(SelectedLayerPreviewBaseBodyId));
                RefreshLayerPreview();
            }
        }

        public string SelectedLayerPreviewCostumeId
        {
            get { return selectedLayerPreviewCostumeId; }
            set
            {
                if (selectedLayerPreviewCostumeId == value) { return; }
                selectedLayerPreviewCostumeId = value;
                OnPropertyChanged(nameof(SelectedLayerPreviewCostumeId));
                RefreshLayerPreview();
            }
        }

        public string SelectedLayerPreviewExpressionId
        {
            get { return selectedLayerPreviewExpressionId; }
            set
            {
                if (selectedLayerPreviewExpressionId == value) { return; }
                selectedLayerPreviewExpressionId = value;
                OnPropertyChanged(nameof(SelectedLayerPreviewExpressionId));
                RefreshLayerPreview();
            }
        }

        public string LayerPreviewMessage
        {
            get { return layerPreviewMessage; }
            set
            {
                if (layerPreviewMessage == value) { return; }
                layerPreviewMessage = value;
                OnPropertyChanged(nameof(LayerPreviewMessage));
            }
        }

        public LayerPreviewItem SelectedLayerPreviewItem
        {
            get => selectedLayerPreviewItem;
            set
            {
                if (selectedLayerPreviewItem == value) return;
                selectedLayerPreviewItem = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedOutfitCostumeBodyAssetId
        {
            get { return selectedOutfitCostumeBodyAssetId; }
            set
            {
                if (selectedOutfitCostumeBodyAssetId == value) return;
                selectedOutfitCostumeBodyAssetId = value;
                OnPropertyChanged(nameof(SelectedOutfitCostumeBodyAssetId));
                RefreshOutfitCompositionPreview();
            }
        }

        public string SelectedOutfitBackAccessoryAssetId
        {
            get { return selectedOutfitBackAccessoryAssetId; }
            set
            {
                if (selectedOutfitBackAccessoryAssetId == value) return;
                selectedOutfitBackAccessoryAssetId = value;
                OnPropertyChanged(nameof(SelectedOutfitBackAccessoryAssetId));
                RefreshOutfitCompositionPreview();
            }
        }

        public string SelectedOutfitFrontAccessoryAssetId
        {
            get { return selectedOutfitFrontAccessoryAssetId; }
            set
            {
                if (selectedOutfitFrontAccessoryAssetId == value) return;
                selectedOutfitFrontAccessoryAssetId = value;
                OnPropertyChanged(nameof(SelectedOutfitFrontAccessoryAssetId));
                RefreshOutfitCompositionPreview();
            }
        }

        public string OutfitCompositionMessage
        {
            get { return outfitCompositionMessage; }
            set
            {
                if (outfitCompositionMessage == value) return;
                outfitCompositionMessage = value;
                OnPropertyChanged(nameof(OutfitCompositionMessage));
            }
        }

        public string ProfilePreviewImagePath
        {
            get { return profilePreviewImagePath; }
            set
            {
                if (profilePreviewImagePath == value) { return; }
                profilePreviewImagePath = value;
                OnPropertyChanged(nameof(ProfilePreviewImagePath));
            }
        }

        public string ProfilePreviewMessage
        {
            get { return profilePreviewMessage; }
            set
            {
                if (profilePreviewMessage == value) { return; }
                profilePreviewMessage = value;
                OnPropertyChanged(nameof(ProfilePreviewMessage));
            }
        }

        public PromptRecord CurrentPromptRecord
        {
            get { return currentPromptRecord; }
            set
            {
                if (currentPromptRecord == value) { return; }
                currentPromptRecord = value;
                OnPropertyChanged(nameof(CurrentPromptRecord));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public HeroineProfile SelectedProfile
        {
            get { return selectedProfile; }
            set
            {
                if (selectedProfile == value) { return; }
                if (selectedProfile != null)
                {
                    selectedProfile.PropertyChanged -= SelectedProfilePropertyChanged;
                }

                selectedProfile = value;
                if (selectedProfile != null)
                {
                    selectedProfile.PropertyChanged += SelectedProfilePropertyChanged;
                }

                OnPropertyChanged(nameof(SelectedProfile));
                SelectedOutfitMessageOverride = selectedProfile?.OutfitMessageOverrides?.FirstOrDefault();
                SelectedOutfitReactionMessageOverride = selectedProfile?.OutfitReactionMessageOverrides?.FirstOrDefault();
                SelectedTrainingImageEntry = selectedProfile?.TrainingImages?.Items?.FirstOrDefault();
                SelectedBattleResultEvent = null;
                SelectedBattlePanelMessage = null;
                RefreshStillPromptAfterProfilePromptChanged();
                LoadStillDefinitions();
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                RefreshLayerPreviewOptions();
                RefreshLayerPreview();
                RefreshProfilePreview();
                RefreshOutfitLayerAssetOptions();
                RefreshOutfitCompositionEditor();
                RefreshConversationCategorySuggestions();
                RefreshFilteredConversationEntries();
                RefreshSelectedStillStatus();
                RefreshAvailableVoiceIds();
                RefreshFilteredAudioLibrary();
                SelectedMenuAction = selectedProfile?.MenuActions?.FirstOrDefault();
                ValidateMenuActions();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public EnemyProfile SelectedEnemyProfile
        {
            get { return selectedEnemyProfile; }
            set
            {
                if (selectedEnemyProfile == value) { return; }
                if (selectedEnemyProfile != null)
                {
                    selectedEnemyProfile.PropertyChanged -= SelectedEnemyProfile_PropertyChanged;
                }

                selectedEnemyProfile = value;
                if (selectedEnemyProfile != null)
                {
                    selectedEnemyProfile.PropertyChanged += SelectedEnemyProfile_PropertyChanged;
                }

                OnPropertyChanged(nameof(SelectedEnemyProfile));
                OnPropertyChanged(nameof(EnemyPromptPreview));
                SelectedEnemyAsset = selectedEnemyProfile?.Assets?.FirstOrDefault();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public EnemyAsset SelectedEnemyAsset
        {
            get { return selectedEnemyAsset; }
            set
            {
                if (selectedEnemyAsset == value) { return; }
                selectedEnemyAsset = value;
                OnPropertyChanged(nameof(SelectedEnemyAsset));
                RefreshSelectedEnemyAssetPreview();
                ApplySelectedEnemyAssetToInputs();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public PlayerAsset SelectedPlayerAsset
        {
            get { return selectedPlayerAsset; }
            set
            {
                if (selectedPlayerAsset == value) { return; }
                selectedPlayerAsset = value;
                OnPropertyChanged(nameof(SelectedPlayerAsset));
                RefreshSelectedPlayerAssetPreview();
                ApplySelectedPlayerAssetToInputs();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public OutfitMessageOverride SelectedOutfitMessageOverride
        {
            get { return selectedOutfitMessageOverride; }
            set
            {
                if (selectedOutfitMessageOverride == value) { return; }
                selectedOutfitMessageOverride = value;
                OnPropertyChanged(nameof(SelectedOutfitMessageOverride));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public OutfitReactionMessageOverride SelectedOutfitReactionMessageOverride
        {
            get { return selectedOutfitReactionMessageOverride; }
            set
            {
                if (selectedOutfitReactionMessageOverride == value) { return; }
                selectedOutfitReactionMessageOverride = value;
                OnPropertyChanged(nameof(SelectedOutfitReactionMessageOverride));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ExportReport LastExportReport
        {
            get { return lastExportReport; }
            set
            {
                if (lastExportReport == value) { return; }
                lastExportReport = value;
                OnPropertyChanged(nameof(LastExportReport));
            }
        }

        public EnemyExportReport LastEnemyExportReport
        {
            get { return lastEnemyExportReport; }
            set
            {
                if (lastEnemyExportReport == value) { return; }
                lastEnemyExportReport = value;
                OnPropertyChanged(nameof(LastEnemyExportReport));
            }
        }

        public PlayerExportReport LastPlayerExportReport
        {
            get { return lastPlayerExportReport; }
            set
            {
                if (lastPlayerExportReport == value) { return; }
                lastPlayerExportReport = value;
                OnPropertyChanged(nameof(LastPlayerExportReport));
            }
        }

        public string SelectedAssetImagePath
        {
            get { return selectedAssetImagePath; }
            set
            {
                if (selectedAssetImagePath == value) { return; }
                selectedAssetImagePath = value;
                OnPropertyChanged(nameof(SelectedAssetImagePath));
            }
        }

        public string SelectedAssetImageMessage
        {
            get { return selectedAssetImageMessage; }
            set
            {
                if (selectedAssetImageMessage == value) { return; }
                selectedAssetImageMessage = value;
                OnPropertyChanged(nameof(SelectedAssetImageMessage));
            }
        }

        public string SelectedEnemyAssetImagePath
        {
            get { return selectedEnemyAssetImagePath; }
            set
            {
                if (selectedEnemyAssetImagePath == value) { return; }
                selectedEnemyAssetImagePath = value;
                OnPropertyChanged(nameof(SelectedEnemyAssetImagePath));
            }
        }

        public string SelectedEnemyAssetImageMessage
        {
            get { return selectedEnemyAssetImageMessage; }
            set
            {
                if (selectedEnemyAssetImageMessage == value) { return; }
                selectedEnemyAssetImageMessage = value;
                OnPropertyChanged(nameof(SelectedEnemyAssetImageMessage));
            }
        }

        public string SelectedPlayerAssetImagePath
        {
            get { return selectedPlayerAssetImagePath; }
            set
            {
                if (selectedPlayerAssetImagePath == value) { return; }
                selectedPlayerAssetImagePath = value;
                OnPropertyChanged(nameof(SelectedPlayerAssetImagePath));
            }
        }

        public string SelectedPlayerAssetImageMessage
        {
            get { return selectedPlayerAssetImageMessage; }
            set
            {
                if (selectedPlayerAssetImageMessage == value) { return; }
                selectedPlayerAssetImageMessage = value;
                OnPropertyChanged(nameof(SelectedPlayerAssetImageMessage));
            }
        }

        public string SelectedStillAssetStatusText
        {
            get { return selectedStillAssetStatusText; }
            set
            {
                if (selectedStillAssetStatusText == value) { return; }
                selectedStillAssetStatusText = value;
                OnPropertyChanged(nameof(SelectedStillAssetStatusText));
            }
        }

        public string SelectedStillImageStatusText
        {
            get { return selectedStillImageStatusText; }
            set
            {
                if (selectedStillImageStatusText == value) { return; }
                selectedStillImageStatusText = value;
                OnPropertyChanged(nameof(SelectedStillImageStatusText));
            }
        }

        public string SelectedStillPromptStatusText
        {
            get { return selectedStillPromptStatusText; }
            set
            {
                if (selectedStillPromptStatusText == value) { return; }
                selectedStillPromptStatusText = value;
                OnPropertyChanged(nameof(SelectedStillPromptStatusText));
            }
        }

        public string SelectedStillImagePath
        {
            get { return selectedStillImagePath; }
            set
            {
                if (selectedStillImagePath == value) { return; }
                selectedStillImagePath = value;
                OnPropertyChanged(nameof(SelectedStillImagePath));
            }
        }

        public string SelectedStillImageMessage
        {
            get { return selectedStillImageMessage; }
            set
            {
                if (selectedStillImageMessage == value) { return; }
                selectedStillImageMessage = value;
                OnPropertyChanged(nameof(SelectedStillImageMessage));
            }
        }

        public ComfySettings ComfySettings
        {
            get { return comfySettings; }
            set
            {
                if (comfySettings == value) { return; }
                comfySettings = value;
                OnPropertyChanged(nameof(ComfySettings));
            }
        }

        public string ComfySettingsSummary
        {
            get { return comfySettingsSummary; }
            set
            {
                if (comfySettingsSummary == value) { return; }
                comfySettingsSummary = value;
                OnPropertyChanged(nameof(ComfySettingsSummary));
            }
        }

        public string ComfyWorkflowTemplateEditorText
        {
            get { return comfyWorkflowTemplateEditorText; }
            set
            {
                if (comfyWorkflowTemplateEditorText == value) { return; }
                comfyWorkflowTemplateEditorText = value;
                OnPropertyChanged(nameof(ComfyWorkflowTemplateEditorText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CurrentComfyWorkflowPreview
        {
            get { return currentComfyWorkflowPreview; }
            set
            {
                if (currentComfyWorkflowPreview == value) { return; }
                currentComfyWorkflowPreview = value;
                OnPropertyChanged(nameof(CurrentComfyWorkflowPreview));
            }
        }

        public string CurrentComfyPromptId
        {
            get { return currentComfyPromptId; }
            set
            {
                if (currentComfyPromptId == value) { return; }
                currentComfyPromptId = value;
                OnPropertyChanged(nameof(CurrentComfyPromptId));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CurrentComfyResultSummary
        {
            get { return currentComfyResultSummary; }
            set
            {
                if (currentComfyResultSummary == value) { return; }
                currentComfyResultSummary = value;
                OnPropertyChanged(nameof(CurrentComfyResultSummary));
            }
        }

        public string CurrentComfyPreviewImagePath
        {
            get { return currentComfyPreviewImagePath; }
            set
            {
                if (currentComfyPreviewImagePath == value) { return; }
                currentComfyPreviewImagePath = value;
                OnPropertyChanged(nameof(CurrentComfyPreviewImagePath));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CurrentComfyPreviewImageMessage
        {
            get { return currentComfyPreviewImageMessage; }
            set
            {
                if (currentComfyPreviewImageMessage == value) { return; }
                currentComfyPreviewImageMessage = value;
                OnPropertyChanged(nameof(CurrentComfyPreviewImageMessage));
            }
        }

        public bool IsComfySubmitting
        {
            get { return isComfySubmitting; }
            set
            {
                if (isComfySubmitting == value) { return; }
                isComfySubmitting = value;
                OnPropertyChanged(nameof(IsComfySubmitting));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsComfyCheckingResult
        {
            get { return isComfyCheckingResult; }
            set
            {
                if (isComfyCheckingResult == value) { return; }
                isComfyCheckingResult = value;
                OnPropertyChanged(nameof(IsComfyCheckingResult));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsComfyFetchingImage
        {
            get { return isComfyFetchingImage; }
            set
            {
                if (isComfyFetchingImage == value) { return; }
                isComfyFetchingImage = value;
                OnPropertyChanged(nameof(IsComfyFetchingImage));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsComfyWaitingResult
        {
            get { return isComfyWaitingResult; }
            set
            {
                if (isComfyWaitingResult == value) { return; }
                isComfyWaitingResult = value;
                OnPropertyChanged(nameof(IsComfyWaitingResult));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsComfyInterrupting
        {
            get { return isComfyInterrupting; }
            set
            {
                if (isComfyInterrupting == value) { return; }
                isComfyInterrupting = value;
                OnPropertyChanged(nameof(IsComfyInterrupting));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string StatusMessage
        {
            get { return statusMessage; }
            set
            {
                if (statusMessage == value) { return; }
                statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string LastBattleMessageImportReport
        {
            get { return lastBattleMessageImportReport; }
            set
            {
                if (lastBattleMessageImportReport == value) { return; }
                lastBattleMessageImportReport = value;
                OnPropertyChanged(nameof(LastBattleMessageImportReport));
            }
        }

        public int SelectedMainTabIndex
        {
            get { return selectedMainTabIndex; }
            set
            {
                if (selectedMainTabIndex == value) { return; }
                selectedMainTabIndex = value;
                OnPropertyChanged(nameof(SelectedMainTabIndex));
            }
        }

        public bool ShowOnlyIncompleteProductionStatus
        {
            get { return showOnlyIncompleteProductionStatus; }
            set
            {
                if (showOnlyIncompleteProductionStatus == value) { return; }
                showOnlyIncompleteProductionStatus = value;
                OnPropertyChanged(nameof(ShowOnlyIncompleteProductionStatus));
                RefreshProductionStatus();
            }
        }

        public string UnityProjectPath
        {
            get { return unityProjectPath; }
            set
            {
                if (unityProjectPath == value) { return; }
                unityProjectPath = value;
                OnPropertyChanged(nameof(UnityProjectPath));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string AudioLibrarySearchText
        {
            get { return audioLibrarySearchText; }
            set
            {
                if (audioLibrarySearchText == value) { return; }
                audioLibrarySearchText = value;
                OnPropertyChanged(nameof(AudioLibrarySearchText));
                RefreshFilteredAudioLibrary();
            }
        }

        public string BgmSeAudioSearchText
        {
            get { return bgmSeAudioSearchText; }
            set
            {
                if (bgmSeAudioSearchText == value) { return; }
                bgmSeAudioSearchText = value;
                OnPropertyChanged(nameof(BgmSeAudioSearchText));
                RefreshFilteredAudioLibrary();
            }
        }

        public string VoiceAudioSearchText
        {
            get { return voiceAudioSearchText; }
            set
            {
                if (voiceAudioSearchText == value) { return; }
                voiceAudioSearchText = value;
                OnPropertyChanged(nameof(VoiceAudioSearchText));
                RefreshFilteredAudioLibrary();
            }
        }

        public string SelectedAudioCategory
        {
            get { return selectedAudioCategory; }
            set
            {
                if (selectedAudioCategory == value) { return; }
                selectedAudioCategory = value;
                OnPropertyChanged(nameof(SelectedAudioCategory));
                RefreshFilteredAudioLibrary();
            }
        }

        public bool ShowOnlySelectedHeroineAudio
        {
            get { return showOnlySelectedHeroineAudio; }
            set
            {
                if (showOnlySelectedHeroineAudio == value) { return; }
                showOnlySelectedHeroineAudio = value;
                OnPropertyChanged(nameof(ShowOnlySelectedHeroineAudio));
                RefreshFilteredAudioLibrary();
            }
        }

        public bool ShowOnlyUnusedVoice
        {
            get { return showOnlyUnusedVoice; }
            set
            {
                if (showOnlyUnusedVoice == value) { return; }
                showOnlyUnusedVoice = value;
                OnPropertyChanged(nameof(ShowOnlyUnusedVoice));
                RefreshFilteredAudioLibrary();
            }
        }

        public string AudioLibrarySummary
        {
            get { return audioLibrarySummary; }
            set
            {
                if (audioLibrarySummary == value) { return; }
                audioLibrarySummary = value;
                OnPropertyChanged(nameof(AudioLibrarySummary));
            }
        }

        public string BgmSeAudioSummary
        {
            get { return bgmSeAudioSummary; }
            set
            {
                if (bgmSeAudioSummary == value) { return; }
                bgmSeAudioSummary = value;
                OnPropertyChanged(nameof(BgmSeAudioSummary));
            }
        }

        public string VoiceAudioSummary
        {
            get { return voiceAudioSummary; }
            set
            {
                if (voiceAudioSummary == value) { return; }
                voiceAudioSummary = value;
                OnPropertyChanged(nameof(VoiceAudioSummary));
            }
        }

        public AudioLibraryItem SelectedAudioLibraryItem
        {
            get { return selectedAudioLibraryItem; }
            set
            {
                if (selectedAudioLibraryItem == value) { return; }
                selectedAudioLibraryItem = value;
                OnPropertyChanged(nameof(SelectedAudioLibraryItem));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedVoiceUsage
        {
            get { return selectedVoiceUsage; }
            set
            {
                if (selectedVoiceUsage == value) { return; }
                selectedVoiceUsage = value;
                OnPropertyChanged(nameof(SelectedVoiceUsage));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string VoiceRegistrationId
        {
            get { return voiceRegistrationId; }
            set
            {
                if (voiceRegistrationId == value) { return; }
                voiceRegistrationId = value;
                OnPropertyChanged(nameof(VoiceRegistrationId));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectedVoiceAssignmentTarget
        {
            get { return selectedVoiceAssignmentTarget; }
            set
            {
                if (selectedVoiceAssignmentTarget == value) { return; }
                selectedVoiceAssignmentTarget = value;
                OnPropertyChanged(nameof(SelectedVoiceAssignmentTarget));
                if (value == "選択中の会話・イベント行")
                {
                    ApplyConversationVoiceUsageSuggestion();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public BattleResultEventEntry SelectedBattleResultEvent
        {
            get { return selectedBattleResultEvent; }
            set
            {
                if (selectedBattleResultEvent == value) { return; }
                selectedBattleResultEvent = value;
                OnPropertyChanged(nameof(SelectedBattleResultEvent));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public BattlePanelResultMessageEntry SelectedBattlePanelMessage
        {
            get { return selectedBattlePanelMessage; }
            set
            {
                if (selectedBattlePanelMessage == value) { return; }
                selectedBattlePanelMessage = value;
                OnPropertyChanged(nameof(SelectedBattlePanelMessage));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand CreateCharacterCommand { get; }

        public ICommand SaveSelectedProfileCommand { get; }

        public ICommand ImportHeroineProfileFromUnityCommand { get; }

        public ICommand OpenBattleMessagesTabCommand { get; }

        public ICommand RefreshProductionStatusCommand { get; }

        public ICommand OpenProductionStatusTargetCommand { get; }

        public ICommand BrowseUnityProjectCommand { get; }

        public ICommand RefreshAudioLibraryCommand { get; }

        public ICommand PlaySelectedAudioCommand { get; }

        public ICommand StopAudioPreviewCommand { get; }

        public ICommand RegisterSelectedAudioCommand { get; }

        public ICommand OpenSelectedAudioFolderCommand { get; }

        public ICommand RegisterVoiceCommand { get; }

        public ICommand OpenVoiceFolderCommand { get; }

        public ICommand OpenSelectedVoiceFolderCommand { get; }

        public ICommand AddOutfitMessageOverrideCommand { get; }

        public ICommand RemoveOutfitMessageOverrideCommand { get; }

        public ICommand AddOutfitReactionMessageOverrideCommand { get; }

        public ICommand RemoveOutfitReactionMessageOverrideCommand { get; }

        public ICommand RefreshProfilesCommand { get; }

        public ICommand RefreshEnemiesCommand { get; }

        public ICommand CreateEnemyCommand { get; }

        public ICommand SaveSelectedEnemyCommand { get; }

        public ICommand BrowseEnemyImageCommand { get; }

        public ICommand AddEnemyImageAssetCommand { get; }

        public ICommand UnregisterEnemyImageAssetCommand { get; }

        public ICommand ExportSelectedEnemyCommand { get; }

        public ICommand AddEnemyStandardAssetDataCommand { get; }

        public ICommand BuildEnemyComfyWorkflowPreviewCommand { get; }

        public ICommand SubmitEnemyComfyPromptCommand { get; }

        public ICommand AdoptEnemyComfyImageCommand { get; }

        public ICommand ReloadPlayerProfileCommand { get; }

        public ICommand SavePlayerProfileCommand { get; }

        public ICommand BrowsePlayerImageCommand { get; }

        public ICommand AddPlayerImageAssetCommand { get; }

        public ICommand UnregisterPlayerImageAssetCommand { get; }

        public ICommand ExportPlayerCommand { get; }

        public ICommand AddPlayerStandardAssetDataCommand { get; }

        public ICommand BuildPlayerComfyWorkflowPreviewCommand { get; }

        public ICommand SubmitPlayerComfyPromptCommand { get; }

        public ICommand AdoptPlayerComfyImageCommand { get; }

        public ICommand BrowseImageCommand { get; }

        public ICommand AddImageAssetCommand { get; }

        public ICommand UnregisterImageAssetCommand { get; }

        public ICommand SaveImageAssetsCommand { get; }

        public ICommand AddHeroineBattleStandardAssetDataCommand { get; }

        public ICommand AddTrainingStandardAssetDataCommand { get; }

        public ICommand AddTrainingImageEntryCommand { get; }

        public ICommand RemoveTrainingImageEntryCommand { get; }

        public ICommand AdoptTrainingExternalImageCommand { get; }

        public ICommand AddTrainingDialogueMessageCommand { get; }

        public ICommand RemoveTrainingDialogueMessageCommand { get; }

        public ICommand ImportTrainingDialoguesFromUnityCommand { get; }

        public ICommand ImportTrainingCatalogFromUnityCommand { get; }

        public ICommand SavePromptRecordCommand { get; }

        public ICommand ApplyPromptTemplateCommand { get; }

        public ICommand ApplyStillPromptCommand { get; }

        public ICommand PrepareImageRegistrationForStillCommand { get; }

        public ICommand ExportSelectedProfileCommand { get; }

        public ICommand OpenExportDirectoryCommand { get; }

        public ICommand ReloadComfySettingsCommand { get; }

        public ICommand LoadComfyWorkflowTemplateCommand { get; }

        public ICommand SaveComfyWorkflowTemplateCommand { get; }

        public ICommand BuildComfyWorkflowPreviewCommand { get; }

        public ICommand SubmitSelectedAssetComfyPromptCommand { get; }

        public ICommand BuildStillComfyWorkflowPreviewCommand { get; }

        public ICommand SubmitStillComfyPromptCommand { get; }

        public ICommand CancelStillComfyPollingCommand { get; }

        public ICommand InterruptComfyGenerationCommand { get; }

        public ICommand CheckStillComfyResultCommand { get; }

        public ICommand FetchStillComfyImageCommand { get; }

        public ICommand AdoptStillComfyImageCommand { get; }

        public ICommand AdoptSelectedAssetComfyImageCommand { get; }

        public ICommand AddConversationEntryCommand { get; }

        public ICommand RemoveConversationEntryCommand { get; }

        public ICommand AddConversationLineCommand { get; }

        public ICommand RemoveConversationLineCommand { get; }

        public ICommand AddConversationChoiceCommand { get; }

        public ICommand RemoveConversationChoiceCommand { get; }

        public ICommand SaveConversationDataCommand { get; }

        public ICommand PrepareStandardMenuActionsCommand { get; }

        public ICommand ApplyStandardMenuLayoutCommand { get; }

        public ICommand AddMenuActionCommand { get; }

        public ICommand RemoveMenuActionCommand { get; }

        public ICommand ValidateMenuActionsCommand { get; }

        public ICommand ImportActionsFromUnityCommand { get; }

        public ICommand ImportConversationsFromUnityCommand { get; }

        public ICommand ImportGameEventsFromUnityCommand { get; }

        public ICommand ImportScheduledEventsFromUnityCommand { get; }

        public ICommand ImportEndingsFromUnityCommand { get; }

        public ICommand ApplyConversationCategorySuggestionCommand { get; }

        public ICommand ApplyConversationEventTemplateCommand { get; }

        public ICommand ApplyConversationConditionSuggestionsCommand { get; }

        public ICommand ApplyConversationExpressionSuggestionCommand { get; }

        public ICommand AddConversationImageAssetCommand { get; }

        public ICommand GenerateConversationIdCommand { get; }

        public ICommand AddExpressionDefinitionCommand { get; }

        public ICommand RemoveExpressionDefinitionCommand { get; }

        public ICommand AddCostumeDefinitionCommand { get; }

        public ICommand RemoveCostumeDefinitionCommand { get; }

        public ICommand AddLayerAssetDefinitionCommand { get; }

        public ICommand RemoveLayerAssetDefinitionCommand { get; }

        public ICommand SaveDefinitionCatalogCommand { get; }

        public ICommand ReloadDefinitionCatalogCommand { get; }

        public ICommand RefreshLayerPreviewCommand { get; }

        public ICommand MoveLayerPreviewBackwardCommand { get; }

        public ICommand MoveLayerPreviewForwardCommand { get; }

        public ICommand ImportLayeredSpritesFromUnityCommand { get; }

        public ICommand ApplyOutfitCompositionCommand { get; }

        public ICommand AddOutfitAccessoryAssetTemplatesCommand { get; }

        public ICommand BrowseHeadPartImageCommand { get; }

        public ICommand TestHeadPartPreviewCommand { get; }

        public ICommand AdoptHeadPartImageCommand { get; }

        public MainWindowModel()
        {
            characterProjectService = new CharacterProjectService();
            enemyProjectService = new EnemyProjectService(characterProjectService.WorkspaceRoot);
            playerProjectService = new PlayerProjectService(characterProjectService.WorkspaceRoot);
            promptRecordService = new PromptRecordService(characterProjectService);
            promptTemplateService = new PromptTemplateService(characterProjectService.WorkspaceRoot);
            definitionCatalogService = new DefinitionCatalogService(characterProjectService.WorkspaceRoot);
            stillDefinitionService = new StillDefinitionService(characterProjectService.WorkspaceRoot);
            imageInspectionService = new ImageInspectionService();
            comfySettingsService = new ComfySettingsService(characterProjectService.WorkspaceRoot);
            comfyWorkflowService = new ComfyWorkflowService(characterProjectService.WorkspaceRoot);
            comfyClientService = new ComfyClientService();
            exportService = new ExportService(characterProjectService, imageInspectionService);
            enemyExportService = new EnemyExportService(enemyProjectService, imageInspectionService);
            playerExportService = new PlayerExportService(playerProjectService, imageInspectionService);
            audioLibraryService = new AudioLibraryService();
            audioPreviewService = new AudioPreviewService();
            audioPreviewService.PlaybackFailed += OnAudioPreviewFailed;
            Profiles = new ObservableCollection<HeroineProfile>();
            ProductionStatusCategories = new ObservableCollection<ProductionStatusCell>();
            AudioLibraryItems = new ObservableCollection<AudioLibraryItem>();
            BgmSeAudioItems = new ObservableCollection<AudioLibraryItem>();
            VoiceAudioItems = new ObservableCollection<AudioLibraryItem>();
            AudioCategoryOptions = new ObservableCollection<string>
            {
                "すべて", "BGM", "SE", "VOICE", "未配置"
            };
            AvailableVoiceIds = new ObservableCollection<string>();
            VoiceUsageOptions = new ObservableCollection<string>
            {
                "Training", "Battle", "Conversation", "Event", "Other"
            };
            VoiceAssignmentTargetOptions = new ObservableCollection<string>
            {
                "登録のみ",
                "選択中の訓練セリフ",
                "選択中の戦闘後イベント",
                "選択中の戦闘パネルメッセージ",
                "選択中の会話・イベント行"
            };
            EnemyProfiles = new ObservableCollection<EnemyProfile>();
            FilteredAssets = new ObservableCollection<HeroineAsset>();
            AcceptedAssets = new ObservableCollection<HeroineAsset>();
            TrainingAssets = new ObservableCollection<HeroineAsset>();
            AvailablePromptTemplates = new ObservableCollection<PromptTemplate>();
            StillDefinitions = new ObservableCollection<StillDefinition>();
            FilteredStillDefinitions = new ObservableCollection<StillDefinition>();
            FilteredConversationEntries = new ObservableCollection<ConversationEntry>();
            ConversationCategorySuggestions = new ObservableCollection<string>();
            ConversationCategoryFilters = new ObservableCollection<string>();
            ConversationImageFilters = new ObservableCollection<string>
            {
                "All",
                "画像あり",
                "画像なし"
            };
            ConversationLocationSuggestions = new ObservableCollection<string>(ConversationValueCatalog.Locations);
            ConversationActionSuggestions = new ObservableCollection<string>(ConversationValueCatalog.Actions);
            ConversationWeatherSuggestions = new ObservableCollection<string>(new[] { string.Empty }.Concat(ConversationValueCatalog.Weather));
            ConversationSeasonSuggestions = new ObservableCollection<string>(new[] { string.Empty }.Concat(ConversationValueCatalog.Seasons));
            ConversationTimeOfDaySuggestions = new ObservableCollection<string>(new[] { string.Empty }.Concat(ConversationValueCatalog.TimeOfDay));
            ConversationCostumeSuggestions = new ObservableCollection<string>(new[] { string.Empty }.Concat(ConversationValueCatalog.Costumes));
            ConversationExpressionSuggestions = new ObservableCollection<string>(ConversationValueCatalog.Expressions);
            ExpressionDefinitions = new ObservableCollection<ExpressionDefinition>();
            CostumeDefinitions = new ObservableCollection<CostumeDefinition>();
            LayerAssetDefinitions = new ObservableCollection<LayerAssetDefinition>();
            LayerKindOptions = new ObservableCollection<string>
            {
                "Background",
                "BackAccessory",
                "BackHair",
                "CostumeBody",
                "HeadExpression",
                "FrontAccessory",
                "FrontArm",
                "Effect",
                "BaseBody",
                "Costume",
                "Expression",
                "Accessory"
            };
            ExpressionIdOptions = new ObservableCollection<string>();
            CostumeIdOptions = new ObservableCollection<string>();
            BattleResultTypeOptions = new ObservableCollection<string>(new[]
            {
                "SoloVictory", "DuoVictory", "SoloDefeat", "DuoDefeat", "SoloEscape", "DuoEscape"
            });
            BattlePanelResultTypeOptions = new ObservableCollection<string>(new[]
            {
                "Victory", "Defeat", "Escape", "Default"
            });
            BattleSpeakerTypeOptions = new ObservableCollection<string>(new[]
            {
                "Heroine", "System", "Schedule", "Outfit", "Player"
            });
            BattleVisualModeOptions = new ObservableCollection<string>(new[]
            {
                "Auto", "StillOnly", "StillWithPortrait", "PortraitOnly"
            });
            EndingVisualModeOptions = new ObservableCollection<string>(new[]
            {
                "Auto", "StillOnly", "StillWithPortrait", "PortraitOnly"
            });
            BattleStillIdOptions = new ObservableCollection<string>();
            LayerPreviewBaseBodyOptions = new ObservableCollection<string>();
            LayerPreviewCostumeOptions = new ObservableCollection<string>();
            LayerPreviewExpressionOptions = new ObservableCollection<string>();
            LayerPreviewItems = new ObservableCollection<LayerPreviewItem>();
            ProfilePreviewItems = new ObservableCollection<LayerPreviewItem>();
            OutfitLayerAssetOptions = new ObservableCollection<string>();
            OutfitCompositionPreviewItems = new ObservableCollection<LayerPreviewItem>();
            HeadPartWorkspaceItems = new ObservableCollection<HeadPartWorkspaceItem>
            {
                new HeadPartWorkspaceItem
                {
                    DisplayName = "後ろ髪",
                    Description = "体や顔より後ろに表示する髪パーツ",
                    AssetId = "Head_BackHair",
                    LayerKind = "BackHair",
                    DrawOrder = 30
                },
                new HeadPartWorkspaceItem
                {
                    DisplayName = "顔・表情",
                    Description = "顔、表情、頭の中心部分",
                    AssetId = "HeadExpression_Neutral",
                    LayerKind = "HeadExpression",
                    ExpressionId = "Neutral",
                    DrawOrder = 50
                }
            };
            HeadPartPreviewItems = new ObservableCollection<LayerPreviewItem>();
            SelectedHeadPartWorkspaceItem = HeadPartWorkspaceItems.First();
            AssetStatusFilters = new ObservableCollection<string>
            {
                "All",
                AssetStatus.Accepted.ToString(),
                AssetStatus.Pending.ToString(),
                AssetStatus.Rejected.ToString()
            };
            StillUsageFilters = new ObservableCollection<string>
            {
                "All",
                AssetUsage.Sprites.ToString(),
                AssetUsage.Event.ToString(),
                AssetUsage.Actions.ToString(),
                AssetUsage.Ending.ToString(),
                AssetUsage.Battle.ToString(),
                AssetUsage.Training.ToString()
            };
            AssetUsages = new ObservableCollection<AssetUsage>
            {
                AssetUsage.Sprites,
                AssetUsage.Event,
                AssetUsage.Actions,
                AssetUsage.Ending,
                AssetUsage.Battle,
                AssetUsage.Training
            };
            AssetStatuses = new ObservableCollection<AssetStatus>
            {
                AssetStatus.Accepted,
                AssetStatus.Pending,
                AssetStatus.Rejected
            };
            StillStatuses = new ObservableCollection<StillStatus>
            {
                StillStatus.NotGenerated,
                StillStatus.Generating,
                StillStatus.Accepted,
                StillStatus.NeedsFix,
                StillStatus.NotNeeded
            };
            ConversationDataKinds = new ObservableCollection<ConversationDataKind>
            {
                ConversationDataKind.Conversations,
                ConversationDataKind.GameEvents,
                ConversationDataKind.ScheduledEvents,
                ConversationDataKind.ActionReactions,
                ConversationDataKind.Endings
            };
            MenuActionColumnOptions = new ObservableCollection<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(0, "自動"),
                new KeyValuePair<int, string>(1, "左"),
                new KeyValuePair<int, string>(2, "中央"),
                new KeyValuePair<int, string>(3, "右")
            };
            MenuActionExecutionTypes = new ObservableCollection<string>
            {
                "SimpleAction",
                "OpenConversationGenres",
                "OpenOutfitPanel",
                "OpenOutfitReactionPanel",
                "OpenSchedulePanel",
                "OpenStatusDetailPanel",
                "OpenStillGalleryPanel",
                "OpenMessageLogPanel",
                "OpenDebugBattlePanel",
                "OpenTrainingPanel",
                "OpenSkillPanel"
            };
            MenuActionWarnings = new ObservableCollection<string>();
            heroineIdInput = "TestHeroine";
            displayNameInput = "テストヒロイン";
            enemyIdInput = "ForestSlime";
            enemyDisplayNameInput = "森スライム";
            enemyTypeInput = "Slime";
            enemyAssetIdInput = "Enemy_ForestSlime_Idle";
            enemyImageSourcePathInput = string.Empty;
            enemyAssetPromptInput = GetDefaultEnemyAssetPrompt();
            selectedEnemyAssetStatus = AssetStatus.Accepted;
            selectedEnemyAssetImagePath = string.Empty;
            selectedEnemyAssetImageMessage = "敵画像を選択してください。";
            playerProfile = new PlayerProfile();
            selectedPlayerAsset = null;
            playerAssetIdInput = "Battle_Player_Idle";
            playerImageSourcePathInput = string.Empty;
            playerAssetPromptInput = GetDefaultPlayerAssetPrompt();
            selectedPlayerAssetStatus = AssetStatus.Accepted;
            selectedPlayerAssetImagePath = string.Empty;
            selectedPlayerAssetImageMessage = "プレイヤー画像を選択してください。";
            assetIdInput = "Heroine_Normal";
            imageSourcePathInput = string.Empty;
            selectedAssetUsage = AssetUsage.Sprites;
            selectedAssetStatus = AssetStatus.Accepted;
            selectedAssetStatusFilter = "All";
            selectedPromptTemplateUsage = AssetUsage.Sprites;
            selectedStillUsageFilter = "All";
            selectedConversationDataKind = ConversationDataKind.Conversations;
            selectedConversationEntry = null;
            selectedConversationLine = null;
            selectedConversationChoice = null;
            conversationSearchText = string.Empty;
            gameEventTestLocationId = string.Empty;
            gameEventTestAffection = string.Empty;
            gameEventTestWeather = string.Empty;
            gameEventTestSeason = string.Empty;
            gameEventTestTimeOfDay = string.Empty;
            gameEventTestActionId = string.Empty;
            gameEventTestCostumeId = string.Empty;
            gameEventTestItemId = string.Empty;
            gameEventTestFlagIdsText = string.Empty;
            selectedConversationCategoryFilter = "All";
            selectedConversationImageFilter = "All";
            showOnlyConversationWarnings = false;
            selectedConversationCategorySuggestion = string.Empty;
            selectedConversationLocationSuggestion = string.Empty;
            selectedConversationActionSuggestion = string.Empty;
            selectedConversationWeatherSuggestion = string.Empty;
            selectedConversationSeasonSuggestion = string.Empty;
            selectedConversationTimeOfDaySuggestion = string.Empty;
            selectedConversationCostumeSuggestion = string.Empty;
            selectedConversationExpressionSuggestion = "Neutral";
            selectedConversationImageAsset = null;
            selectedExpressionDefinition = null;
            selectedCostumeDefinition = null;
            selectedLayerAssetDefinition = null;
            definitionCatalogValidationMessage = string.Empty;
            selectedLayerPreviewBaseBodyId = string.Empty;
            selectedLayerPreviewCostumeId = string.Empty;
            selectedLayerPreviewExpressionId = string.Empty;
            layerPreviewMessage = "キャラクターとレイヤーを選択してください。";
            lastExportReport = new ExportReport();
            lastEnemyExportReport = new EnemyExportReport();
            lastPlayerExportReport = new PlayerExportReport();
            selectedAssetImagePath = string.Empty;
            selectedAssetImageMessage = "画像を選択してください。";
            selectedStillAssetStatusText = "Asset: 未選択";
            selectedStillImageStatusText = "画像: 未選択";
            selectedStillPromptStatusText = "Prompt: 未選択";
            selectedStillImagePath = string.Empty;
            selectedStillImageMessage = "スチルを選択してください。";
            comfySettings = new ComfySettings();
            comfySettingsSummary = string.Empty;
            comfyWorkflowTemplateEditorText = string.Empty;
            currentComfyWorkflowPreview = string.Empty;
            currentComfyPromptId = string.Empty;
            currentComfyResultSummary = string.Empty;
            currentComfyPreviewImagePath = string.Empty;
            currentComfyPreviewImageMessage = "Comfy 生成画像は未取得です。";
            currentComfyOutputImage = null;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            currentComfyProgressSummary = string.Empty;
            comfyPollingCancellation = null;
            hasComfyInterruptRequested = false;
            isComfySubmitting = false;
            isComfyCheckingResult = false;
            isComfyFetchingImage = false;
            isComfyWaitingResult = false;
            isComfyInterrupting = false;
            statusMessage = string.Empty;
            lastBattleMessageImportReport = "Unityから戦闘メッセージを読み込むと、ここに差分が表示されます。";
            selectedMainTabIndex = 0;
            showOnlyIncompleteProductionStatus = false;
            unityProjectPath = AudioLibraryService.LoadUnityProjectPath();
            audioLibrarySearchText = string.Empty;
            bgmSeAudioSearchText = string.Empty;
            voiceAudioSearchText = string.Empty;
            selectedAudioCategory = "すべて";
            showOnlySelectedHeroineAudio = true;
            showOnlyUnusedVoice = false;
            selectedVoiceUsage = "Training";
            voiceRegistrationId = string.Empty;
            selectedVoiceAssignmentTarget = "登録のみ";
            audioLibrarySummary = string.IsNullOrWhiteSpace(unityProjectPath)
                ? "Unityプロジェクトを選択してください。"
                : "「再走査」で音声導入状況を確認できます。";
            bgmSeAudioSummary = audioLibrarySummary;
            voiceAudioSummary = audioLibrarySummary;

            CreateCharacterCommand = new RelayCommand(CreateCharacter);
            SaveSelectedProfileCommand = new RelayCommand(SaveSelectedProfile, () => SelectedProfile != null);
            ImportHeroineProfileFromUnityCommand = new RelayCommand(
                ImportHeroineProfileFromUnity,
                () => SelectedProfile != null);
            OpenBattleMessagesTabCommand = new RelayCommand(() => SelectedMainTabIndex = 2);
            RefreshProductionStatusCommand = new RelayCommand(RefreshProductionStatus);
            OpenProductionStatusTargetCommand = new RelayCommand<object>(OpenProductionStatusTarget);
            BrowseUnityProjectCommand = new RelayCommand(BrowseUnityProject);
            RefreshAudioLibraryCommand = new RelayCommand(RefreshAudioLibrary);
            PlaySelectedAudioCommand = new RelayCommand(
                PlaySelectedAudio,
                () => SelectedAudioLibraryItem != null &&
                    SelectedAudioLibraryItem.IsAvailable);
            StopAudioPreviewCommand = new RelayCommand(StopAudioPreview);
            RegisterSelectedAudioCommand = new RelayCommand(
                RegisterSelectedAudio,
                CanRegisterSelectedAudio);
            OpenSelectedAudioFolderCommand = new RelayCommand(
                OpenSelectedAudioFolder,
                CanRegisterSelectedAudio);
            RegisterVoiceCommand = new RelayCommand(
                RegisterVoice,
                CanRegisterVoice);
            OpenVoiceFolderCommand = new RelayCommand(
                OpenVoiceFolder,
                () => SelectedProfile != null &&
                    !string.IsNullOrWhiteSpace(SelectedVoiceUsage));
            OpenSelectedVoiceFolderCommand = new RelayCommand(
                OpenSelectedVoiceFolder,
                () => SelectedAudioLibraryItem?.Category == "VOICE");
            AddOutfitMessageOverrideCommand = new RelayCommand(
                AddOutfitMessageOverride,
                () => SelectedProfile != null);
            RemoveOutfitMessageOverrideCommand = new RelayCommand(
                RemoveOutfitMessageOverride,
                () => SelectedProfile != null && SelectedOutfitMessageOverride != null);
            AddOutfitReactionMessageOverrideCommand = new RelayCommand(
                AddOutfitReactionMessageOverride,
                () => SelectedProfile != null);
            RemoveOutfitReactionMessageOverrideCommand = new RelayCommand(
                RemoveOutfitReactionMessageOverride,
                () => SelectedProfile != null && SelectedOutfitReactionMessageOverride != null);
            RefreshProfilesCommand = new RelayCommand(LoadProfiles);
            RefreshEnemiesCommand = new RelayCommand(LoadEnemies);
            CreateEnemyCommand = new RelayCommand(CreateEnemy);
            SaveSelectedEnemyCommand = new RelayCommand(SaveSelectedEnemy, () => SelectedEnemyProfile != null);
            BrowseEnemyImageCommand = new RelayCommand(BrowseEnemyImage);
            AddEnemyImageAssetCommand = new RelayCommand(AddEnemyImageAsset, () => SelectedEnemyProfile != null);
            UnregisterEnemyImageAssetCommand = new RelayCommand(
                UnregisterEnemyImageAsset,
                () => SelectedEnemyProfile != null && SelectedEnemyAsset != null);
            ExportSelectedEnemyCommand = new RelayCommand(ExportSelectedEnemy, () => SelectedEnemyProfile != null);
            AddEnemyStandardAssetDataCommand = new RelayCommand(
                AddEnemyStandardAssetData,
                () => SelectedEnemyProfile != null);
            BuildEnemyComfyWorkflowPreviewCommand = new RelayCommand(
                BuildEnemyComfyWorkflowPreview,
                () => SelectedEnemyProfile != null);
            SubmitEnemyComfyPromptCommand = new RelayCommand(
                SubmitEnemyComfyPrompt,
                () => SelectedEnemyProfile != null && !IsComfySubmitting && !IsComfyInterrupting && !IsComfyWaitingResult);
            AdoptEnemyComfyImageCommand = new RelayCommand(
                AdoptEnemyComfyImage,
                () => SelectedEnemyProfile != null &&
                    !IsComfyInterrupting &&
                    !IsComfyWaitingResult &&
                    !IsComfyFetchingImage &&
                    !string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) &&
                    File.Exists(CurrentComfyPreviewImagePath));
            ReloadPlayerProfileCommand = new RelayCommand(LoadPlayerProfile);
            SavePlayerProfileCommand = new RelayCommand(SavePlayerProfile, () => PlayerProfile != null);
            BrowsePlayerImageCommand = new RelayCommand(BrowsePlayerImage);
            AddPlayerImageAssetCommand = new RelayCommand(AddPlayerImageAsset, () => PlayerProfile != null);
            UnregisterPlayerImageAssetCommand = new RelayCommand(
                UnregisterPlayerImageAsset,
                () => PlayerProfile != null && SelectedPlayerAsset != null);
            ExportPlayerCommand = new RelayCommand(ExportPlayer, () => PlayerProfile != null);
            AddPlayerStandardAssetDataCommand = new RelayCommand(
                AddPlayerStandardAssetData,
                () => PlayerProfile != null);
            BuildPlayerComfyWorkflowPreviewCommand = new RelayCommand(
                BuildPlayerComfyWorkflowPreview,
                () => PlayerProfile != null);
            SubmitPlayerComfyPromptCommand = new RelayCommand(
                SubmitPlayerComfyPrompt,
                () => PlayerProfile != null && !IsComfySubmitting && !IsComfyInterrupting && !IsComfyWaitingResult);
            AdoptPlayerComfyImageCommand = new RelayCommand(
                AdoptPlayerComfyImage,
                () => PlayerProfile != null &&
                    !IsComfyInterrupting &&
                    !IsComfyWaitingResult &&
                    !IsComfyFetchingImage &&
                    !string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) &&
                    File.Exists(CurrentComfyPreviewImagePath));
            BrowseImageCommand = new RelayCommand(BrowseImage);
            AddImageAssetCommand = new RelayCommand(AddImageAsset, () => SelectedProfile != null);
            UnregisterImageAssetCommand = new RelayCommand(
                UnregisterImageAsset,
                () => SelectedProfile != null && SelectedAsset != null);
            SaveImageAssetsCommand = new RelayCommand(SaveImageAssets, () => SelectedProfile != null);
            AddHeroineBattleStandardAssetDataCommand = new RelayCommand(
                AddHeroineBattleStandardAssetData,
                () => SelectedProfile != null);
            AddTrainingStandardAssetDataCommand = new RelayCommand(
                AddTrainingStandardAssetData,
                () => SelectedProfile != null);
            AddTrainingImageEntryCommand = new RelayCommand(
                AddTrainingImageEntry,
                () => SelectedProfile != null);
            RemoveTrainingImageEntryCommand = new RelayCommand(
                RemoveTrainingImageEntry,
                () => SelectedProfile != null && SelectedTrainingImageEntry != null);
            AdoptTrainingExternalImageCommand = new RelayCommand(
                AdoptTrainingExternalImage,
                () => SelectedProfile != null &&
                    SelectedTrainingAsset != null &&
                    !string.IsNullOrWhiteSpace(ImageSourcePathInput) &&
                    File.Exists(ImageSourcePathInput));
            AddTrainingDialogueMessageCommand = new RelayCommand(
                AddTrainingDialogueMessage,
                () => SelectedTrainingDialogueEntry != null);
            RemoveTrainingDialogueMessageCommand = new RelayCommand(
                RemoveTrainingDialogueMessage,
                () => SelectedTrainingDialogueEntry != null && SelectedTrainingDialogueMessage != null);
            ImportTrainingDialoguesFromUnityCommand = new RelayCommand(
                ImportTrainingDialoguesFromUnity,
                () => SelectedProfile != null);
            ImportTrainingCatalogFromUnityCommand = new RelayCommand(
                ImportTrainingCatalogFromUnity,
                () => SelectedProfile != null);
            SavePromptRecordCommand = new RelayCommand(
                SavePromptRecord,
                () => SelectedProfile != null && SelectedAsset != null && CurrentPromptRecord != null);
            ApplyPromptTemplateCommand = new RelayCommand(
                ApplyPromptTemplate,
                () => SelectedProfile != null && SelectedPromptTemplate != null && CurrentPromptRecord != null);
            ApplyStillPromptCommand = new RelayCommand(
                ApplyStillPrompt,
                () => SelectedProfile != null && SelectedStillDefinition != null);
            PrepareImageRegistrationForStillCommand = new RelayCommand(
                PrepareImageRegistrationForStill,
                () => SelectedProfile != null && SelectedStillDefinition != null);
            ExportSelectedProfileCommand = new RelayCommand(ExportSelectedProfile, () => SelectedProfile != null);
            OpenExportDirectoryCommand = new RelayCommand(OpenExportDirectory);
            ReloadComfySettingsCommand = new RelayCommand(ReloadComfySettings);
            LoadComfyWorkflowTemplateCommand = new RelayCommand(LoadComfyWorkflowTemplate);
            SaveComfyWorkflowTemplateCommand = new RelayCommand(
                SaveComfyWorkflowTemplate,
                () => !string.IsNullOrWhiteSpace(ComfyWorkflowTemplateEditorText));
            BuildComfyWorkflowPreviewCommand = new RelayCommand(
                BuildComfyWorkflowPreview,
                () => CurrentPromptRecord != null);
            SubmitSelectedAssetComfyPromptCommand = new RelayCommand(
                SubmitSelectedAssetComfyPrompt,
                () => SelectedProfile != null &&
                    SelectedAsset != null &&
                    CurrentPromptRecord != null &&
                    !IsComfySubmitting &&
                    !IsComfyInterrupting &&
                    !IsComfyWaitingResult);
            BuildStillComfyWorkflowPreviewCommand = new RelayCommand(
                BuildStillComfyWorkflowPreview,
                () => SelectedProfile != null && SelectedStillDefinition != null);
            SubmitStillComfyPromptCommand = new RelayCommand(
                SubmitStillComfyPrompt,
                () => SelectedProfile != null && SelectedStillDefinition != null && !IsComfySubmitting && !IsComfyInterrupting && !IsComfyWaitingResult);
            CancelStillComfyPollingCommand = new RelayCommand(
                CancelStillComfyPolling,
                () => IsComfyWaitingResult && !IsComfyInterrupting);
            InterruptComfyGenerationCommand = new RelayCommand(
                InterruptComfyGeneration,
                () => !IsComfyInterrupting &&
                    !hasComfyInterruptRequested &&
                    (IsComfyWaitingResult || (!string.IsNullOrWhiteSpace(CurrentComfyPromptId) && currentComfyOutputImage == null)));
            CheckStillComfyResultCommand = new RelayCommand(
                CheckStillComfyResult,
                () => !string.IsNullOrWhiteSpace(CurrentComfyPromptId) && !IsComfySubmitting && !IsComfyInterrupting && !IsComfyWaitingResult && !IsComfyCheckingResult);
            FetchStillComfyImageCommand = new RelayCommand(
                FetchStillComfyImage,
                () => currentComfyOutputImage != null && !IsComfySubmitting && !IsComfyInterrupting && !IsComfyWaitingResult && !IsComfyCheckingResult && !IsComfyFetchingImage);
            AdoptStillComfyImageCommand = new RelayCommand(
                AdoptStillComfyImage,
                () => SelectedProfile != null &&
                    SelectedStillDefinition != null &&
                    !IsComfyInterrupting &&
                    !IsComfyWaitingResult &&
                    !IsComfyFetchingImage &&
                    !string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) &&
                    File.Exists(CurrentComfyPreviewImagePath));
            AdoptSelectedAssetComfyImageCommand = new RelayCommand(
                AdoptSelectedAssetComfyImage,
                () => SelectedProfile != null &&
                    SelectedAsset != null &&
                    !IsComfyInterrupting &&
                    !IsComfyWaitingResult &&
                    !IsComfyFetchingImage &&
                    !string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) &&
                    File.Exists(CurrentComfyPreviewImagePath));
            AddConversationEntryCommand = new RelayCommand(
                AddConversationEntry,
                () => SelectedProfile != null);
            RemoveConversationEntryCommand = new RelayCommand(
                RemoveConversationEntry,
                () => SelectedProfile != null && SelectedConversationEntry != null);
            AddConversationLineCommand = new RelayCommand(
                AddConversationLine,
                () => SelectedConversationEntry != null);
            RemoveConversationLineCommand = new RelayCommand(
                RemoveConversationLine,
                () => SelectedConversationEntry != null && SelectedConversationLine != null);
            AddConversationChoiceCommand = new RelayCommand(
                AddConversationChoice,
                () => SelectedConversationEntry != null);
            RemoveConversationChoiceCommand = new RelayCommand(
                RemoveConversationChoice,
                () => SelectedConversationEntry != null && SelectedConversationChoice != null);
            SaveConversationDataCommand = new RelayCommand(
                SaveConversationData,
                () => SelectedProfile != null);
            PrepareStandardMenuActionsCommand = new RelayCommand(
                PrepareStandardMenuActions,
                () => SelectedProfile != null);
            ApplyStandardMenuLayoutCommand = new RelayCommand(
                ApplyStandardMenuLayout,
                () => SelectedProfile != null);
            AddMenuActionCommand = new RelayCommand(
                AddMenuAction,
                () => SelectedProfile != null);
            RemoveMenuActionCommand = new RelayCommand(
                RemoveMenuAction,
                () => SelectedProfile != null && SelectedMenuAction != null);
            ValidateMenuActionsCommand = new RelayCommand(
                ValidateMenuActions,
                () => SelectedProfile != null);
            ImportActionsFromUnityCommand = new RelayCommand(
                ImportActionsFromUnity,
                () => SelectedProfile != null);
            ImportConversationsFromUnityCommand = new RelayCommand(
                ImportConversationsFromUnity,
                () => SelectedProfile != null);
            ImportGameEventsFromUnityCommand = new RelayCommand(
                ImportGameEventsFromUnity,
                () => SelectedProfile != null);
            ImportScheduledEventsFromUnityCommand = new RelayCommand(
                ImportScheduledEventsFromUnity,
                () => SelectedProfile != null);
            ImportEndingsFromUnityCommand = new RelayCommand(
                ImportEndingsFromUnity,
                () => SelectedProfile != null);
            ApplyConversationCategorySuggestionCommand = new RelayCommand(
                ApplyConversationCategorySuggestion,
                () => SelectedConversationEntry != null && !string.IsNullOrWhiteSpace(SelectedConversationCategorySuggestion));
            ApplyConversationEventTemplateCommand = new RelayCommand(
                ApplyConversationEventTemplate,
                () => SelectedProfile != null
                    && SelectedConversationEntry != null
                    && (SelectedConversationDataKind == ConversationDataKind.GameEvents
                        || SelectedConversationDataKind == ConversationDataKind.ScheduledEvents));
            ApplyConversationConditionSuggestionsCommand = new RelayCommand(
                ApplyConversationConditionSuggestions,
                () => SelectedConversationEntry != null);
            ApplyConversationExpressionSuggestionCommand = new RelayCommand(
                ApplyConversationExpressionSuggestion,
                () => SelectedConversationLine != null && !string.IsNullOrWhiteSpace(SelectedConversationExpressionSuggestion));
            AddConversationImageAssetCommand = new RelayCommand(
                AddConversationImageAsset,
                () => SelectedConversationEntry != null && SelectedConversationImageAsset != null);
            GenerateConversationIdCommand = new RelayCommand(
                GenerateConversationId,
                () => SelectedProfile != null && SelectedConversationEntry != null);
            AddExpressionDefinitionCommand = new RelayCommand(AddExpressionDefinition);
            RemoveExpressionDefinitionCommand = new RelayCommand(
                RemoveExpressionDefinition,
                () => SelectedExpressionDefinition != null);
            AddCostumeDefinitionCommand = new RelayCommand(AddCostumeDefinition);
            RemoveCostumeDefinitionCommand = new RelayCommand(
                RemoveCostumeDefinition,
                () => SelectedCostumeDefinition != null);
            AddLayerAssetDefinitionCommand = new RelayCommand(AddLayerAssetDefinition);
            RemoveLayerAssetDefinitionCommand = new RelayCommand(
                RemoveLayerAssetDefinition,
                () => SelectedLayerAssetDefinition != null);
            SaveDefinitionCatalogCommand = new RelayCommand(SaveDefinitionCatalog);
            ReloadDefinitionCatalogCommand = new RelayCommand(ReloadDefinitionCatalog);
            RefreshLayerPreviewCommand = new RelayCommand(RefreshLayerPreview);
            MoveLayerPreviewBackwardCommand = new RelayCommand(
                () => MoveSelectedLayerPreview(-1),
                () => CanMoveSelectedLayerPreview(-1));
            MoveLayerPreviewForwardCommand = new RelayCommand(
                () => MoveSelectedLayerPreview(1),
                () => CanMoveSelectedLayerPreview(1));
            ImportLayeredSpritesFromUnityCommand = new RelayCommand(
                ImportLayeredSpritesFromUnity,
                () => SelectedProfile != null);
            ApplyOutfitCompositionCommand = new RelayCommand(
                ApplyOutfitComposition,
                () => SelectedProfile != null && SelectedCostumeDefinition != null);
            AddOutfitAccessoryAssetTemplatesCommand = new RelayCommand(
                AddOutfitAccessoryAssetTemplates,
                () => SelectedProfile != null && SelectedCostumeDefinition != null);
            BrowseHeadPartImageCommand = new RelayCommand(
                BrowseHeadPartImage,
                () => SelectedProfile != null && SelectedHeadPartWorkspaceItem != null);
            TestHeadPartPreviewCommand = new RelayCommand(
                TestHeadPartPreview,
                () => SelectedProfile != null &&
                    SelectedHeadPartWorkspaceItem != null &&
                    !string.IsNullOrWhiteSpace(SelectedHeadPartWorkspaceItem.SourceImagePath) &&
                    File.Exists(SelectedHeadPartWorkspaceItem.SourceImagePath));
            AdoptHeadPartImageCommand = new RelayCommand(
                AdoptHeadPartImage,
                () => SelectedProfile != null &&
                    SelectedHeadPartWorkspaceItem != null &&
                    !string.IsNullOrWhiteSpace(SelectedHeadPartWorkspaceItem.SourceImagePath) &&
                    File.Exists(SelectedHeadPartWorkspaceItem.SourceImagePath));

            ReloadComfySettings();
            LoadDefinitionCatalog();
            RefreshConversationCategorySuggestions();
            LoadStillDefinitions();
            LoadProfiles();
            LoadEnemies();
            LoadPlayerProfile();
            if (AudioLibraryService.IsUnityProjectPath(UnityProjectPath))
            {
                RefreshAudioLibrary();
            }
            StatusMessage = "キャラクター基本情報の保存準備ができています。";
        }

        private void BrowseUnityProject()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Unityプロジェクトの ProjectSettings/ProjectVersion.txt を選択",
                Filter = "Unity ProjectVersion|ProjectVersion.txt|すべてのファイル|*.*",
                CheckFileExists = true
            };
            if (!string.IsNullOrWhiteSpace(UnityProjectPath))
            {
                string settingsFolder = Path.Combine(UnityProjectPath, "ProjectSettings");
                if (Directory.Exists(settingsFolder)) dialog.InitialDirectory = settingsFolder;
            }
            if (dialog.ShowDialog() != true) return;

            DirectoryInfo projectSettings = Directory.GetParent(dialog.FileName);
            DirectoryInfo projectRoot = projectSettings?.Parent;
            UnityProjectPath = projectRoot?.FullName ?? string.Empty;
            RefreshAudioLibrary();
        }

        private void RefreshAudioLibrary()
        {
            try
            {
                AudioLibraryScanResult result =
                    audioLibraryService.Scan(UnityProjectPath, Profiles);
                AudioLibraryService.SaveUnityProjectPath(UnityProjectPath);
                allAudioLibraryItems.Clear();
                allAudioLibraryItems.AddRange(result.Items);
                AudioLibrarySummary = result.CreateSummary();
                RefreshAvailableVoiceIds();
                RefreshFilteredAudioLibrary();
                RefreshProductionStatus();
                StatusMessage = "Unity音声ライブラリを再走査しました。";
            }
            catch (Exception ex)
            {
                allAudioLibraryItems.Clear();
                AudioLibraryItems.Clear();
                BgmSeAudioItems.Clear();
                VoiceAudioItems.Clear();
                AvailableVoiceIds.Clear();
                AudioLibrarySummary = "走査できません: " + ex.Message;
                BgmSeAudioSummary = AudioLibrarySummary;
                VoiceAudioSummary = AudioLibrarySummary;
                StatusMessage = AudioLibrarySummary;
            }
        }

        private void RefreshFilteredAudioLibrary()
        {
            if (AudioLibraryItems == null) return;
            AudioLibraryItems.Clear();
            BgmSeAudioItems.Clear();
            VoiceAudioItems.Clear();
            string search = (AudioLibrarySearchText ?? string.Empty).Trim();
            string bgmSeSearch = (BgmSeAudioSearchText ?? string.Empty).Trim();
            string voiceSearch = (VoiceAudioSearchText ?? string.Empty).Trim();
            string heroineId = SelectedProfile?.HeroineId ?? string.Empty;
            IEnumerable<AudioLibraryItem> searchedItems = allAudioLibraryItems
                .OrderBy(item => item.Category)
                .ThenBy(item => item.LogicalId);
            foreach (AudioLibraryItem item in searchedItems)
            {
                if ((item.Category == "BGM" || item.Category == "SE") &&
                    MatchesAudioSearch(item, bgmSeSearch))
                {
                    BgmSeAudioItems.Add(item);
                }
                if (item.Category == "VOICE" &&
                    MatchesAudioSearch(item, voiceSearch) &&
                    (!ShowOnlyUnusedVoice || item.IsUnusedVoice) &&
                    (!ShowOnlySelectedHeroineAudio ||
                     string.Equals(item.HeroineId, heroineId, StringComparison.OrdinalIgnoreCase)))
                {
                    VoiceAudioItems.Add(item);
                }
                if (MatchesAudioSearch(item, search) &&
                    MatchesAudioCategory(item, SelectedAudioCategory) &&
                    (!ShowOnlySelectedHeroineAudio ||
                     item.Category != "VOICE" ||
                     string.Equals(item.HeroineId, heroineId, StringComparison.OrdinalIgnoreCase)))
                {
                    AudioLibraryItems.Add(item);
                }
            }
            BgmSeAudioSummary =
                $"BGM・SE {BgmSeAudioItems.Count} 件 / 導入済み " +
                $"{BgmSeAudioItems.Count(item => item.IsAvailable)} / 未配置 " +
                $"{BgmSeAudioItems.Count(item => !item.IsAvailable)}";
            VoiceAudioSummary =
                $"VOICE {VoiceAudioItems.Count} 件 / 導入済み " +
                $"{VoiceAudioItems.Count(item => item.IsAvailable)} / 未配置 " +
                $"{VoiceAudioItems.Count(item => !item.IsAvailable)} / 参照数合計 " +
                $"{VoiceAudioItems.Sum(item => item.ReferenceCount)}";
        }

        private static bool MatchesAudioSearch(AudioLibraryItem item, string search)
        {
            return string.IsNullOrEmpty(search) ||
                (item.LogicalId ?? string.Empty).IndexOf(
                    search,
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                (item.FilePath ?? string.Empty).IndexOf(
                    search,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RefreshAvailableVoiceIds()
        {
            AvailableVoiceIds.Clear();
            AvailableVoiceIds.Add(string.Empty);
            string heroineId = SelectedProfile?.HeroineId ?? string.Empty;
            string prefix = heroineId + "/";
            foreach (string voiceId in allAudioLibraryItems
                .Where(item =>
                    item.Category == "VOICE" &&
                    string.Equals(item.HeroineId, heroineId, StringComparison.OrdinalIgnoreCase))
                .Select(item =>
                    item.LogicalId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        ? item.LogicalId.Substring(prefix.Length)
                        : item.LogicalId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(id => id))
            {
                AvailableVoiceIds.Add(voiceId);
            }
        }

        private void PlaySelectedAudio()
        {
            try
            {
                audioPreviewService.Play(SelectedAudioLibraryItem?.FilePath);
                StatusMessage = "音声を再生しています: " +
                    SelectedAudioLibraryItem?.LogicalId;
            }
            catch (Exception ex)
            {
                StatusMessage = "音声を再生できません: " + ex.Message;
            }
        }

        private void StopAudioPreview()
        {
            audioPreviewService.Stop();
            StatusMessage = "音声プレビューを停止しました。";
        }

        private void OnAudioPreviewFailed(string message)
        {
            if (Application.Current?.Dispatcher == null ||
                Application.Current.Dispatcher.CheckAccess())
            {
                StatusMessage = message;
                return;
            }
            Application.Current.Dispatcher.Invoke(() => StatusMessage = message);
        }

        private bool CanRegisterSelectedAudio()
        {
            return SelectedAudioLibraryItem != null &&
                (string.Equals(
                    SelectedAudioLibraryItem.Category,
                    "BGM",
                    StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                    SelectedAudioLibraryItem.Category,
                    "SE",
                    StringComparison.OrdinalIgnoreCase));
        }

        private void RegisterSelectedAudio()
        {
            AudioLibraryItem selected = SelectedAudioLibraryItem;
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = $"{selected.Category}「{selected.LogicalId}」へ登録する音声を選択",
                Filter = "音声ファイル|*.wav;*.mp3;*.ogg;*.aif;*.aiff|" +
                    "WAV|*.wav|MP3|*.mp3|Ogg Vorbis|*.ogg|AIFF|*.aif;*.aiff",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                AudioRegistrationPlan plan = audioLibraryService.CreateRegistrationPlan(
                    UnityProjectPath,
                    selected,
                    dialog.FileName);
                bool replaceExisting = false;
                if (plan.HasConflicts)
                {
                    string existingFiles = string.Join(
                        Environment.NewLine,
                        plan.ExistingPaths.Select(path => "・" + path));
                    MessageBoxResult result = MessageBox.Show(
                        $"同じIDの音声がすでに存在します。{Environment.NewLine}{Environment.NewLine}" +
                        existingFiles + Environment.NewLine + Environment.NewLine +
                        $"次のファイルへ置き換えますか？{Environment.NewLine}{plan.DestinationPath}" +
                        Environment.NewLine + Environment.NewLine +
                        "別拡張子の既存ファイルと、その.metaも削除されます。",
                        "音声ファイルの置き換え確認",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                    {
                        StatusMessage = "音声ファイルの登録をキャンセルしました。";
                        return;
                    }
                    replaceExisting = true;
                }

                string destination = audioLibraryService.RegisterAudio(plan, replaceExisting);
                string category = selected.Category;
                string logicalId = selected.LogicalId;
                RefreshAudioLibrary();
                SelectedAudioLibraryItem = AudioLibraryItems.FirstOrDefault(item =>
                    string.Equals(item.Category, category, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.LogicalId, logicalId, StringComparison.OrdinalIgnoreCase));
                StatusMessage = $"音声を登録しました: {destination}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "音声ファイルを登録できません",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusMessage = "音声ファイルを登録できません: " + ex.Message;
            }
        }

        private void OpenSelectedAudioFolder()
        {
            try
            {
                string folder = audioLibraryService.GetRegistrationDirectory(
                    UnityProjectPath,
                    SelectedAudioLibraryItem);
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
                StatusMessage = "音声の保存先を開きました: " + folder;
            }
            catch (Exception ex)
            {
                StatusMessage = "音声の保存先を開けません: " + ex.Message;
            }
        }

        private bool CanRegisterVoice()
        {
            if (SelectedProfile == null ||
                string.IsNullOrWhiteSpace(SelectedProfile.HeroineId) ||
                string.IsNullOrWhiteSpace(SelectedVoiceUsage) ||
                string.IsNullOrWhiteSpace(VoiceRegistrationId))
            {
                return false;
            }
            if (SelectedVoiceAssignmentTarget == "選択中の訓練セリフ")
                return SelectedTrainingDialogueMessage != null;
            if (SelectedVoiceAssignmentTarget == "選択中の戦闘後イベント")
                return SelectedBattleResultEvent != null;
            if (SelectedVoiceAssignmentTarget == "選択中の戦闘パネルメッセージ")
                return SelectedBattlePanelMessage != null;
            if (SelectedVoiceAssignmentTarget == "選択中の会話・イベント行")
                return SelectedConversationEntry != null && SelectedConversationLine != null;
            return true;
        }

        private void RegisterVoice()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = $"{SelectedProfile.HeroineId} のVOICEとして登録する音声を選択",
                Filter = "音声ファイル|*.wav;*.mp3;*.ogg;*.aif;*.aiff|" +
                    "WAV|*.wav|MP3|*.mp3|Ogg Vorbis|*.ogg|AIFF|*.aif;*.aiff",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                AudioRegistrationPlan plan = audioLibraryService.CreateVoiceRegistrationPlan(
                    UnityProjectPath,
                    SelectedProfile.HeroineId,
                    SelectedVoiceUsage,
                    VoiceRegistrationId,
                    dialog.FileName);
                bool replaceExisting = ConfirmAudioReplacement(plan);
                if (plan.HasConflicts && !replaceExisting)
                {
                    StatusMessage = "VOICEファイルの登録をキャンセルしました。";
                    return;
                }

                string destination = audioLibraryService.RegisterAudio(plan, replaceExisting);
                string heroinePrefix = SelectedProfile.HeroineId.Trim() + "/";
                string relativeVoiceId = plan.LogicalId.StartsWith(
                    heroinePrefix,
                    StringComparison.OrdinalIgnoreCase)
                    ? plan.LogicalId.Substring(heroinePrefix.Length)
                    : plan.LogicalId;
                AssignRegisteredVoiceId(relativeVoiceId);
                RefreshAudioLibrary();
                SelectedAudioLibraryItem = AudioLibraryItems.FirstOrDefault(item =>
                    item.Category == "VOICE" &&
                    string.Equals(
                        item.LogicalId,
                        plan.LogicalId,
                        StringComparison.OrdinalIgnoreCase));
                VoiceRegistrationId = string.Empty;
                StatusMessage = $"VOICEを登録しました: {destination}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "VOICEファイルを登録できません",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusMessage = "VOICEファイルを登録できません: " + ex.Message;
            }
        }

        private bool ConfirmAudioReplacement(AudioRegistrationPlan plan)
        {
            if (!plan.HasConflicts) return false;
            string existingFiles = string.Join(
                Environment.NewLine,
                plan.ExistingPaths.Select(path => "・" + path));
            return MessageBox.Show(
                $"同じIDの音声がすでに存在します。{Environment.NewLine}{Environment.NewLine}" +
                existingFiles + Environment.NewLine + Environment.NewLine +
                $"次のファイルへ置き換えますか？{Environment.NewLine}{plan.DestinationPath}" +
                Environment.NewLine + Environment.NewLine +
                "別拡張子の既存ファイルと、その.metaも削除されます。",
                "音声ファイルの置き換え確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        private void AssignRegisteredVoiceId(string voiceId)
        {
            if (SelectedVoiceAssignmentTarget == "選択中の訓練セリフ")
            {
                SelectedTrainingDialogueMessage.VoiceId = voiceId;
            }
            else if (SelectedVoiceAssignmentTarget == "選択中の戦闘後イベント")
            {
                SelectedBattleResultEvent.VoiceId = voiceId;
                OnPropertyChanged(nameof(SelectedBattleResultEvent));
            }
            else if (SelectedVoiceAssignmentTarget == "選択中の戦闘パネルメッセージ")
            {
                SelectedBattlePanelMessage.VoiceId = voiceId;
                OnPropertyChanged(nameof(SelectedBattlePanelMessage));
            }
            else if (SelectedVoiceAssignmentTarget == "選択中の会話・イベント行")
            {
                SelectedConversationLine.VoiceId = voiceId;
            }
            else
            {
                return;
            }
            characterProjectService.SaveProfile(SelectedProfile);
        }

        private void ApplyConversationVoiceUsageSuggestion()
        {
            SelectedVoiceUsage =
                SelectedConversationDataKind == ConversationDataKind.Conversations ||
                SelectedConversationDataKind == ConversationDataKind.ActionReactions
                    ? "Conversation"
                    : "Event";
        }

        private void OpenVoiceFolder()
        {
            try
            {
                string folder = audioLibraryService.GetVoiceRegistrationDirectory(
                    UnityProjectPath,
                    SelectedProfile.HeroineId,
                    SelectedVoiceUsage);
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
                StatusMessage = "VOICEの保存先を開きました: " + folder;
            }
            catch (Exception ex)
            {
                StatusMessage = "VOICEの保存先を開けません: " + ex.Message;
            }
        }

        private void OpenSelectedVoiceFolder()
        {
            try
            {
                AudioLibraryItem item = SelectedAudioLibraryItem;
                string path = item.IsAvailable ? item.FilePath : item.ExpectedPath;
                if (path.EndsWith(".*", StringComparison.Ordinal))
                {
                    path = path.Substring(0, path.Length - 2);
                }
                string folder = Path.GetDirectoryName(path);
                if (string.IsNullOrWhiteSpace(folder))
                {
                    throw new InvalidOperationException("選択したVOICEの保存先を確認できません。");
                }
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
                StatusMessage = "選択したVOICEの保存先を開きました: " + folder;
            }
            catch (Exception ex)
            {
                StatusMessage = "選択したVOICEの保存先を開けません: " + ex.Message;
            }
        }

        private static bool MatchesAudioCategory(
            AudioLibraryItem item,
            string category)
        {
            if (item == null) return false;
            if (string.IsNullOrWhiteSpace(category) || category == "すべて") return true;
            if (category == "未配置") return !item.IsAvailable;
            return string.Equals(item.Category, category, StringComparison.OrdinalIgnoreCase);
        }

        private void CreateCharacter()
        {
            try
            {
                bool overwriteExisting = characterProjectService.HasExistingCharacterData(HeroineIdInput);
                if (overwriteExisting)
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"HeroineId「{HeroineIdInput?.Trim()}」のキャラクターデータは既に存在します。\n\n" +
                        "新規作成を続けると、既存の基本データが新しい内容で上書きされます。\n" +
                        "この操作は元に戻せません。上書きしますか？",
                        "既存キャラクターの上書き確認",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No);

                    if (result != MessageBoxResult.Yes)
                    {
                        StatusMessage = "新規作成をキャンセルしました。既存データは変更されていません。";
                        return;
                    }
                }

                HeroineProfile profile = characterProjectService.CreateCharacter(
                    HeroineIdInput,
                    DisplayNameInput,
                    overwriteExisting);
                LoadProfiles();
                SelectProfile(profile.HeroineId);
                StatusMessage = overwriteExisting
                    ? $"{profile.HeroineId} を上書きして作成しました。"
                    : $"{profile.HeroineId} を作成しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"作成に失敗しました: {ex.Message}";
            }
        }

        private void ReloadComfySettings()
        {
            try
            {
                ComfySettings = comfySettingsService.Load();
                ComfySettingsSummary = BuildComfySettingsSummary(ComfySettings);
                StatusMessage = "ComfyUI 設定を読み込みました。";
                try
                {
                    LoadComfyWorkflowTemplateCore();
                    StatusMessage = "ComfyUI 設定と workflow template を読み込みました。";
                }
                catch (Exception templateEx)
                {
                    ComfyWorkflowTemplateEditorText = string.Empty;
                    StatusMessage = $"ComfyUI 設定を読み込みました。workflow template 読み込みに失敗しました: {templateEx.Message}";
                }
            }
            catch (Exception ex)
            {
                ComfySettings = new ComfySettings();
                ComfySettingsSummary = BuildComfySettingsSummary(ComfySettings);
                ComfyWorkflowTemplateEditorText = string.Empty;
                StatusMessage = $"ComfyUI 設定の読み込みに失敗しました: {ex.Message}";
            }
        }

        private static string BuildComfySettingsSummary(ComfySettings settings)
        {
            if (settings == null)
            {
                return string.Empty;
            }

            return $"Endpoint: {settings.EndpointUrl} / Workflow: {settings.WorkflowTemplatePath} / PositiveNode: {settings.PositivePromptNodeId} / NegativeNode: {settings.NegativePromptNodeId} / OutputNode: {settings.OutputNodeId}";
        }

        private void LoadComfyWorkflowTemplate()
        {
            try
            {
                LoadComfyWorkflowTemplateCore();
                StatusMessage = "ComfyUI workflow template を読み込みました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ComfyUI workflow template 読み込みに失敗しました: {ex.Message}";
            }
        }

        private void LoadComfyWorkflowTemplateCore()
        {
            ComfyWorkflowTemplateEditorText = comfyWorkflowService.LoadWorkflowTemplate(ComfySettings);
        }

        private void SaveComfyWorkflowTemplate()
        {
            try
            {
                comfyWorkflowService.SaveWorkflowTemplate(ComfySettings, ComfyWorkflowTemplateEditorText);
                CurrentComfyWorkflowPreview = string.Empty;
                CurrentComfyPromptId = string.Empty;
                CurrentComfyResultSummary = string.Empty;
                currentComfySubmittedPromptRecord = null;
                currentComfyWorkflowJson = string.Empty;
                ClearComfyPreviewImage();
                StatusMessage = "ComfyUI workflow template を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ComfyUI workflow template 保存に失敗しました: {ex.Message}";
            }
        }

        private void BuildComfyWorkflowPreview()
        {
            if (CurrentPromptRecord == null)
            {
                return;
            }

            try
            {
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, CurrentPromptRecord);
                StatusMessage = "ComfyUI workflow preview を作成しました。";
            }
            catch (Exception ex)
            {
                CurrentComfyWorkflowPreview = string.Empty;
                StatusMessage = $"ComfyUI workflow preview 作成に失敗しました: {ex.Message}";
            }
        }

        private void BuildStillComfyWorkflowPreview()
        {
            if (SelectedProfile == null || SelectedStillDefinition == null)
            {
                return;
            }

            try
            {
                PromptRecord promptRecord = CreateStillPromptRecord();
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                StatusMessage = $"{SelectedStillDefinition.DisplayName} の ComfyUI workflow preview を作成しました。";
            }
            catch (Exception ex)
            {
                CurrentComfyWorkflowPreview = string.Empty;
                StatusMessage = $"スチル ComfyUI workflow preview 作成に失敗しました: {ex.Message}";
            }
        }

        private async void SubmitSelectedAssetComfyPrompt()
        {
            if (SelectedProfile == null || SelectedAsset == null || CurrentPromptRecord == null)
            {
                return;
            }

            RequestComfyPollingCancellation();
            hasComfyInterruptRequested = false;
            IsComfySubmitting = true;
            CurrentComfyPromptId = string.Empty;
            CurrentComfyResultSummary = string.Empty;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            ClearComfyPreviewImage();
            string queuedPromptId = string.Empty;
            string queuedClientId = string.Empty;
            string assetId = SelectedAsset.AssetId;
            try
            {
                // 訓練画像を含む選択中 Asset の prompt JSON をそのまま生成条件に使う。
                PromptRecord promptRecord = CurrentPromptRecord;
                promptRecordService.SavePromptRecord(SelectedProfile, SelectedAsset, promptRecord);
                string workflowJson = comfyWorkflowService.BuildWorkflowJson(ComfySettings, promptRecord);
                currentComfySubmittedPromptRecord = promptRecord;
                currentComfyWorkflowJson = workflowJson;
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                ComfyPromptQueueResult queueResult = await comfyClientService.QueuePromptWithClientAsync(ComfySettings, workflowJson);
                CurrentComfyPromptId = queueResult.PromptId;
                queuedPromptId = CurrentComfyPromptId;
                queuedClientId = queueResult.ClientId;
                StatusMessage = $"{assetId} を ComfyUI に送信しました。prompt_id: {CurrentComfyPromptId}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ComfyUI 送信に失敗しました: {ex.Message}";
            }
            finally
            {
                IsComfySubmitting = false;
            }

            if (!string.IsNullOrWhiteSpace(queuedPromptId))
            {
                await WaitForComfyResultAsync(queuedPromptId, queuedClientId, assetId);
            }
        }

        private async void SubmitStillComfyPrompt()
        {
            if (SelectedProfile == null || SelectedStillDefinition == null)
            {
                return;
            }

            RequestComfyPollingCancellation();
            hasComfyInterruptRequested = false;
            IsComfySubmitting = true;
            CurrentComfyPromptId = string.Empty;
            CurrentComfyResultSummary = string.Empty;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            ClearComfyPreviewImage();
            string queuedPromptId = string.Empty;
            string queuedClientId = string.Empty;
            string stillDisplayName = SelectedStillDefinition.DisplayName;
            try
            {
                PromptRecord promptRecord = CreateStillPromptRecord();
                string workflowJson = comfyWorkflowService.BuildWorkflowJson(ComfySettings, promptRecord);
                currentComfySubmittedPromptRecord = promptRecord;
                currentComfyWorkflowJson = workflowJson;
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                ComfyPromptQueueResult queueResult = await comfyClientService.QueuePromptWithClientAsync(ComfySettings, workflowJson);
                CurrentComfyPromptId = queueResult.PromptId;
                queuedPromptId = CurrentComfyPromptId;
                queuedClientId = queueResult.ClientId;
                StatusMessage = $"{stillDisplayName} を ComfyUI に送信しました。prompt_id: {CurrentComfyPromptId}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ComfyUI 送信に失敗しました: {ex.Message}";
            }
            finally
            {
                IsComfySubmitting = false;
            }

            if (!string.IsNullOrWhiteSpace(queuedPromptId))
            {
                await WaitForComfyResultAsync(queuedPromptId, queuedClientId, stillDisplayName);
            }
        }

        private async void CheckStillComfyResult()
        {
            if (string.IsNullOrWhiteSpace(CurrentComfyPromptId))
            {
                return;
            }

            IsComfyCheckingResult = true;
            try
            {
                System.Collections.Generic.IReadOnlyList<ComfyOutputImage> images =
                    await comfyClientService.GetOutputImagesAsync(ComfySettings, CurrentComfyPromptId);
                if (images.Count == 0)
                {
                    CurrentComfyResultSummary = "生成結果はまだ取得できません。生成中、または画像出力がありません。";
                    currentComfyOutputImage = null;
                    ClearComfyPreviewImage();
                    StatusMessage = "ComfyUI の生成結果はまだ取得できません。";
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }

                ApplyComfyOutputImages(images);
                StatusMessage = $"ComfyUI 生成結果を {images.Count} 件取得しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ComfyUI 生成結果取得に失敗しました: {ex.Message}";
            }
            finally
            {
                IsComfyCheckingResult = false;
            }
        }

        private void CancelStillComfyPolling()
        {
            if (!IsComfyWaitingResult)
            {
                return;
            }

            RequestComfyPollingCancellation();
            StatusMessage = "ComfyUI 生成結果の待機をキャンセルしました。ComfyUI 側の生成処理は停止していない場合があります。";
        }

        private async void InterruptComfyGeneration()
        {
            RequestComfyPollingCancellation();
            IsComfyInterrupting = true;
            try
            {
                await comfyClientService.InterruptAsync(ComfySettings);
                hasComfyInterruptRequested = true;
                CurrentComfyResultSummary = "ComfyUI 本体へ停止要求を送信しました。必要なら結果確認で出力有無を確認してください。";
                StatusMessage = "ComfyUI 本体へ停止要求を送信しました。";
            }
            catch (Exception ex)
            {
                CurrentComfyResultSummary = "ComfyUI 本体への停止要求に失敗しました。アプリ側の自動確認は停止しました。";
                StatusMessage = $"ComfyUI 停止要求に失敗しました: {ex.Message}";
            }
            finally
            {
                IsComfyInterrupting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async Task WaitForComfyResultAsync(string promptId, string clientId, string stillDisplayName)
        {
            RequestComfyPollingCancellation();
            CancellationTokenSource pollingCancellation = new CancellationTokenSource();
            comfyPollingCancellation = pollingCancellation;
            CancellationToken cancellationToken = pollingCancellation.Token;
            IsComfyWaitingResult = true;
            currentComfyProgressSummary = string.Empty;
            CurrentComfyResultSummary = "ComfyUI 生成中です。生成結果を自動確認しています。";
            StatusMessage = $"{stillDisplayName} の ComfyUI 生成結果を待機しています。";
            Task progressWatchTask = StartComfyProgressWatchAsync(promptId, clientId, cancellationToken);

            const int maxAttempts = 120;
            const int delayMilliseconds = 2000;
            try
            {
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    System.Collections.Generic.IReadOnlyList<ComfyOutputImage> images =
                        await comfyClientService.GetOutputImagesAsync(ComfySettings, promptId);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (images.Count > 0)
                    {
                        ApplyComfyOutputImages(images);
                        StatusMessage = $"ComfyUI 生成結果を {images.Count} 件取得しました。画像取得できます。";
                        return;
                    }

                    CurrentComfyResultSummary = await BuildComfyWaitingSummaryAsync(promptId, attempt, maxAttempts);
                    await Task.Delay(delayMilliseconds, cancellationToken);
                }

                CurrentComfyResultSummary = "ComfyUI 生成結果を時間内に取得できませんでした。必要なら結果確認を押してください。";
                StatusMessage = "ComfyUI 生成結果の自動確認がタイムアウトしました。";
            }
            catch (OperationCanceledException)
            {
                CurrentComfyResultSummary = "ComfyUI 生成結果の自動確認をキャンセルしました。必要なら結果確認を押してください。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ComfyUI 生成結果の自動確認に失敗しました: {ex.Message}";
            }
            finally
            {
                if (ReferenceEquals(comfyPollingCancellation, pollingCancellation))
                {
                    comfyPollingCancellation = null;
                    IsComfyWaitingResult = false;
                }

                pollingCancellation.Cancel();
                await ObserveComfyProgressWatchCompletionAsync(progressWatchTask);
                pollingCancellation.Dispose();
            }
        }

        private async Task<string> BuildComfyWaitingSummaryAsync(string promptId, int attempt, int maxAttempts)
        {
            string baseSummary = $"ComfyUI 生成中です。生成結果を自動確認しています。({attempt}/{maxAttempts})";
            if (!string.IsNullOrWhiteSpace(currentComfyProgressSummary))
            {
                baseSummary += Environment.NewLine + currentComfyProgressSummary;
            }

            try
            {
                ComfyQueueStatus queueStatus = await comfyClientService.GetQueueStatusAsync(ComfySettings, promptId);
                string targetStatus = "対象prompt: queue内に見つかりません";
                if (queueStatus.IsTargetRunning)
                {
                    targetStatus = "対象prompt: 実行中";
                }
                else if (queueStatus.TargetPendingIndex > 0)
                {
                    targetStatus = $"対象prompt: 待機中 {queueStatus.TargetPendingIndex} 番目";
                }

                return baseSummary +
                    Environment.NewLine +
                    $"Queue: 実行中 {queueStatus.RunningCount} 件 / 待機中 {queueStatus.PendingCount} 件 / {targetStatus}";
            }
            catch (Exception ex)
            {
                return baseSummary +
                    Environment.NewLine +
                    $"Queue 状態取得に失敗しました: {ex.Message}";
            }
        }

        private Task StartComfyProgressWatchAsync(string promptId, string clientId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                currentComfyProgressSummary = "WebSocket: client_id がないため詳細進捗は表示しません。";
                return Task.CompletedTask;
            }

            return WatchComfyProgressWithFallbackAsync(promptId, clientId, cancellationToken);
        }

        private async Task WatchComfyProgressWithFallbackAsync(string promptId, string clientId, CancellationToken cancellationToken)
        {
            try
            {
                await comfyClientService.WatchPromptProgressAsync(
                    ComfySettings,
                    promptId,
                    clientId,
                    update => UpdateComfyProgressSummary(update),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                UpdateComfyProgressSummaryText($"WebSocket: 詳細進捗を取得できません。/queue と /history で確認します。({ex.Message})");
            }
        }

        private void UpdateComfyProgressSummary(ComfyProgressUpdate update)
        {
            if (update == null || string.IsNullOrWhiteSpace(update.Summary))
            {
                return;
            }

            UpdateComfyProgressSummaryText(update.Summary);
        }

        private void UpdateComfyProgressSummaryText(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return;
            }

            void ApplyUpdate()
            {
                currentComfyProgressSummary = summary;
                if (IsComfyWaitingResult && CurrentComfyResultSummary.StartsWith("ComfyUI 生成中です。", StringComparison.Ordinal))
                {
                    CurrentComfyResultSummary = "ComfyUI 生成中です。生成結果を自動確認しています。" +
                        Environment.NewLine +
                        summary;
                }
            }

            if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke((Action)ApplyUpdate);
            }
            else
            {
                ApplyUpdate();
            }
        }

        private static async Task ObserveComfyProgressWatchCompletionAsync(Task progressWatchTask)
        {
            if (progressWatchTask == null)
            {
                return;
            }

            try
            {
                await progressWatchTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
            }
        }

        private void ApplyComfyOutputImages(System.Collections.Generic.IReadOnlyList<ComfyOutputImage> images)
        {
            currentComfyOutputImage = images[0];
            CurrentComfyResultSummary = string.Join(
                Environment.NewLine,
                images.Select(image => $"{image.DisplayPath} ({image.Type})"));
            CommandManager.InvalidateRequerySuggested();
        }

        private void RequestComfyPollingCancellation()
        {
            if (comfyPollingCancellation == null)
            {
                return;
            }

            comfyPollingCancellation.Cancel();
        }

        private async void FetchStillComfyImage()
        {
            if (currentComfyOutputImage == null)
            {
                return;
            }

            IsComfyFetchingImage = true;
            try
            {
                byte[] imageBytes = await comfyClientService.GetImageAsync(ComfySettings, currentComfyOutputImage);
                string tempImagePath = SaveComfyTempImage(CurrentComfyPromptId, currentComfyOutputImage, imageBytes);
                CurrentComfyPreviewImagePath = tempImagePath;
                CurrentComfyPreviewImageMessage = $"Comfy 生成画像: {currentComfyOutputImage.DisplayPath}";
                StatusMessage = $"ComfyUI 生成画像を一時保存しました: {tempImagePath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ComfyUI 生成画像取得に失敗しました: {ex.Message}";
            }
            finally
            {
                IsComfyFetchingImage = false;
            }
        }

        private string SaveComfyTempImage(string promptId, ComfyOutputImage image, byte[] imageBytes)
        {
            string tempDirectory = Path.Combine(characterProjectService.WorkspaceRoot, "Temp", "ComfyResults");
            Directory.CreateDirectory(tempDirectory);

            string fileName = SanitizeFileName($"{promptId}_{image.FileName}");
            string outputPath = Path.Combine(tempDirectory, fileName);
            File.WriteAllBytes(outputPath, imageBytes);
            return outputPath;
        }

        private static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            char[] chars = fileName.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray();
            return new string(chars);
        }

        private void ClearComfyPreviewImage()
        {
            currentComfyOutputImage = null;
            CurrentComfyPreviewImagePath = string.Empty;
            CurrentComfyPreviewImageMessage = "Comfy 生成画像は未取得です。";
            CommandManager.InvalidateRequerySuggested();
        }

        private void AdoptStillComfyImage()
        {
            if (SelectedProfile == null || SelectedStillDefinition == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) || !File.Exists(CurrentComfyPreviewImagePath))
            {
                StatusMessage = "採用できる Comfy 生成画像がありません。先に画像取得を行ってください。";
                return;
            }

            ImageSourcePathInput = CurrentComfyPreviewImagePath;
            AssetIdInput = SelectedStillDefinition.AssetId;
            SelectedAssetUsage = SelectedStillDefinition.Usage;
            SelectedAssetStatus = AssetStatus.Accepted;
            try
            {
                HeroineAsset asset = AddImageAssetCore();
                if (asset == null)
                {
                    return;
                }

                SaveComfyPromptRecord(asset);
                StatusMessage += " Comfy 生成条件を prompt 記録に保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Comfy 生成画像の採用に失敗しました: {ex.Message}";
            }
        }

        private void AdoptSelectedAssetComfyImage()
        {
            if (SelectedProfile == null || SelectedAsset == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) || !File.Exists(CurrentComfyPreviewImagePath))
            {
                StatusMessage = "採用できる Comfy 生成画像がありません。先に画像取得を行ってください。";
                return;
            }

            HeroineAsset targetAsset = SelectedAsset;
            ImageSourcePathInput = CurrentComfyPreviewImagePath;
            AssetIdInput = targetAsset.AssetId;
            SelectedAssetUsage = targetAsset.Usage;
            SelectedAssetStatus = AssetStatus.Accepted;
            try
            {
                HeroineAsset asset = AddImageAssetCore();
                if (asset == null)
                {
                    return;
                }

                SaveComfyPromptRecord(asset);
                StatusMessage += " Comfy 生成条件を prompt 記録に保存しました。";
                if (targetAsset.Usage == AssetUsage.Training)
                {
                    SelectNextPendingTrainingAsset(asset);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Comfy 生成画像の採用に失敗しました: {ex.Message}";
            }
        }

        private void SelectNextPendingTrainingAsset(HeroineAsset adoptedAsset)
        {
            RefreshTrainingAssets();
            if (TrainingAssets.Count == 0)
            {
                return;
            }

            int adoptedIndex = TrainingAssets.IndexOf(adoptedAsset);
            int startIndex = adoptedIndex >= 0 ? adoptedIndex : -1;
            for (int offset = 1; offset <= TrainingAssets.Count; offset++)
            {
                int index = (startIndex + offset) % TrainingAssets.Count;
                HeroineAsset candidate = TrainingAssets[index];
                if (candidate.Status != AssetStatus.Accepted)
                {
                    SelectedTrainingAsset = candidate;
                    CurrentComfyPromptId = string.Empty;
                    CurrentComfyResultSummary = string.Empty;
                    currentComfySubmittedPromptRecord = null;
                    currentComfyWorkflowJson = string.Empty;
                    ClearComfyPreviewImage();
                    StatusMessage += $" 次の未採用枠 {candidate.AssetId} を選択しました。";
                    return;
                }
            }
        }

        private void SaveComfyPromptRecord(HeroineAsset asset)
        {
            if (SelectedProfile == null || asset == null)
            {
                return;
            }

            PromptRecord record = promptRecordService.LoadOrCreatePromptRecord(SelectedProfile, asset);
            PromptRecord sourceRecord = currentComfySubmittedPromptRecord ?? CreateStillPromptRecord();

            record.PositivePrompt = sourceRecord.PositivePrompt;
            record.NegativePrompt = sourceRecord.NegativePrompt;
            record.ComfyPromptId = CurrentComfyPromptId ?? string.Empty;
            record.ComfyEndpointUrl = ComfySettings != null ? ComfySettings.EndpointUrl : string.Empty;
            record.ComfyWorkflowTemplatePath = ComfySettings != null ? ComfySettings.WorkflowTemplatePath : string.Empty;
            record.ComfyWorkflowJson = currentComfyWorkflowJson ?? string.Empty;

            if (currentComfyOutputImage != null)
            {
                record.ComfyOutputFileName = currentComfyOutputImage.FileName;
                record.ComfyOutputSubfolder = currentComfyOutputImage.Subfolder;
                record.ComfyOutputType = currentComfyOutputImage.Type;
            }

            ApplyComfyWorkflowSettings(record, currentComfyWorkflowJson);
            promptRecordService.SavePromptRecord(SelectedProfile, asset, record);
            characterProjectService.SaveProfile(SelectedProfile);
            CurrentPromptRecord = record;
            RefreshSelectedStillStatus();
        }

        private static void ApplyComfyWorkflowSettings(PromptRecord record, string workflowJson)
        {
            if (record == null || string.IsNullOrWhiteSpace(workflowJson))
            {
                return;
            }

            using JsonDocument document = JsonDocument.Parse(workflowJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (JsonProperty nodeProperty in document.RootElement.EnumerateObject())
            {
                JsonElement nodeElement = nodeProperty.Value;
                string classType = GetJsonString(nodeElement, "class_type");
                if (!nodeElement.TryGetProperty("inputs", out JsonElement inputsElement) ||
                    inputsElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                switch (classType)
                {
                    case "CheckpointLoaderSimple":
                        record.Model = GetJsonString(inputsElement, "ckpt_name");
                        break;
                    case "EmptyLatentImage":
                        record.ImageWidth = GetJsonInt(inputsElement, "width");
                        record.ImageHeight = GetJsonInt(inputsElement, "height");
                        break;
                    case "KSamplerAdvanced":
                        record.Seed = GetJsonLong(inputsElement, "noise_seed");
                        record.Steps = GetJsonInt(inputsElement, "steps");
                        record.CfgScale = GetJsonDouble(inputsElement, "cfg");
                        record.Sampler = GetJsonString(inputsElement, "sampler_name");
                        break;
                }
            }
        }

        private static string GetJsonString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement) &&
                propertyElement.ValueKind == JsonValueKind.String)
            {
                return propertyElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static int GetJsonInt(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement) &&
                propertyElement.ValueKind == JsonValueKind.Number &&
                propertyElement.TryGetInt32(out int value))
            {
                return value;
            }

            return 0;
        }

        private static long GetJsonLong(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement) &&
                propertyElement.ValueKind == JsonValueKind.Number &&
                propertyElement.TryGetInt64(out long value))
            {
                return value;
            }

            return 0;
        }

        private static double GetJsonDouble(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement) &&
                propertyElement.ValueKind == JsonValueKind.Number &&
                propertyElement.TryGetDouble(out double value))
            {
                return value;
            }

            return 0;
        }

        private PromptRecord CreateStillPromptRecord()
        {
            return new PromptRecord
            {
                PositivePrompt = BuildStillPositivePrompt(SelectedProfile, SelectedStillDefinition),
                NegativePrompt = BuildStillNegativePrompt()
            };
        }

        private string BuildStillNegativePrompt()
        {
            string commonPrompt = CurrentPromptRecord != null ? CurrentPromptRecord.NegativePrompt : string.Empty;
            string additionPrompt = SelectedStillDefinition != null
                ? SelectedStillDefinition.NegativePromptAddition
                : string.Empty;

            return MergePromptParts(commonPrompt, additionPrompt);
        }

        private static string MergePromptParts(params string[] promptParts)
        {
            return string.Join(
                ", ",
                promptParts
                    .Select(NormalizePromptPart)
                    .Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private void SelectedStillDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StillDefinition.SpecificPrompt))
            {
                OnPropertyChanged(nameof(StillPromptPreview));
                CurrentComfyWorkflowPreview = string.Empty;
                UpdateStillWorkItemFromDefinition();
            }
            else if (e.PropertyName == nameof(StillDefinition.NegativePromptAddition))
            {
                CurrentComfyWorkflowPreview = string.Empty;
                CurrentComfyPromptId = string.Empty;
                CurrentComfyResultSummary = string.Empty;
                currentComfySubmittedPromptRecord = null;
                currentComfyWorkflowJson = string.Empty;
                ClearComfyPreviewImage();
                UpdateStillWorkItemFromDefinition();
            }
            else if (e.PropertyName == nameof(StillDefinition.Status))
            {
                UpdateStillWorkItemFromDefinition();
            }
        }

        private void SelectedProfilePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HeroineProfile.AppearancePrompt) ||
                e.PropertyName == nameof(HeroineProfile.StillCommonPositivePrompt))
            {
                RefreshStillPromptAfterProfilePromptChanged();
            }
        }

        private void SelectedEnemyProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EnemyProfile.AppearancePrompt) ||
                e.PropertyName == nameof(EnemyProfile.BattleCommonPositivePrompt) ||
                e.PropertyName == nameof(EnemyProfile.NegativePrompt))
            {
                OnPropertyChanged(nameof(EnemyPromptPreview));
                ClearEnemyComfyWork();
            }
        }

        private void PlayerProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerProfile.AppearancePrompt) ||
                e.PropertyName == nameof(PlayerProfile.BattleCommonPositivePrompt) ||
                e.PropertyName == nameof(PlayerProfile.NegativePrompt))
            {
                OnPropertyChanged(nameof(PlayerPromptPreview));
                ClearPlayerComfyWork();
            }
        }

        private void RefreshStillPromptAfterProfilePromptChanged()
        {
            RequestComfyPollingCancellation();
            IsComfyWaitingResult = false;
            OnPropertyChanged(nameof(StillPromptPreview));
            CurrentComfyWorkflowPreview = string.Empty;
            CurrentComfyPromptId = string.Empty;
            CurrentComfyResultSummary = string.Empty;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            hasComfyInterruptRequested = false;
            ClearComfyPreviewImage();
        }

        private void CreateEnemy()
        {
            try
            {
                EnemyProfile profile = enemyProjectService.CreateEnemy(EnemyIdInput, EnemyDisplayNameInput, EnemyTypeInput);
                LoadEnemies();
                SelectEnemy(profile.EnemyId);
                StatusMessage = $"{profile.EnemyId} を作成しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵キャラ作成に失敗しました: {ex.Message}";
            }
        }

        private void SaveSelectedEnemy()
        {
            if (SelectedEnemyProfile == null)
            {
                return;
            }

            try
            {
                enemyProjectService.SaveProfile(SelectedEnemyProfile);
                StatusMessage = $"{SelectedEnemyProfile.EnemyId} を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵キャラ保存に失敗しました: {ex.Message}";
            }
        }

        private void ExportSelectedEnemy()
        {
            if (SelectedEnemyProfile == null)
            {
                return;
            }

            try
            {
                enemyProjectService.SaveProfile(SelectedEnemyProfile);
                LastEnemyExportReport = enemyExportService.ExportEnemy(SelectedEnemyProfile);
                StatusMessage = $"{SelectedEnemyProfile.EnemyId} を enemy export しました。画像 {LastEnemyExportReport.ExportedImageCount}/{LastEnemyExportReport.AcceptedAssetCount} 件、prompt {LastEnemyExportReport.ExportedPromptCount} 件、警告 {LastEnemyExportReport.Warnings.Count} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵キャラ export に失敗しました: {ex.Message}";
            }
        }

        private void AddEnemyStandardAssetData()
        {
            if (SelectedEnemyProfile == null)
            {
                return;
            }

            try
            {
                SelectedEnemyProfile.Assets ??= new ObservableCollection<EnemyAsset>();
                enemyProjectService.EnsureEnemyDirectories(SelectedEnemyProfile.EnemyId);

                int addedCount = 0;
                int updatedCount = 0;
                EnemyAsset firstAsset = null;
                foreach (EnemyAssetTemplate template in BuildEnemyStandardAssetTemplates(SelectedEnemyProfile))
                {
                    EnemyAsset asset = SelectedEnemyProfile.Assets
                        .FirstOrDefault(item => string.Equals(item.AssetId, template.AssetId, StringComparison.Ordinal));
                    if (asset == null)
                    {
                        asset = new EnemyAsset
                        {
                            AssetId = template.AssetId,
                            Usage = AssetUsage.Battle,
                            Status = AssetStatus.Pending,
                            PromptRecordPath = Path.Combine("Prompts", template.AssetId + ".prompt.json"),
                            Memo = template.Memo
                        };
                        SelectedEnemyProfile.Assets.Add(asset);
                        addedCount++;
                    }
                    else
                    {
                        asset.Usage = AssetUsage.Battle;
                        if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
                        {
                            asset.PromptRecordPath = Path.Combine("Prompts", template.AssetId + ".prompt.json");
                        }

                        if (string.IsNullOrWhiteSpace(asset.Memo))
                        {
                            asset.Memo = template.Memo;
                        }

                        updatedCount++;
                    }

                    SaveEnemyPromptRecordForTemplate(asset, template.Prompt);
                    firstAsset ??= asset;
                }

                enemyProjectService.SaveProfile(SelectedEnemyProfile);
                SelectedEnemyAsset = firstAsset ?? SelectedEnemyProfile.Assets.FirstOrDefault();
                StatusMessage = $"敵画像の標準候補を登録しました。追加 {addedCount} 件、更新 {updatedCount} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵画像の標準候補登録に失敗しました: {ex.Message}";
            }
        }

        private void BuildEnemyComfyWorkflowPreview()
        {
            if (SelectedEnemyProfile == null)
            {
                return;
            }

            try
            {
                PromptRecord promptRecord = CreateEnemyPromptRecord();
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                StatusMessage = $"{SelectedEnemyProfile.DisplayName} の敵画像 ComfyUI workflow preview を作成しました。";
            }
            catch (Exception ex)
            {
                CurrentComfyWorkflowPreview = string.Empty;
                StatusMessage = $"敵画像 ComfyUI workflow preview 作成に失敗しました: {ex.Message}";
            }
        }

        private async void SubmitEnemyComfyPrompt()
        {
            if (SelectedEnemyProfile == null)
            {
                return;
            }

            RequestComfyPollingCancellation();
            hasComfyInterruptRequested = false;
            IsComfySubmitting = true;
            CurrentComfyPromptId = string.Empty;
            CurrentComfyResultSummary = string.Empty;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            ClearComfyPreviewImage();
            string queuedPromptId = string.Empty;
            string queuedClientId = string.Empty;
            string displayName = BuildEnemyComfyDisplayName();
            try
            {
                PromptRecord promptRecord = CreateEnemyPromptRecord();
                string workflowJson = comfyWorkflowService.BuildWorkflowJson(ComfySettings, promptRecord);
                currentComfySubmittedPromptRecord = promptRecord;
                currentComfyWorkflowJson = workflowJson;
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                ComfyPromptQueueResult queueResult = await comfyClientService.QueuePromptWithClientAsync(ComfySettings, workflowJson);
                CurrentComfyPromptId = queueResult.PromptId;
                queuedPromptId = CurrentComfyPromptId;
                queuedClientId = queueResult.ClientId;
                StatusMessage = $"{displayName} を ComfyUI に送信しました。prompt_id: {CurrentComfyPromptId}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵画像 ComfyUI 送信に失敗しました: {ex.Message}";
            }
            finally
            {
                IsComfySubmitting = false;
            }

            if (!string.IsNullOrWhiteSpace(queuedPromptId))
            {
                await WaitForComfyResultAsync(queuedPromptId, queuedClientId, displayName);
            }
        }

        private void AdoptEnemyComfyImage()
        {
            if (SelectedEnemyProfile == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) || !File.Exists(CurrentComfyPreviewImagePath))
            {
                StatusMessage = "採用できる Comfy 生成画像がありません。先に画像取得を行ってください。";
                return;
            }

            if (string.IsNullOrWhiteSpace(EnemyAssetIdInput))
            {
                EnemyAssetIdInput = BuildDefaultEnemyAssetId(SelectedEnemyProfile);
            }

            EnemyImageSourcePathInput = CurrentComfyPreviewImagePath;
            SelectedEnemyAssetStatus = AssetStatus.Accepted;
            try
            {
                EnemyAsset asset = AddEnemyImageAssetCore();
                if (asset == null)
                {
                    return;
                }

                SaveEnemyComfyPromptRecord(asset);
                StatusMessage += " Comfy 生成条件を敵 prompt 記録に保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Comfy 生成画像の敵画像採用に失敗しました: {ex.Message}";
            }
        }

        private void BrowseEnemyImage()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "登録する敵画像を選択",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                EnemyImageSourcePathInput = dialog.FileName;
                if (string.IsNullOrWhiteSpace(EnemyAssetIdInput))
                {
                    EnemyAssetIdInput = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void AddEnemyImageAsset()
        {
            if (SelectedEnemyProfile == null)
            {
                return;
            }

            try
            {
                AddEnemyImageAssetCore();
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵画像登録に失敗しました: {ex.Message}";
            }
        }

        private EnemyAsset AddEnemyImageAssetCore()
        {
            bool hasExistingAssetId = HasExistingEnemyAssetId();
            bool hasExistingStoredFile = HasExistingEnemyStoredImageFile();
            bool overwriteExisting = ShouldOverwriteExistingEnemyAsset(hasExistingAssetId, hasExistingStoredFile);
            if (overwriteExisting == false && (hasExistingAssetId || hasExistingStoredFile))
            {
                StatusMessage = "敵画像登録をキャンセルしました。";
                return null;
            }

            EnemyAsset asset = enemyProjectService.AddImageAsset(
                SelectedEnemyProfile,
                EnemyImageSourcePathInput,
                EnemyAssetIdInput,
                SelectedEnemyAssetStatus,
                overwriteExisting);

            SelectedEnemyAsset = asset;
            string registrationMessage = overwriteExisting
                ? $"{asset.AssetId} を Battle に上書き登録しました。"
                : $"{asset.AssetId} を Battle に登録しました。";
            StatusMessage = AppendEnemyImageInspectionMessage(registrationMessage, asset);
            return asset;
        }

        private void UnregisterEnemyImageAsset()
        {
            if (SelectedEnemyProfile == null || SelectedEnemyAsset == null)
            {
                return;
            }

            EnemyAsset asset = SelectedEnemyAsset;
            MessageBoxResult result = MessageBox.Show(
                $"AssetId '{asset.AssetId}' の登録を解除しますか？\n画像ファイルと prompt JSON は削除されません。",
                "敵画像登録解除の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "敵画像登録解除をキャンセルしました。";
                return;
            }

            try
            {
                string storedPath = asset.StoredPath;
                string promptPath = asset.PromptRecordPath;
                bool unregistered = enemyProjectService.UnregisterImageAsset(SelectedEnemyProfile, asset);
                if (!unregistered)
                {
                    StatusMessage = $"{asset.AssetId} は敵画像一覧に見つかりませんでした。";
                    return;
                }

                SelectedEnemyAsset = SelectedEnemyProfile.Assets.FirstOrDefault();
                StatusMessage = $"{asset.AssetId} の登録を解除しました。画像ファイルと prompt JSON は残しています。画像: {storedPath} / prompt: {promptPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵画像登録解除に失敗しました: {ex.Message}";
            }
        }

        private bool HasExistingEnemyAssetId()
        {
            if (SelectedEnemyProfile == null || SelectedEnemyProfile.Assets == null || string.IsNullOrWhiteSpace(EnemyAssetIdInput))
            {
                return false;
            }

            string assetId = EnemyAssetIdInput.Trim();
            return SelectedEnemyProfile.Assets.Any(asset => asset.AssetId == assetId);
        }

        private bool HasExistingEnemyStoredImageFile()
        {
            string storedImagePath = BuildEnemyStoredImagePathForInput();
            return !string.IsNullOrWhiteSpace(storedImagePath) && File.Exists(storedImagePath);
        }

        private string BuildEnemyStoredImagePathForInput()
        {
            if (SelectedEnemyProfile == null || string.IsNullOrWhiteSpace(EnemyAssetIdInput))
            {
                return string.Empty;
            }

            string extension = Path.GetExtension(EnemyImageSourcePathInput);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = EnemyAssetIdInput.Trim() + extension;
            return Path.Combine(
                enemyProjectService.GetImageUsageDirectory(SelectedEnemyProfile.EnemyId, AssetUsage.Battle),
                fileName);
        }

        private bool ShouldOverwriteExistingEnemyAsset(bool hasExistingAssetId, bool hasExistingStoredFile)
        {
            if (!hasExistingAssetId && !hasExistingStoredFile)
            {
                return false;
            }

            string message = hasExistingAssetId
                ? $"AssetId '{EnemyAssetIdInput.Trim()}' はすでに登録されています。画像と登録情報を上書きしますか？"
                : $"AssetId '{EnemyAssetIdInput.Trim()}' の登録はありませんが、保存先画像ファイルが残っています。\n残っている画像ファイルを上書きして登録しますか？";

            MessageBoxResult result = MessageBox.Show(
                message,
                hasExistingAssetId ? "敵画像登録の上書き確認" : "敵画像ファイルの上書き確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        private void RefreshSelectedEnemyAssetPreview()
        {
            SelectedEnemyAssetImagePath = string.Empty;

            if (SelectedEnemyProfile == null || SelectedEnemyAsset == null)
            {
                SelectedEnemyAssetImageMessage = "敵画像を選択してください。";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedEnemyAsset.StoredPath))
            {
                SelectedEnemyAssetImageMessage = "StoredPath が空です。";
                return;
            }

            string imagePath = Path.Combine(
                enemyProjectService.GetEnemyDirectory(SelectedEnemyProfile.EnemyId),
                SelectedEnemyAsset.StoredPath);

            if (!File.Exists(imagePath))
            {
                SelectedEnemyAssetImageMessage = "画像ファイルが見つかりません: " + imagePath;
                return;
            }

            SelectedEnemyAssetImagePath = imagePath;
            SelectedEnemyAssetImageMessage = AppendEnemyImageInspectionMessage(imagePath, SelectedEnemyAsset);
        }

        private string AppendEnemyImageInspectionMessage(string baseMessage, EnemyAsset asset)
        {
            if (SelectedEnemyProfile == null || asset == null || string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                return baseMessage;
            }

            string imagePath = Path.Combine(
                enemyProjectService.GetEnemyDirectory(SelectedEnemyProfile.EnemyId),
                asset.StoredPath);

            try
            {
                ImageInspectionResult result = imageInspectionService.Inspect(imagePath, AssetUsage.Battle);
                string summary = imageInspectionService.BuildSummary(result);
                if (result.Warnings.Count == 0)
                {
                    return $"{baseMessage} 検査: {summary}";
                }

                return $"{baseMessage} 検査: {summary} / 警告 {result.Warnings.Count} 件: {string.Join(" / ", result.Warnings)}";
            }
            catch (Exception ex)
            {
                return $"{baseMessage} 画像検査に失敗しました: {ex.Message}";
            }
        }

        private void ApplySelectedEnemyAssetToInputs()
        {
            if (SelectedEnemyProfile == null || SelectedEnemyAsset == null)
            {
                return;
            }

            EnemyAssetIdInput = SelectedEnemyAsset.AssetId;
            SelectedEnemyAssetStatus = SelectedEnemyAsset.Status;
            EnemyImageSourcePathInput = ResolveEnemyAssetInputImagePath(SelectedEnemyProfile, SelectedEnemyAsset);
            LoadEnemyPromptForSelectedAsset();
        }

        private string ResolveEnemyAssetInputImagePath(EnemyProfile profile, EnemyAsset asset)
        {
            if (profile == null || asset == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                string storedPath = Path.Combine(
                    enemyProjectService.GetEnemyDirectory(profile.EnemyId),
                    asset.StoredPath);
                if (File.Exists(storedPath))
                {
                    return storedPath;
                }
            }

            if (!string.IsNullOrWhiteSpace(asset.SourcePath) && File.Exists(asset.SourcePath))
            {
                return asset.SourcePath;
            }

            return string.Empty;
        }

        private void LoadEnemyPromptForSelectedAsset()
        {
            if (SelectedEnemyProfile == null || SelectedEnemyAsset == null)
            {
                return;
            }

            string promptPath = BuildEnemyPromptRecordPath(SelectedEnemyProfile, SelectedEnemyAsset);
            if (!File.Exists(promptPath))
            {
                EnemyAssetPromptInput = GetDefaultEnemyAssetPrompt();
                return;
            }

            try
            {
                string json = File.ReadAllText(promptPath);
                PromptRecord record = JsonSerializer.Deserialize<PromptRecord>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (record == null)
                {
                    EnemyAssetPromptInput = GetDefaultEnemyAssetPrompt();
                    return;
                }

                EnemyAssetPromptInput = string.IsNullOrWhiteSpace(record.RevisionMemo)
                    ? GetDefaultEnemyAssetPrompt()
                    : record.RevisionMemo;
            }
            catch
            {
                EnemyAssetPromptInput = GetDefaultEnemyAssetPrompt();
            }
        }

        private void SaveEnemyComfyPromptRecord(EnemyAsset asset)
        {
            if (SelectedEnemyProfile == null || asset == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                asset.PromptRecordPath = Path.Combine("Prompts", asset.AssetId + ".prompt.json");
            }

            PromptRecord record = currentComfySubmittedPromptRecord ?? CreateEnemyPromptRecord();
            record.ComfyPromptId = CurrentComfyPromptId ?? string.Empty;
            record.ComfyEndpointUrl = ComfySettings != null ? ComfySettings.EndpointUrl : string.Empty;
            record.ComfyWorkflowTemplatePath = ComfySettings != null ? ComfySettings.WorkflowTemplatePath : string.Empty;
            record.ComfyWorkflowJson = currentComfyWorkflowJson ?? string.Empty;
            record.RevisionMemo = EnemyAssetPromptInput ?? string.Empty;

            if (currentComfyOutputImage != null)
            {
                record.ComfyOutputFileName = currentComfyOutputImage.FileName;
                record.ComfyOutputSubfolder = currentComfyOutputImage.Subfolder;
                record.ComfyOutputType = currentComfyOutputImage.Type;
            }

            ApplyComfyWorkflowSettings(record, currentComfyWorkflowJson);

            string promptPath = BuildEnemyPromptRecordPath(SelectedEnemyProfile, asset);
            string promptDirectory = Path.GetDirectoryName(promptPath);
            if (!string.IsNullOrWhiteSpace(promptDirectory))
            {
                Directory.CreateDirectory(promptDirectory);
            }

            string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(promptPath, json);
            enemyProjectService.SaveProfile(SelectedEnemyProfile);
        }

        private void SaveEnemyPromptRecordForTemplate(EnemyAsset asset, string assetPrompt)
        {
            if (SelectedEnemyProfile == null || asset == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                asset.PromptRecordPath = Path.Combine("Prompts", asset.AssetId + ".prompt.json");
            }

            PromptRecord record = new PromptRecord
            {
                PositivePrompt = BuildEnemyPositivePrompt(SelectedEnemyProfile, assetPrompt),
                NegativePrompt = SelectedEnemyProfile.NegativePrompt,
                RevisionMemo = assetPrompt
            };

            string promptPath = BuildEnemyPromptRecordPath(SelectedEnemyProfile, asset);
            string promptDirectory = Path.GetDirectoryName(promptPath);
            if (!string.IsNullOrWhiteSpace(promptDirectory))
            {
                Directory.CreateDirectory(promptDirectory);
            }

            string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(promptPath, json);
        }

        private string BuildEnemyPromptRecordPath(EnemyProfile profile, EnemyAsset asset)
        {
            string relativePath = asset.PromptRecordPath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                relativePath = Path.Combine("Prompts", asset.AssetId + ".prompt.json");
                asset.PromptRecordPath = relativePath;
            }

            return Path.Combine(enemyProjectService.GetEnemyDirectory(profile.EnemyId), relativePath);
        }

        private PromptRecord CreateEnemyPromptRecord()
        {
            return new PromptRecord
            {
                PositivePrompt = BuildEnemyPositivePrompt(SelectedEnemyProfile, EnemyAssetPromptInput),
                NegativePrompt = SelectedEnemyProfile != null ? SelectedEnemyProfile.NegativePrompt : string.Empty,
                RevisionMemo = EnemyAssetPromptInput ?? string.Empty
            };
        }

        private static string BuildEnemyPositivePrompt(EnemyProfile profile, string assetPrompt)
        {
            if (profile == null)
            {
                return string.Empty;
            }

            string[] promptParts =
            {
                NormalizePromptPart(profile.AppearancePrompt),
                NormalizePromptPart(profile.BattleCommonPositivePrompt),
                NormalizePromptPart(assetPrompt)
            };

            return string.Join(", ", promptParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private string BuildEnemyComfyDisplayName()
        {
            if (SelectedEnemyProfile == null)
            {
                return "敵画像";
            }

            string assetId = string.IsNullOrWhiteSpace(EnemyAssetIdInput)
                ? BuildDefaultEnemyAssetId(SelectedEnemyProfile)
                : EnemyAssetIdInput.Trim();
            return $"{SelectedEnemyProfile.DisplayName} / {assetId}";
        }

        private static string BuildDefaultEnemyAssetId(EnemyProfile profile)
        {
            string enemyId = profile == null || string.IsNullOrWhiteSpace(profile.EnemyId)
                ? "Enemy"
                : profile.EnemyId.Trim();
            return $"Enemy_{enemyId}_Idle";
        }

        private static string GetDefaultEnemyAssetPrompt()
        {
            return "solo enemy, full body, front view, battle sprite, transparent background, isolated character";
        }

        private static IReadOnlyList<EnemyAssetTemplate> BuildEnemyStandardAssetTemplates(EnemyProfile profile)
        {
            string enemyId = profile == null || string.IsNullOrWhiteSpace(profile.EnemyId)
                ? "Enemy"
                : profile.EnemyId.Trim();

            return new[]
            {
                new EnemyAssetTemplate(
                    $"Enemy_{enemyId}_Idle",
                    "通常画像",
                    "solo enemy, full body, front view, idle pose, battle sprite, transparent background, isolated character"),
                new EnemyAssetTemplate(
                    $"Enemy_{enemyId}_Attack",
                    "攻撃画像",
                    "solo enemy, full body, dynamic attack pose, aggressive motion, battle sprite, transparent background, isolated character"),
                new EnemyAssetTemplate(
                    $"Enemy_{enemyId}_Damage",
                    "被ダメージ画像",
                    "solo enemy, full body, damaged reaction pose, hurt expression, battle sprite, transparent background, isolated character"),
                new EnemyAssetTemplate(
                    $"Enemy_{enemyId}_Defeat",
                    "撃破画像",
                    "solo enemy, full body, defeated pose, weakened, battle sprite, transparent background, isolated character")
            };
        }

        private void ClearEnemyComfyWork()
        {
            RequestComfyPollingCancellation();
            IsComfyWaitingResult = false;
            CurrentComfyWorkflowPreview = string.Empty;
            CurrentComfyPromptId = string.Empty;
            CurrentComfyResultSummary = string.Empty;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            hasComfyInterruptRequested = false;
            ClearComfyPreviewImage();
        }

        private void LoadPlayerProfile()
        {
            try
            {
                PlayerProfile = playerProjectService.LoadOrCreateProfile();
                SelectedPlayerAsset = PlayerProfile.Assets.FirstOrDefault();
                StatusMessage = "プレイヤー素材を読み込みました。";
            }
            catch (Exception ex)
            {
                PlayerProfile = new PlayerProfile();
                StatusMessage = $"プレイヤー素材読み込みに失敗しました: {ex.Message}";
            }
        }

        private void SavePlayerProfile()
        {
            if (PlayerProfile == null)
            {
                return;
            }

            try
            {
                playerProjectService.SaveProfile(PlayerProfile);
                StatusMessage = "プレイヤー素材を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"プレイヤー素材保存に失敗しました: {ex.Message}";
            }
        }

        private void ExportPlayer()
        {
            if (PlayerProfile == null)
            {
                return;
            }

            try
            {
                playerProjectService.SaveProfile(PlayerProfile);
                LastPlayerExportReport = playerExportService.ExportPlayer(PlayerProfile);
                StatusMessage = $"プレイヤー素材を export しました。画像 {LastPlayerExportReport.ExportedImageCount}/{LastPlayerExportReport.AcceptedAssetCount} 件、prompt {LastPlayerExportReport.ExportedPromptCount} 件、警告 {LastPlayerExportReport.Warnings.Count} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"プレイヤー素材 export に失敗しました: {ex.Message}";
            }
        }

        private void AddPlayerStandardAssetData()
        {
            if (PlayerProfile == null)
            {
                return;
            }

            try
            {
                PlayerProfile.Assets ??= new ObservableCollection<PlayerAsset>();
                playerProjectService.EnsurePlayerDirectories();

                int addedCount = 0;
                int updatedCount = 0;
                PlayerAsset firstAsset = null;
                foreach (PlayerAssetTemplate template in BuildPlayerStandardAssetTemplates())
                {
                    PlayerAsset asset = PlayerProfile.Assets
                        .FirstOrDefault(item => string.Equals(item.AssetId, template.AssetId, StringComparison.Ordinal));
                    if (asset == null)
                    {
                        asset = new PlayerAsset
                        {
                            AssetId = template.AssetId,
                            Usage = AssetUsage.Battle,
                            Status = AssetStatus.Pending,
                            PromptRecordPath = Path.Combine("Prompts", template.AssetId + ".prompt.json"),
                            Memo = template.Memo
                        };
                        PlayerProfile.Assets.Add(asset);
                        addedCount++;
                    }
                    else
                    {
                        asset.Usage = AssetUsage.Battle;
                        if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
                        {
                            asset.PromptRecordPath = Path.Combine("Prompts", template.AssetId + ".prompt.json");
                        }

                        if (string.IsNullOrWhiteSpace(asset.Memo))
                        {
                            asset.Memo = template.Memo;
                        }

                        updatedCount++;
                    }

                    SavePlayerPromptRecordForTemplate(asset, template.Prompt);
                    firstAsset ??= asset;
                }

                playerProjectService.SaveProfile(PlayerProfile);
                SelectedPlayerAsset = firstAsset ?? PlayerProfile.Assets.FirstOrDefault();
                StatusMessage = $"プレイヤー画像の標準候補を登録しました。追加 {addedCount} 件、更新 {updatedCount} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"プレイヤー画像の標準候補登録に失敗しました: {ex.Message}";
            }
        }

        private void BuildPlayerComfyWorkflowPreview()
        {
            if (PlayerProfile == null)
            {
                return;
            }

            try
            {
                PromptRecord promptRecord = CreatePlayerPromptRecord();
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                StatusMessage = $"{BuildPlayerComfyDisplayName()} の ComfyUI workflow preview を作成しました。";
            }
            catch (Exception ex)
            {
                CurrentComfyWorkflowPreview = string.Empty;
                StatusMessage = $"プレイヤー画像 ComfyUI workflow preview 作成に失敗しました: {ex.Message}";
            }
        }

        private async void SubmitPlayerComfyPrompt()
        {
            if (PlayerProfile == null)
            {
                return;
            }

            RequestComfyPollingCancellation();
            hasComfyInterruptRequested = false;
            IsComfySubmitting = true;
            CurrentComfyPromptId = string.Empty;
            CurrentComfyResultSummary = string.Empty;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            ClearComfyPreviewImage();
            string queuedPromptId = string.Empty;
            string queuedClientId = string.Empty;
            string displayName = BuildPlayerComfyDisplayName();
            try
            {
                PromptRecord promptRecord = CreatePlayerPromptRecord();
                string workflowJson = comfyWorkflowService.BuildWorkflowJson(ComfySettings, promptRecord);
                currentComfySubmittedPromptRecord = promptRecord;
                currentComfyWorkflowJson = workflowJson;
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                ComfyPromptQueueResult queueResult = await comfyClientService.QueuePromptWithClientAsync(ComfySettings, workflowJson);
                CurrentComfyPromptId = queueResult.PromptId;
                queuedPromptId = CurrentComfyPromptId;
                queuedClientId = queueResult.ClientId;
                StatusMessage = $"{displayName} を ComfyUI に送信しました。prompt_id: {CurrentComfyPromptId}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"プレイヤー画像 ComfyUI 送信に失敗しました: {ex.Message}";
            }
            finally
            {
                IsComfySubmitting = false;
            }

            if (!string.IsNullOrWhiteSpace(queuedPromptId))
            {
                await WaitForComfyResultAsync(queuedPromptId, queuedClientId, displayName);
            }
        }

        private void AdoptPlayerComfyImage()
        {
            if (PlayerProfile == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentComfyPreviewImagePath) || !File.Exists(CurrentComfyPreviewImagePath))
            {
                StatusMessage = "採用できる Comfy 生成画像がありません。先に画像取得を行ってください。";
                return;
            }

            if (string.IsNullOrWhiteSpace(PlayerAssetIdInput))
            {
                PlayerAssetIdInput = "Battle_Player_Idle";
            }

            PlayerImageSourcePathInput = CurrentComfyPreviewImagePath;
            SelectedPlayerAssetStatus = AssetStatus.Accepted;
            try
            {
                PlayerAsset asset = AddPlayerImageAssetCore();
                if (asset == null)
                {
                    return;
                }

                SavePlayerComfyPromptRecord(asset);
                StatusMessage += " Comfy 生成条件をプレイヤー prompt 記録に保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Comfy 生成画像のプレイヤー画像採用に失敗しました: {ex.Message}";
            }
        }

        private void BrowsePlayerImage()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "登録するプレイヤー画像を選択",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                PlayerImageSourcePathInput = dialog.FileName;
                if (string.IsNullOrWhiteSpace(PlayerAssetIdInput))
                {
                    PlayerAssetIdInput = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void AddPlayerImageAsset()
        {
            if (PlayerProfile == null)
            {
                return;
            }

            try
            {
                AddPlayerImageAssetCore();
            }
            catch (Exception ex)
            {
                StatusMessage = $"プレイヤー画像登録に失敗しました: {ex.Message}";
            }
        }

        private PlayerAsset AddPlayerImageAssetCore()
        {
            bool hasExistingAssetId = HasExistingPlayerAssetId();
            bool hasExistingStoredFile = HasExistingPlayerStoredImageFile();
            bool overwriteExisting = ShouldOverwriteExistingPlayerAsset(hasExistingAssetId, hasExistingStoredFile);
            if (overwriteExisting == false && (hasExistingAssetId || hasExistingStoredFile))
            {
                StatusMessage = "プレイヤー画像登録をキャンセルしました。";
                return null;
            }

            PlayerAsset asset = playerProjectService.AddImageAsset(
                PlayerProfile,
                PlayerImageSourcePathInput,
                PlayerAssetIdInput,
                SelectedPlayerAssetStatus,
                overwriteExisting);

            SelectedPlayerAsset = asset;
            string registrationMessage = overwriteExisting
                ? $"{asset.AssetId} を Battle に上書き登録しました。"
                : $"{asset.AssetId} を Battle に登録しました。";
            StatusMessage = AppendPlayerImageInspectionMessage(registrationMessage, asset);
            return asset;
        }

        private void UnregisterPlayerImageAsset()
        {
            if (PlayerProfile == null || SelectedPlayerAsset == null)
            {
                return;
            }

            PlayerAsset asset = SelectedPlayerAsset;
            MessageBoxResult result = MessageBox.Show(
                $"AssetId '{asset.AssetId}' の登録を解除しますか？\n画像ファイルと prompt JSON は削除されません。",
                "プレイヤー画像登録解除の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "プレイヤー画像登録解除をキャンセルしました。";
                return;
            }

            try
            {
                string storedPath = asset.StoredPath;
                string promptPath = asset.PromptRecordPath;
                bool unregistered = playerProjectService.UnregisterImageAsset(PlayerProfile, asset);
                if (!unregistered)
                {
                    StatusMessage = $"{asset.AssetId} はプレイヤー画像一覧に見つかりませんでした。";
                    return;
                }

                SelectedPlayerAsset = PlayerProfile.Assets.FirstOrDefault();
                StatusMessage = $"{asset.AssetId} の登録を解除しました。画像ファイルと prompt JSON は残しています。画像: {storedPath} / prompt: {promptPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"プレイヤー画像登録解除に失敗しました: {ex.Message}";
            }
        }

        private bool HasExistingPlayerAssetId()
        {
            if (PlayerProfile == null || PlayerProfile.Assets == null || string.IsNullOrWhiteSpace(PlayerAssetIdInput))
            {
                return false;
            }

            string assetId = PlayerAssetIdInput.Trim();
            return PlayerProfile.Assets.Any(asset => asset.AssetId == assetId);
        }

        private bool HasExistingPlayerStoredImageFile()
        {
            string storedImagePath = BuildPlayerStoredImagePathForInput();
            return !string.IsNullOrWhiteSpace(storedImagePath) && File.Exists(storedImagePath);
        }

        private string BuildPlayerStoredImagePathForInput()
        {
            if (PlayerProfile == null || string.IsNullOrWhiteSpace(PlayerAssetIdInput))
            {
                return string.Empty;
            }

            string extension = Path.GetExtension(PlayerImageSourcePathInput);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = PlayerAssetIdInput.Trim() + extension;
            return Path.Combine(
                playerProjectService.GetImageUsageDirectory(AssetUsage.Battle),
                fileName);
        }

        private bool ShouldOverwriteExistingPlayerAsset(bool hasExistingAssetId, bool hasExistingStoredFile)
        {
            if (!hasExistingAssetId && !hasExistingStoredFile)
            {
                return false;
            }

            string message = hasExistingAssetId
                ? $"AssetId '{PlayerAssetIdInput.Trim()}' はすでに登録されています。画像と登録情報を上書きしますか？"
                : $"AssetId '{PlayerAssetIdInput.Trim()}' の登録はありませんが、保存先画像ファイルが残っています。\n残っている画像ファイルを上書きして登録しますか？";

            MessageBoxResult result = MessageBox.Show(
                message,
                hasExistingAssetId ? "プレイヤー画像登録の上書き確認" : "プレイヤー画像ファイルの上書き確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        private void RefreshSelectedPlayerAssetPreview()
        {
            SelectedPlayerAssetImagePath = string.Empty;

            if (PlayerProfile == null || SelectedPlayerAsset == null)
            {
                SelectedPlayerAssetImageMessage = "プレイヤー画像を選択してください。";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedPlayerAsset.StoredPath))
            {
                SelectedPlayerAssetImageMessage = "StoredPath が空です。";
                return;
            }

            string imagePath = Path.Combine(playerProjectService.PlayerDirectory, SelectedPlayerAsset.StoredPath);
            if (!File.Exists(imagePath))
            {
                SelectedPlayerAssetImageMessage = "画像ファイルが見つかりません: " + imagePath;
                return;
            }

            SelectedPlayerAssetImagePath = imagePath;
            SelectedPlayerAssetImageMessage = AppendPlayerImageInspectionMessage(imagePath, SelectedPlayerAsset);
        }

        private string AppendPlayerImageInspectionMessage(string baseMessage, PlayerAsset asset)
        {
            if (PlayerProfile == null || asset == null || string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                return baseMessage;
            }

            string imagePath = Path.Combine(playerProjectService.PlayerDirectory, asset.StoredPath);
            try
            {
                ImageInspectionResult result = imageInspectionService.Inspect(imagePath, AssetUsage.Battle);
                string summary = imageInspectionService.BuildSummary(result);
                if (result.Warnings.Count == 0)
                {
                    return $"{baseMessage} 検査: {summary}";
                }

                return $"{baseMessage} 検査: {summary} / 警告 {result.Warnings.Count} 件: {string.Join(" / ", result.Warnings)}";
            }
            catch (Exception ex)
            {
                return $"{baseMessage} 画像検査に失敗しました: {ex.Message}";
            }
        }

        private void ApplySelectedPlayerAssetToInputs()
        {
            if (PlayerProfile == null || SelectedPlayerAsset == null)
            {
                return;
            }

            PlayerAssetIdInput = SelectedPlayerAsset.AssetId;
            SelectedPlayerAssetStatus = SelectedPlayerAsset.Status;
            PlayerImageSourcePathInput = ResolvePlayerAssetInputImagePath(SelectedPlayerAsset);
            LoadPlayerPromptForSelectedAsset();
        }

        private string ResolvePlayerAssetInputImagePath(PlayerAsset asset)
        {
            if (asset == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                string storedPath = Path.Combine(playerProjectService.PlayerDirectory, asset.StoredPath);
                if (File.Exists(storedPath))
                {
                    return storedPath;
                }
            }

            if (!string.IsNullOrWhiteSpace(asset.SourcePath) && File.Exists(asset.SourcePath))
            {
                return asset.SourcePath;
            }

            return string.Empty;
        }

        private void LoadPlayerPromptForSelectedAsset()
        {
            if (PlayerProfile == null || SelectedPlayerAsset == null)
            {
                return;
            }

            string promptPath = BuildPlayerPromptRecordPath(SelectedPlayerAsset);
            if (!File.Exists(promptPath))
            {
                PlayerAssetPromptInput = GetDefaultPlayerAssetPrompt();
                return;
            }

            try
            {
                string json = File.ReadAllText(promptPath);
                PromptRecord record = JsonSerializer.Deserialize<PromptRecord>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (record == null)
                {
                    PlayerAssetPromptInput = GetDefaultPlayerAssetPrompt();
                    return;
                }

                PlayerAssetPromptInput = string.IsNullOrWhiteSpace(record.RevisionMemo)
                    ? GetDefaultPlayerAssetPrompt()
                    : record.RevisionMemo;
            }
            catch
            {
                PlayerAssetPromptInput = GetDefaultPlayerAssetPrompt();
            }
        }

        private void SavePlayerComfyPromptRecord(PlayerAsset asset)
        {
            if (PlayerProfile == null || asset == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                asset.PromptRecordPath = Path.Combine("Prompts", asset.AssetId + ".prompt.json");
            }

            PromptRecord record = currentComfySubmittedPromptRecord ?? CreatePlayerPromptRecord();
            record.ComfyPromptId = CurrentComfyPromptId ?? string.Empty;
            record.ComfyEndpointUrl = ComfySettings != null ? ComfySettings.EndpointUrl : string.Empty;
            record.ComfyWorkflowTemplatePath = ComfySettings != null ? ComfySettings.WorkflowTemplatePath : string.Empty;
            record.ComfyWorkflowJson = currentComfyWorkflowJson ?? string.Empty;
            record.RevisionMemo = PlayerAssetPromptInput ?? string.Empty;

            if (currentComfyOutputImage != null)
            {
                record.ComfyOutputFileName = currentComfyOutputImage.FileName;
                record.ComfyOutputSubfolder = currentComfyOutputImage.Subfolder;
                record.ComfyOutputType = currentComfyOutputImage.Type;
            }

            ApplyComfyWorkflowSettings(record, currentComfyWorkflowJson);
            string promptPath = BuildPlayerPromptRecordPath(asset);
            string promptDirectory = Path.GetDirectoryName(promptPath);
            if (!string.IsNullOrWhiteSpace(promptDirectory))
            {
                Directory.CreateDirectory(promptDirectory);
            }

            string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(promptPath, json);
            playerProjectService.SaveProfile(PlayerProfile);
        }

        private void SavePlayerPromptRecordForTemplate(PlayerAsset asset, string assetPrompt)
        {
            if (PlayerProfile == null || asset == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                asset.PromptRecordPath = Path.Combine("Prompts", asset.AssetId + ".prompt.json");
            }

            PromptRecord record = new PromptRecord
            {
                PositivePrompt = BuildPlayerPositivePrompt(PlayerProfile, assetPrompt),
                NegativePrompt = PlayerProfile.NegativePrompt,
                RevisionMemo = assetPrompt
            };

            string promptPath = BuildPlayerPromptRecordPath(asset);
            string promptDirectory = Path.GetDirectoryName(promptPath);
            if (!string.IsNullOrWhiteSpace(promptDirectory))
            {
                Directory.CreateDirectory(promptDirectory);
            }

            string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(promptPath, json);
        }

        private string BuildPlayerPromptRecordPath(PlayerAsset asset)
        {
            string relativePath = asset.PromptRecordPath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                relativePath = Path.Combine("Prompts", asset.AssetId + ".prompt.json");
                asset.PromptRecordPath = relativePath;
            }

            return Path.Combine(playerProjectService.PlayerDirectory, relativePath);
        }

        private PromptRecord CreatePlayerPromptRecord()
        {
            return new PromptRecord
            {
                PositivePrompt = BuildPlayerPositivePrompt(PlayerProfile, PlayerAssetPromptInput),
                NegativePrompt = PlayerProfile != null ? PlayerProfile.NegativePrompt : string.Empty,
                RevisionMemo = PlayerAssetPromptInput ?? string.Empty
            };
        }

        private static string BuildPlayerPositivePrompt(PlayerProfile profile, string assetPrompt)
        {
            if (profile == null)
            {
                return string.Empty;
            }

            string[] promptParts =
            {
                NormalizePromptPart(profile.AppearancePrompt),
                NormalizePromptPart(profile.BattleCommonPositivePrompt),
                NormalizePromptPart(assetPrompt)
            };

            return string.Join(", ", promptParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private string BuildPlayerComfyDisplayName()
        {
            string assetId = string.IsNullOrWhiteSpace(PlayerAssetIdInput)
                ? "Battle_Player_Idle"
                : PlayerAssetIdInput.Trim();
            return $"プレイヤー / {assetId}";
        }

        private static string GetDefaultPlayerAssetPrompt()
        {
            return "solo player character, full body, front view, battle sprite, transparent background, isolated character";
        }

        private static IReadOnlyList<PlayerAssetTemplate> BuildPlayerStandardAssetTemplates()
        {
            return new[]
            {
                new PlayerAssetTemplate(
                    "Battle_Player_Idle",
                    "通常画像",
                    "solo player character, full body, front view, idle pose, battle sprite, transparent background, isolated character"),
                new PlayerAssetTemplate(
                    "Battle_Player_Attack",
                    "攻撃画像",
                    "solo player character, full body, dynamic attack pose, battle sprite, transparent background, isolated character"),
                new PlayerAssetTemplate(
                    "Battle_Player_Damage",
                    "被ダメージ画像",
                    "solo player character, full body, damaged reaction pose, battle sprite, transparent background, isolated character"),
                new PlayerAssetTemplate(
                    "Battle_Player_Victory",
                    "勝利画像",
                    "solo player character, full body, victory pose, confident, battle sprite, transparent background, isolated character"),
                new PlayerAssetTemplate(
                    "Battle_Player_Defeat",
                    "敗北画像",
                    "solo player character, full body, defeated pose, weakened, battle sprite, transparent background, isolated character")
            };
        }

        private void ClearPlayerComfyWork()
        {
            RequestComfyPollingCancellation();
            IsComfyWaitingResult = false;
            CurrentComfyWorkflowPreview = string.Empty;
            CurrentComfyPromptId = string.Empty;
            CurrentComfyResultSummary = string.Empty;
            currentComfySubmittedPromptRecord = null;
            currentComfyWorkflowJson = string.Empty;
            hasComfyInterruptRequested = false;
            ClearComfyPreviewImage();
        }

        private void BrowseImage()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "登録する画像を選択",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                ImageSourcePathInput = dialog.FileName;
                if (string.IsNullOrWhiteSpace(AssetIdInput))
                {
                    AssetIdInput = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void BrowseHeadPartImage()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = $"{SelectedHeadPartWorkspaceItem?.DisplayName ?? "頭パーツ"}の透過PNGを選択",
                Filter = "PNG image (*.png)|*.png"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedHeadPartWorkspaceItem.SourceImagePath = dialog.FileName;
                HeadPartWorkspaceMessage = $"{SelectedHeadPartWorkspaceItem.DisplayName}の画像を選択しました。";
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public void SetHeadPartImageFromDroppedFiles(string[] filePaths)
        {
            if (SelectedHeadPartWorkspaceItem == null || filePaths == null) return;

            string imagePath = filePaths.FirstOrDefault(path =>
                File.Exists(path) && string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                HeadPartWorkspaceMessage = "透過PNGファイルをドロップしてください。";
                return;
            }

            SelectedHeadPartWorkspaceItem.SourceImagePath = imagePath;
            HeadPartWorkspaceMessage = $"{SelectedHeadPartWorkspaceItem.DisplayName}の画像を選択しました。";
            CommandManager.InvalidateRequerySuggested();
        }

        private void TestHeadPartPreview()
        {
            HeadPartWorkspaceItem selectedItem = SelectedHeadPartWorkspaceItem;
            if (SelectedProfile == null || selectedItem == null) return;

            if (!string.Equals(Path.GetExtension(selectedItem.SourceImagePath), ".png", StringComparison.OrdinalIgnoreCase))
            {
                HeadPartWorkspaceMessage = "テスト表示にはPNGファイルを選択してください。";
                return;
            }

            if (selectedItem.IsExpression && string.IsNullOrWhiteSpace(selectedItem.ExpressionId))
            {
                HeadPartWorkspaceMessage = "テスト表示する表情の種類を入力してください。";
                return;
            }

            RefreshHeadPartWorkspace();
            List<LayerPreviewItem> previewItems = new List<LayerPreviewItem>();
            foreach (HeadPartWorkspaceItem item in HeadPartWorkspaceItems)
            {
                string imagePath = ReferenceEquals(item, selectedItem)
                    ? item.SourceImagePath
                    : item.RegisteredImagePath;
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath)) continue;

                previewItems.Add(new LayerPreviewItem
                {
                    AssetId = item.AssetId,
                    DisplayName = ReferenceEquals(item, selectedItem)
                        ? $"{item.DisplayName}（テスト）"
                        : item.IsExpression ? $"表情: {item.ExpressionId}" : item.DisplayName,
                    LayerKind = item.LayerKind,
                    DrawOrder = item.DrawOrder,
                    ImagePath = imagePath
                });
            }

            HeadPartPreviewItems.Clear();
            LayerPreviewItems.Clear();
            foreach (LayerPreviewItem previewItem in previewItems.OrderBy(item => item.DrawOrder))
            {
                HeadPartPreviewItems.Add(previewItem);
                LayerPreviewItems.Add(previewItem);
            }

            LayerPreviewMessage =
                $"{selectedItem.DisplayName}の未登録画像をテスト表示しています。保存や採用は行っていません。";
            HeadPartWorkspaceMessage = LayerPreviewMessage;
            SelectedMainTabIndex = LayerPreviewTabIndex;
        }

        private void AdoptHeadPartImage()
        {
            HeadPartWorkspaceItem item = SelectedHeadPartWorkspaceItem;
            if (SelectedProfile == null || item == null) return;

            try
            {
                if (item.IsExpression)
                {
                    string expressionId = item.ExpressionId?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(expressionId))
                    {
                        HeadPartWorkspaceMessage = "表情の種類を入力してください。例: Neutral、Smile、Angry";
                        return;
                    }

                    if (expressionId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    {
                        HeadPartWorkspaceMessage = "表情の種類にファイル名として使用できない文字が含まれています。";
                        return;
                    }

                    item.ExpressionId = expressionId;
                    item.AssetId = "HeadExpression_" + expressionId.Replace(' ', '_');
                }

                HeroineAsset existingAsset = SelectedProfile.Assets?.FirstOrDefault(asset =>
                    string.Equals(asset.AssetId, item.AssetId, StringComparison.OrdinalIgnoreCase));
                string destinationPath = Path.Combine(
                    characterProjectService.GetImageUsageDirectory(SelectedProfile.HeroineId, AssetUsage.Sprites),
                    item.AssetId + Path.GetExtension(item.SourceImagePath));
                bool hasOrphanedImage = existingAsset == null && File.Exists(destinationPath);
                if (existingAsset != null || hasOrphanedImage)
                {
                    string message = existingAsset != null
                        ? $"{item.DisplayName}（{item.AssetId}）は登録済みです。画像を上書きしますか？"
                        : $"{item.DisplayName}（{item.AssetId}）の保存済み画像がありますが、登録情報がありません。画像を使って登録を復旧しますか？";
                    MessageBoxResult result = MessageBox.Show(
                        message,
                        existingAsset != null ? "頭パーツ画像の上書き確認" : "頭パーツ画像の登録復旧",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes) return;
                }

                LayerAssetDefinition definition = LayerAssetDefinitions.FirstOrDefault(layer =>
                    string.Equals(layer.AssetId, item.AssetId, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    definition = new LayerAssetDefinition { AssetId = item.AssetId };
                    LayerAssetDefinitions.Add(definition);
                }

                definition.LayerKind = item.LayerKind;
                definition.ExpressionId = item.ExpressionId;
                definition.CostumeId = string.Empty;
                definition.DisplayName = item.IsExpression
                    ? $"表情: {item.ExpressionId}"
                    : item.DisplayName;
                definition.FileName = item.AssetId + ".png";
                definition.DrawOrder = item.DrawOrder;
                definition.Prompt = item.Description + ", transparent background, aligned to base body";

                if (!string.IsNullOrWhiteSpace(item.ExpressionId) &&
                    !ExpressionDefinitions.Any(expression => string.Equals(
                        expression.ExpressionId,
                        item.ExpressionId,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    ExpressionDefinitions.Add(new ExpressionDefinition
                    {
                        ExpressionId = item.ExpressionId,
                        DisplayName = item.ExpressionId,
                        UnityExpressionId = item.ExpressionId,
                        Prompt = item.ExpressionId + " face expression"
                    });
                    if (!ExpressionIdOptions.Contains(item.ExpressionId))
                    {
                        ExpressionIdOptions.Add(item.ExpressionId);
                    }
                    definitionCatalogService.SaveExpressionDefinitionFile(ExpressionDefinitions);
                }

                HeroineAsset asset = characterProjectService.AddImageAsset(
                    SelectedProfile,
                    item.SourceImagePath,
                    AssetUsage.Sprites,
                    item.AssetId,
                    AssetStatus.Accepted,
                    existingAsset != null || hasOrphanedImage);
                definition.FileName = asset.FileName;
                definitionCatalogService.SaveLayerAssetDefinitionFile(LayerAssetDefinitions);

                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                RefreshLayerPreviewOptions();
                if (item.IsExpression)
                {
                    selectedLayerPreviewExpressionId = item.ExpressionId;
                    OnPropertyChanged(nameof(SelectedLayerPreviewExpressionId));
                }
                else
                {
                    selectedLayerPreviewBaseBodyId = item.AssetId;
                    OnPropertyChanged(nameof(SelectedLayerPreviewBaseBodyId));
                }
                RefreshLayerPreview();
                RefreshHeadPartWorkspace();
                SelectedHeadPartWorkspaceItem = item;
                HeadPartWorkspaceMessage = AppendImageInspectionMessage(
                    $"{item.DisplayName}を登録し、採用済みにしました。",
                    asset);
            }
            catch (Exception ex)
            {
                HeadPartWorkspaceMessage = $"頭パーツ画像の登録に失敗しました: {ex.Message}";
            }
        }

        private void RefreshHeadPartWorkspace()
        {
            if (HeadPartWorkspaceItems == null || HeadPartPreviewItems == null) return;

            HeadPartPreviewItems.Clear();
            foreach (HeadPartWorkspaceItem item in HeadPartWorkspaceItems)
            {
                LayerAssetDefinition definition = LayerAssetDefinitions?.FirstOrDefault(layer =>
                    string.Equals(layer.LayerKind, item.LayerKind, StringComparison.OrdinalIgnoreCase) &&
                    (item.LayerKind != "HeadExpression" ||
                        string.Equals(layer.ExpressionId, item.ExpressionId, StringComparison.OrdinalIgnoreCase)));
                if (definition != null)
                {
                    item.AssetId = definition.AssetId;
                    item.DrawOrder = definition.DrawOrder;
                }

                HeroineAsset asset = SelectedProfile?.Assets?.FirstOrDefault(candidate =>
                    string.Equals(candidate.AssetId, item.AssetId, StringComparison.OrdinalIgnoreCase));
                string imagePath = asset == null ? string.Empty : BuildStoredImagePath(asset);
                bool hasImage = !string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath);
                item.RegisteredImagePath = hasImage ? imagePath : string.Empty;
                item.StatusText = asset == null
                    ? "未登録"
                    : hasImage ? asset.Status.ToString() : $"{asset.Status} / ファイルなし";

                if (asset?.Status == AssetStatus.Accepted && hasImage)
                {
                    HeadPartPreviewItems.Add(new LayerPreviewItem
                    {
                        AssetId = item.AssetId,
                        DisplayName = item.IsExpression ? $"表情: {item.ExpressionId}" : item.DisplayName,
                        LayerKind = item.LayerKind,
                        DrawOrder = item.DrawOrder,
                        ImagePath = imagePath
                    });
                }
            }

            HeadPartWorkspaceMessage = SelectedProfile == null
                ? "キャラクターを選択してください。"
                : $"採用済み頭パーツ {HeadPartPreviewItems.Count}/{HeadPartWorkspaceItems.Count} 件";
        }

        private void SelectedHeadPartWorkspaceItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(HeadPartWorkspaceItem.ExpressionId) ||
                sender is not HeadPartWorkspaceItem item ||
                !item.IsExpression)
            {
                return;
            }

            string expressionId = item.ExpressionId?.Trim() ?? string.Empty;
            item.AssetId = string.IsNullOrWhiteSpace(expressionId)
                ? "HeadExpression_"
                : "HeadExpression_" + expressionId.Replace(' ', '_');
            RefreshHeadPartWorkspace();
        }

        private void AddHeroineBattleStandardAssetData()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                SelectedProfile.Assets ??= new ObservableCollection<HeroineAsset>();

                int addedCount = 0;
                int updatedCount = 0;
                HeroineAsset firstAsset = null;
                foreach (StillDefinition definition in GetHeroineBattleStandardDefinitions())
                {
                    bool existed = SelectedProfile.Assets.Any(asset => asset.AssetId == definition.AssetId);
                    HeroineAsset asset = EnsureAssetForStill(SelectedProfile, definition);
                    asset.Usage = AssetUsage.Battle;
                    asset.FileName = string.IsNullOrWhiteSpace(asset.FileName)
                        ? definition.FileName
                        : asset.FileName;
                    asset.PromptRecordPath = string.IsNullOrWhiteSpace(asset.PromptRecordPath)
                        ? Path.Combine("Prompts", asset.AssetId + ".prompt.json")
                        : asset.PromptRecordPath;
                    if (string.IsNullOrWhiteSpace(asset.Memo))
                    {
                        asset.Memo = "ヒロイン戦闘画像の標準候補";
                    }

                    SavePromptRecordForHeroineBattleTemplate(asset, definition);
                    firstAsset ??= asset;
                    if (existed)
                    {
                        updatedCount++;
                    }
                    else
                    {
                        addedCount++;
                    }
                }

                characterProjectService.SaveProfile(SelectedProfile);
                SelectedAssetStatusFilter = "All";
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                SelectedAsset = firstAsset ?? SelectedProfile.Assets.FirstOrDefault(asset => asset.Usage == AssetUsage.Battle);
                if (SelectedAsset != null)
                {
                    AssetIdInput = SelectedAsset.AssetId;
                    SelectedAssetUsage = SelectedAsset.Usage;
                    SelectedAssetStatus = SelectedAsset.Status;
                }

                RefreshSelectedStillStatus();
                StatusMessage = $"ヒロイン戦闘画像の標準候補を登録しました。追加 {addedCount} 件、更新 {updatedCount} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"ヒロイン戦闘画像の標準候補登録に失敗しました: {ex.Message}";
            }
        }

        private void AddTrainingStandardAssetData()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                SelectedProfile.Assets ??= new ObservableCollection<HeroineAsset>();
                SelectedProfile.TrainingImages ??= new TrainingImageSettings();
                SelectedProfile.TrainingImages.Defaults ??= new TrainingImageDefaults();
                SelectedProfile.TrainingImages.Items ??= new ObservableCollection<TrainingImageEntry>();
                SelectedProfile.TrainingDialogues ??= new TrainingDialogueSettings();
                SelectedProfile.TrainingDialogues.Items ??= new ObservableCollection<TrainingDialogueEntry>();

                EnsureLegacyTrainingCatalog();
                IReadOnlyList<TrainingAssetTemplate> templates = CreateTrainingAssetTemplates();
                int addedCount = 0;
                HeroineAsset firstAsset = null;
                foreach (TrainingAssetTemplate template in templates)
                {
                    HeroineAsset asset = SelectedProfile.Assets.FirstOrDefault(
                        item => string.Equals(item.AssetId, template.AssetId, StringComparison.Ordinal));
                    if (asset == null)
                    {
                        asset = new HeroineAsset
                        {
                            AssetId = template.AssetId,
                            Usage = AssetUsage.Training,
                            Status = AssetStatus.Pending,
                            FileName = template.AssetId + ".png",
                            PromptRecordPath = Path.Combine("Prompts", template.AssetId + ".prompt.json"),
                            Memo = "訓練画像の標準候補"
                        };
                        SelectedProfile.Assets.Add(asset);
                        addedCount++;
                    }
                    else
                    {
                        asset.Usage = AssetUsage.Training;
                        asset.FileName = string.IsNullOrWhiteSpace(asset.FileName)
                            ? template.AssetId + ".png"
                            : asset.FileName;
                        asset.PromptRecordPath = string.IsNullOrWhiteSpace(asset.PromptRecordPath)
                            ? Path.Combine("Prompts", template.AssetId + ".prompt.json")
                            : asset.PromptRecordPath;
                    }

                    SaveTrainingPromptRecord(asset, template);
                    EnsureStandardTrainingDialogue(template);
                    firstAsset ??= asset;
                }

                ApplyStandardTrainingMappings(
                    SelectedProfile.TrainingImages,
                    SelectedProfile.TrainingCatalog.Items);
                characterProjectService.SaveProfile(SelectedProfile);
                SelectedAssetStatusFilter = "All";
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                SelectedAsset = firstAsset;
                SelectedTrainingImageEntry = SelectedProfile.TrainingImages.Items.FirstOrDefault();
                StatusMessage = $"登録済み訓練 {SelectedProfile.TrainingCatalog.Items.Count} 件の不足枠を準備しました。画像候補の新規追加 {addedCount} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"訓練画像の標準枠作成に失敗しました: {ex.Message}";
            }
        }

        private void AddTrainingImageEntry()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            SelectedProfile.TrainingImages ??= new TrainingImageSettings();
            SelectedProfile.TrainingImages.Items ??= new ObservableCollection<TrainingImageEntry>();
            TrainingImageEntry item = new TrainingImageEntry();
            SelectedProfile.TrainingImages.Items.Add(item);
            SelectedTrainingImageEntry = item;
        }

        private void AddTrainingDialogueMessage()
        {
            if (SelectedTrainingDialogueEntry == null)
            {
                return;
            }

            SelectedTrainingDialogueEntry.Messages ??= new ObservableCollection<TrainingDialogueMessage>();
            TrainingDialogueMessage message = new TrainingDialogueMessage();
            SelectedTrainingDialogueEntry.Messages.Add(message);
            SelectedTrainingDialogueMessage = message;
        }

        private void RemoveTrainingDialogueMessage()
        {
            if (SelectedTrainingDialogueEntry?.Messages == null || SelectedTrainingDialogueMessage == null)
            {
                return;
            }

            SelectedTrainingDialogueEntry.Messages.Remove(SelectedTrainingDialogueMessage);
            SelectedTrainingDialogueMessage = SelectedTrainingDialogueEntry.Messages.FirstOrDefault();
        }

        private void ImportTrainingDialoguesFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "training_dialogues_from_unity.json を選択",
                Filter = "training_dialogues_from_unity.json|training_dialogues_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity 訓練セリフ import をキャンセルしました。";
                return;
            }

            try
            {
                ImportTrainingDialoguesFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity 訓練セリフ import に失敗しました: {ex.Message}";
            }
        }

        private void ImportTrainingCatalogFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "training_catalog_from_unity.json を選択",
                Filter = "training_catalog_from_unity.json|training_catalog_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity 訓練一覧 import をキャンセルしました。";
                return;
            }

            try
            {
                ImportTrainingCatalogFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity 訓練一覧 import に失敗しました: {ex.Message}";
            }
        }

        private void ImportTrainingCatalogFromUnityFile(string filePath)
        {
            FromUnityTrainingCatalogDataFile data =
                TrainingCatalogSyncService.DeserializeFromUnity(File.ReadAllText(filePath));

            SelectedProfile.TrainingCatalog ??= new TrainingCatalogSettings();
            TrainingCatalogMergeResult result = TrainingCatalogSyncService.MergeFromUnity(
                SelectedProfile.TrainingCatalog,
                SelectedProfile.HeroineId,
                data);

            characterProjectService.SaveProfile(SelectedProfile);
            OnPropertyChanged(nameof(SelectedProfile));
            StatusMessage =
                $"FromUnity 訓練一覧を取り込みました。追加 {result.AddedCount} 件、更新 {result.UpdatedCount} 件、" +
                $"重複・不正値スキップ {result.SkippedCount} 件、条件警告 {result.WarningCount} 件。" +
                "旧JSONにない条件とTool専用項目は維持しています。";
        }

        private void ImportTrainingDialoguesFromUnityFile(string filePath)
        {
            FromUnityTrainingDialogueDataFile data = TrainingDialogueSyncService.DeserializeFromUnity(File.ReadAllText(filePath));
            SelectedProfile.TrainingDialogues ??= new TrainingDialogueSettings();
            TrainingDialogueMergeResult result = TrainingDialogueSyncService.MergeFromUnity(
                SelectedProfile.TrainingDialogues, SelectedProfile.HeroineId, data);

            characterProjectService.SaveProfile(SelectedProfile);
            RefreshSelectedTrainingDialogueEntry();
            OnPropertyChanged(nameof(SelectedProfile));
            StatusMessage =
                $"FromUnity 訓練セリフを取り込みました。枠追加 {result.AddedEntryCount} 件、候補追加 {result.AddedMessageCount} 件、Voice ID更新 {result.UpdatedVoiceIdCount} 件、重複・不正値スキップ {result.SkippedCount} 件。";
        }

        private void RefreshSelectedTrainingDialogueEntry()
        {
            if (SelectedProfile?.TrainingDialogues?.Items == null || CurrentPromptRecord == null)
            {
                SelectedTrainingDialogueEntry = null;
                return;
            }

            SelectedTrainingDialogueEntry = SelectedProfile.TrainingDialogues.Items.FirstOrDefault(entry =>
                entry != null &&
                string.Equals(entry.TrainingId, CurrentPromptRecord.TrainingId, StringComparison.Ordinal) &&
                string.Equals(entry.VisualState, CurrentPromptRecord.TrainingVisualState, StringComparison.Ordinal));
        }

        private void EnsureStandardTrainingDialogue(TrainingAssetTemplate template)
        {
            TrainingDialogueEntry entry = SelectedProfile.TrainingDialogues.Items.FirstOrDefault(item =>
                item != null &&
                string.Equals(item.TrainingId, template.TrainingId, StringComparison.Ordinal) &&
                string.Equals(item.VisualState, template.VisualState, StringComparison.Ordinal));
            if (entry == null)
            {
                entry = new TrainingDialogueEntry
                {
                    TrainingId = template.TrainingId,
                    VisualState = template.VisualState
                };
                SelectedProfile.TrainingDialogues.Items.Add(entry);
            }
            entry.Messages ??= new ObservableCollection<TrainingDialogueMessage>();
            if (entry.Messages.Count == 0)
            {
                entry.Messages.Add(new TrainingDialogueMessage { Text = template.DialogueMessage });
            }
        }

        private void AdoptTrainingExternalImage()
        {
            if (SelectedProfile == null || SelectedTrainingAsset == null || CurrentPromptRecord == null)
            {
                return;
            }

            HeroineAsset targetAsset = SelectedTrainingAsset;
            try
            {
                promptRecordService.SavePromptRecord(SelectedProfile, targetAsset, CurrentPromptRecord);
                AssetIdInput = targetAsset.AssetId;
                SelectedAssetUsage = AssetUsage.Training;
                SelectedAssetStatus = AssetStatus.Accepted;
                HeroineAsset asset = AddImageAssetCore();
                if (asset == null)
                {
                    return;
                }

                SelectNextPendingTrainingAsset(asset);
            }
            catch (Exception ex)
            {
                StatusMessage = $"訓練画像の採用に失敗しました: {ex.Message}";
            }
        }

        private void RemoveTrainingImageEntry()
        {
            if (SelectedProfile?.TrainingImages?.Items == null || SelectedTrainingImageEntry == null)
            {
                return;
            }

            SelectedProfile.TrainingImages.Items.Remove(SelectedTrainingImageEntry);
            SelectedTrainingImageEntry = SelectedProfile.TrainingImages.Items.FirstOrDefault();
        }

        private void SaveTrainingPromptRecord(HeroineAsset asset, TrainingAssetTemplate template)
        {
            PromptRecord record = promptRecordService.LoadOrCreatePromptRecord(SelectedProfile, asset);
            record.PositivePrompt = MergePromptParts(
                SelectedProfile.AppearancePrompt,
                SelectedProfile.StillCommonPositivePrompt,
                template.PositivePrompt);
            record.TrainingId = template.TrainingId;
            record.TrainingVisualState = template.VisualState;
            record.PlayerVisible = true;
            record.HeroineVisible = true;
            record.RevisionMemo = template.PositivePrompt;
            promptRecordService.SavePromptRecord(SelectedProfile, asset, record);
        }

        private static void ApplyStandardTrainingMappings(
            TrainingImageSettings settings,
            IEnumerable<TrainingCatalogItem> catalogItems)
        {
            foreach (TrainingCatalogItem catalogItem in catalogItems ?? Enumerable.Empty<TrainingCatalogItem>())
            {
                string trainingId = catalogItem?.TrainingId?.Trim();
                if (string.IsNullOrWhiteSpace(trainingId)) continue;
                TrainingImageEntry entry = settings.Items.FirstOrDefault(
                    item => string.Equals(item.TrainingId, trainingId, StringComparison.Ordinal));
                if (entry == null)
                {
                    entry = new TrainingImageEntry { TrainingId = trainingId };
                    settings.Items.Add(entry);
                }
                entry.BeforeFirstStepImageAssetId = DefaultIfEmpty(
                    entry.BeforeFirstStepImageAssetId,
                    $"Training_{trainingId}_SelectedBeforeFirstStep");
                entry.AfterFirstStepImageAssetId = DefaultIfEmpty(
                    entry.AfterFirstStepImageAssetId,
                    $"Training_{trainingId}_SelectedAfterFirstStep");
                entry.PlayerLpConsumedImageAssetId = DefaultIfEmpty(
                    entry.PlayerLpConsumedImageAssetId,
                    $"Training_{trainingId}_PlayerLpConsumed");
                entry.HeroineLpConsumedImageAssetId = DefaultIfEmpty(
                    entry.HeroineLpConsumedImageAssetId,
                    $"Training_{trainingId}_HeroineLpConsumed");
                entry.SimultaneousLpConsumedImageAssetId = DefaultIfEmpty(
                    entry.SimultaneousLpConsumedImageAssetId,
                    $"Training_{trainingId}_SimultaneousLpConsumed");
            }
        }

        private static string DefaultIfEmpty(string currentValue, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(currentValue) ? defaultValue : currentValue;
        }

        private IReadOnlyList<TrainingAssetTemplate> CreateTrainingAssetTemplates()
        {
            List<TrainingAssetTemplate> templates = new List<TrainingAssetTemplate>();
            IEnumerable<TrainingCatalogItem> catalogItems =
                SelectedProfile?.TrainingCatalog?.Items ?? Enumerable.Empty<TrainingCatalogItem>();
            foreach (TrainingCatalogItem item in catalogItems)
            {
                string trainingId = item?.TrainingId?.Trim();
                if (string.IsNullOrWhiteSpace(trainingId)) continue;
                string displayName = string.IsNullOrWhiteSpace(item.DisplayName)
                    ? trainingId
                    : item.DisplayName.Trim();
                templates.Add(new TrainingAssetTemplate(
                    $"Training_{trainingId}_SelectedBeforeFirstStep", trainingId,
                    displayName,
                    "SelectedBeforeFirstStep",
                    "visual novel training scene, both characters ready to begin, before first training step"));
                templates.Add(new TrainingAssetTemplate(
                    $"Training_{trainingId}_SelectedAfterFirstStep", trainingId,
                    displayName,
                    "SelectedAfterFirstStep",
                    "visual novel training scene, both characters actively training, after training has begun"));
                templates.Add(new TrainingAssetTemplate(
                    $"Training_{trainingId}_PlayerLpConsumed", trainingId,
                    displayName,
                    "PlayerLpConsumed",
                    "visual novel training scene, player exhausted after reaching the limit, heroine reacting clearly"));
                templates.Add(new TrainingAssetTemplate(
                    $"Training_{trainingId}_HeroineLpConsumed", trainingId,
                    displayName,
                    "HeroineLpConsumed",
                    "visual novel training scene, heroine exhausted after reaching her limit, player reacting clearly"));
                templates.Add(new TrainingAssetTemplate(
                    $"Training_{trainingId}_SimultaneousLpConsumed", trainingId,
                    displayName,
                    "SimultaneousLpConsumed",
                    "visual novel training scene, player and heroine both exhausted after simultaneously reaching their limits"));
            }
            return templates;
        }

        private sealed class TrainingAssetTemplate
        {
            public string AssetId { get; }
            public string TrainingId { get; }
            public string VisualState { get; }
            public string PositivePrompt { get; }
            public string DialogueMessage { get; }

            public TrainingAssetTemplate(
                string assetId,
                string trainingId,
                string displayName,
                string visualState,
                string positivePrompt)
            {
                AssetId = assetId;
                TrainingId = trainingId;
                VisualState = visualState;
                PositivePrompt = positivePrompt;
                DialogueMessage = BuildStandardTrainingDialogue(displayName, visualState);
            }
        }

        private static string BuildStandardTrainingDialogue(string trainingName, string visualState)
        {
            switch (visualState)
            {
                case "SelectedBeforeFirstStep": return trainingName + "を始めよう。準備はできてる？";
                case "SelectedAfterFirstStep": return trainingName + "はまだ続くよ。いい調子！";
                case "PlayerLpConsumed": return "無理しすぎないで。少し呼吸を整えよう？";
                case "HeroineLpConsumed": return "少し疲れたけど、まだ諦めないよ。";
                case "SimultaneousLpConsumed": return "二人とも限界まで頑張ったね。";
                default: return trainingName + "を続けよう。";
            }
        }

        private void EnsureLegacyTrainingCatalog()
        {
            SelectedProfile.TrainingCatalog ??= new TrainingCatalogSettings();
            SelectedProfile.TrainingCatalog.Items ??= new ObservableCollection<TrainingCatalogItem>();
            if (SelectedProfile.TrainingCatalog.Items.Count > 0)
            {
                return;
            }

            SelectedProfile.TrainingCatalog.Items.Add(new TrainingCatalogItem
            {
                TrainingId = "LightPractice",
                DisplayName = "軽い稽古",
                TrainingCategoryId = "Fundamentals",
                UnlockedByDefault = true
            });
            SelectedProfile.TrainingCatalog.Items.Add(new TrainingCatalogItem
            {
                TrainingId = "SparringPractice",
                DisplayName = "実戦形式",
                TrainingCategoryId = "Combat",
                UnlockedByDefault = true
            });
            SelectedProfile.TrainingCatalog.Items.Add(new TrainingCatalogItem
            {
                TrainingId = "EnduranceTraining",
                DisplayName = "持久訓練",
                TrainingCategoryId = "Endurance",
                UnlockedByDefault = true
            });
        }

        private IReadOnlyList<StillDefinition> GetHeroineBattleStandardDefinitions()
        {
            string[] assetIds =
            {
                "Battle_Heroine_Idle",
                "Battle_Heroine_Attack",
                "Battle_Heroine_Damage",
                "Battle_Heroine_Victory",
                "Battle_Heroine_Defeat"
            };

            return assetIds
                .Select(assetId => StillDefinitions.FirstOrDefault(definition => definition.AssetId == assetId))
                .Where(definition => definition != null)
                .ToList();
        }

        private void SavePromptRecordForHeroineBattleTemplate(HeroineAsset asset, StillDefinition definition)
        {
            PromptRecord record = promptRecordService.LoadOrCreatePromptRecord(SelectedProfile, asset);
            record.PositivePrompt = BuildStillPositivePrompt(SelectedProfile, definition);
            record.NegativePrompt = MergePromptParts(
                record.NegativePrompt,
                definition.NegativePromptAddition);
            record.RevisionMemo = definition.SpecificPrompt ?? string.Empty;
            promptRecordService.SavePromptRecord(SelectedProfile, asset, record);
        }

        public void SetImageSourceFromDroppedFiles(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
            {
                StatusMessage = "ドロップされた画像ファイルがありません。";
                return;
            }

            string imagePath = filePaths.FirstOrDefault(IsSupportedImageFile);
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                StatusMessage = "PNG/JPG/JPEG/WEBP の画像ファイルをドロップしてください。";
                return;
            }

            ImageSourcePathInput = imagePath;
            if (string.IsNullOrWhiteSpace(AssetIdInput))
            {
                AssetIdInput = Path.GetFileNameWithoutExtension(imagePath);
            }

            if (filePaths.Length > 1)
            {
                StatusMessage = $"複数ファイルがドロップされたため、先頭の画像を元画像に設定しました: {imagePath}";
            }
            else
            {
                StatusMessage = $"元画像を設定しました: {imagePath}";
            }
        }

        private static bool IsSupportedImageFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".png"
                || extension == ".jpg"
                || extension == ".jpeg"
                || extension == ".webp";
        }

        private void AddImageAsset()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                AddImageAssetCore();
            }
            catch (Exception ex)
            {
                StatusMessage = $"画像登録に失敗しました: {ex.Message}";
            }
        }

        private void UnregisterImageAsset()
        {
            if (SelectedProfile == null || SelectedAsset == null)
            {
                return;
            }

            HeroineAsset asset = SelectedAsset;
            MessageBoxResult result = MessageBox.Show(
                $"AssetId '{asset.AssetId}' の登録を解除しますか？\n画像ファイルと prompt JSON は削除されません。",
                "画像登録解除の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "画像登録解除をキャンセルしました。";
                return;
            }

            try
            {
                string storedPath = asset.StoredPath;
                string promptPath = asset.PromptRecordPath;
                bool unregistered = characterProjectService.UnregisterImageAsset(SelectedProfile, asset);
                if (!unregistered)
                {
                    StatusMessage = $"{asset.AssetId} は登録済み画像一覧に見つかりませんでした。";
                    return;
                }

                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                RefreshSelectedStillStatus();
                StatusMessage = $"{asset.AssetId} の登録を解除しました。画像ファイルと prompt JSON は残しています。画像: {storedPath} / prompt: {promptPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"画像登録解除に失敗しました: {ex.Message}";
            }
        }

        private HeroineAsset AddImageAssetCore()
        {
            bool hasExistingAssetId = HasExistingAssetId();
            bool hasExistingStoredFile = HasExistingStoredImageFile();
            bool overwriteExisting = ShouldOverwriteExistingAsset(hasExistingAssetId, hasExistingStoredFile);
            if (overwriteExisting == false && (hasExistingAssetId || hasExistingStoredFile))
            {
                StatusMessage = "画像登録をキャンセルしました。";
                return null;
            }

            HeroineAsset asset = characterProjectService.AddImageAsset(
                SelectedProfile,
                ImageSourcePathInput,
                SelectedAssetUsage,
                AssetIdInput,
                SelectedAssetStatus,
                overwriteExisting);

            SelectedAssetStatusFilter = asset.Status.ToString();
            RefreshFilteredAssets();
            RefreshAcceptedAssets();
            SelectedAsset = asset;
            // 同じAssetIdへの上書きでは選択参照が変わらず setter が通知しないため、
            // 登録直後に保存済み画像を明示的に読み直してプレビューを更新する。
            RefreshSelectedAssetImagePath();
            RefreshSelectedStillStatus();
            string registrationMessage = overwriteExisting
                ? $"{asset.AssetId} を {asset.Usage} に上書き登録しました。"
                : $"{asset.AssetId} を {asset.Usage} に登録しました。";
            StatusMessage = AppendImageInspectionMessage(registrationMessage, asset);
            return asset;
        }

        private bool HasExistingAssetId()
        {
            if (SelectedProfile == null || SelectedProfile.Assets == null || string.IsNullOrWhiteSpace(AssetIdInput))
            {
                return false;
            }

            string assetId = AssetIdInput.Trim();
            return SelectedProfile.Assets.Any(asset => asset.AssetId == assetId);
        }

        private bool HasExistingStoredImageFile()
        {
            string storedImagePath = BuildStoredImagePathForInput();
            return !string.IsNullOrWhiteSpace(storedImagePath) && File.Exists(storedImagePath);
        }

        private string BuildStoredImagePathForInput()
        {
            if (SelectedProfile == null || string.IsNullOrWhiteSpace(AssetIdInput))
            {
                return string.Empty;
            }

            string extension = Path.GetExtension(ImageSourcePathInput);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string fileName = AssetIdInput.Trim() + extension;
            return Path.Combine(
                characterProjectService.GetImageUsageDirectory(SelectedProfile.HeroineId, SelectedAssetUsage),
                fileName);
        }

        private bool ShouldOverwriteExistingAsset(bool hasExistingAssetId, bool hasExistingStoredFile)
        {
            if (!hasExistingAssetId && !hasExistingStoredFile)
            {
                return false;
            }

            string message = hasExistingAssetId
                ? $"AssetId '{AssetIdInput.Trim()}' はすでに登録されています。画像と登録情報を上書きしますか？"
                : $"AssetId '{AssetIdInput.Trim()}' の登録はありませんが、保存先画像ファイルが残っています。\n残っている画像ファイルを上書きして登録しますか？";

            MessageBoxResult result = MessageBox.Show(
                message,
                hasExistingAssetId ? "画像登録の上書き確認" : "残存画像ファイルの上書き確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        private void RefreshSelectedAssetImagePath()
        {
            SelectedAssetImagePath = string.Empty;

            if (SelectedProfile == null || SelectedAsset == null)
            {
                SelectedAssetImageMessage = "画像を選択してください。";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedAsset.StoredPath))
            {
                SelectedAssetImageMessage = "StoredPath が空です。";
                return;
            }

            string imagePath = Path.Combine(
                characterProjectService.GetCharacterDirectory(SelectedProfile.HeroineId),
                SelectedAsset.StoredPath);

            if (!File.Exists(imagePath))
            {
                SelectedAssetImageMessage = "画像ファイルが見つかりません: " + imagePath;
                return;
            }

            SelectedAssetImagePath = imagePath;
            SelectedAssetImageMessage = AppendImageInspectionMessage(imagePath, SelectedAsset);
        }

        private string AppendImageInspectionMessage(string baseMessage, HeroineAsset asset)
        {
            if (SelectedProfile == null || asset == null || string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                return baseMessage;
            }

            string imagePath = Path.Combine(
                characterProjectService.GetCharacterDirectory(SelectedProfile.HeroineId),
                asset.StoredPath);

            try
            {
                ImageInspectionResult result = imageInspectionService.Inspect(imagePath, asset.Usage);
                string summary = imageInspectionService.BuildSummary(result);
                if (result.Warnings.Count == 0)
                {
                    return $"{baseMessage} 検査: {summary}";
                }

                return $"{baseMessage} 検査: {summary} / 警告 {result.Warnings.Count} 件: {string.Join(" / ", result.Warnings)}";
            }
            catch (Exception ex)
            {
                return $"{baseMessage} 画像検査に失敗しました: {ex.Message}";
            }
        }

        private void SaveImageAssets()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                if (SelectedAsset != null)
                {
                    SelectedAsset.Usage = SelectedAssetUsage;
                    SelectedAsset.Status = SelectedAssetStatus;
                    SelectedAsset.SourcePath = ImageSourcePathInput ?? string.Empty;
                }
                characterProjectService.SaveProfile(SelectedProfile);
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                RefreshSelectedStillStatus();
                StatusMessage = $"{SelectedProfile.HeroineId} の画像情報を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"画像情報の保存に失敗しました: {ex.Message}";
            }
        }

        private void LoadPromptForSelectedAsset()
        {
            if (SelectedProfile == null || SelectedAsset == null)
            {
                CurrentPromptRecord = null;
                RefreshSelectedTrainingDialogueEntry();
                return;
            }

            try
            {
                CurrentPromptRecord = promptRecordService.LoadOrCreatePromptRecord(SelectedProfile, SelectedAsset);
            }
            catch (Exception ex)
            {
                CurrentPromptRecord = new PromptRecord();
                StatusMessage = $"prompt 読み込みに失敗しました: {ex.Message}";
            }
            RefreshSelectedTrainingDialogueEntry();
        }

        private void RefreshPromptTemplates()
        {
            AvailablePromptTemplates.Clear();
            SelectedPromptTemplate = null;

            foreach (PromptTemplate template in promptTemplateService.GetTemplates(SelectedPromptTemplateUsage))
            {
                AvailablePromptTemplates.Add(template);
            }

            if (AvailablePromptTemplates.Count > 0)
            {
                SelectedPromptTemplate = AvailablePromptTemplates[0];
            }
        }

        private void ImportLayeredSpritesFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "heroine_layered_sprites_from_unity.json を選択",
                Filter = "heroine_layered_sprites_from_unity.json|heroine_layered_sprites_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity レイヤー import をキャンセルしました。";
                return;
            }

            try
            {
                ImportLayeredSpritesFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity レイヤー import に失敗しました: {ex.Message}";
            }
        }

        private void ImportLayeredSpritesFromUnityFile(string filePath)
        {
            FromUnityLayeredSpriteDataFile data = LayeredSpriteSyncService.DeserializeFromUnity(
                File.ReadAllText(filePath));
            if (!string.IsNullOrWhiteSpace(data.HeroineId) &&
                !string.Equals(data.HeroineId, SelectedProfile.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"HeroineId が選択中のキャラクターと一致しません。JSON: {data.HeroineId} / Selected: {SelectedProfile.HeroineId}");
            }

            LayeredSpriteMergeResult result = LayeredSpriteSyncService.MergeFromUnity(
                LayerAssetDefinitions,
                CostumeDefinitions,
                ExpressionDefinitions,
                data);
            int registeredImages = 0;
            int missingImages = 0;
            foreach (FromUnityLayeredSpriteItem item in result.ImportedItems)
            {
                HeroineAsset existing = SelectedProfile.Assets?.FirstOrDefault(asset =>
                    asset != null && string.Equals(asset.AssetId, item.AssetId, StringComparison.OrdinalIgnoreCase));
                string existingPath = existing == null ? string.Empty : BuildStoredImagePath(existing);
                if (!string.IsNullOrWhiteSpace(existingPath) && File.Exists(existingPath))
                {
                    continue;
                }

                string sourcePath = ResolveUnityLayerImagePath(item.UnityImagePath, filePath);
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                {
                    missingImages++;
                    continue;
                }

                characterProjectService.AddImageAsset(
                    SelectedProfile,
                    sourcePath,
                    AssetUsage.Sprites,
                    item.AssetId,
                    AssetStatus.Accepted,
                    overwriteExisting: existing != null);
                registeredImages++;
            }

            definitionCatalogService.SaveExpressionDefinitionFile(ExpressionDefinitions);
            definitionCatalogService.SaveCostumeDefinitionFile(CostumeDefinitions);
            definitionCatalogService.SaveLayerAssetDefinitionFile(LayerAssetDefinitions);
            characterProjectService.SaveProfile(SelectedProfile);
            RefreshDefinitionCatalogOptions();
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            RefreshLayerPreview();
            RefreshProfilePreview();
            RefreshAcceptedAssets();
            RefreshProductionStatus();
            SelectedLayerAssetDefinition = LayerAssetDefinitions.FirstOrDefault();
            RefreshOutfitCompositionEditor();
            StatusMessage =
                $"FromUnity レイヤーを取り込みました。追加 {result.AddedCount}、更新 {result.UpdatedCount}、" +
                $"画像登録 {registeredImages}、画像未検出 {missingImages}、スキップ {result.SkippedCount}。";
        }

        private string ResolveUnityLayerImagePath(string unityImagePath, string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(unityImagePath))
            {
                return string.Empty;
            }
            if (Path.IsPathRooted(unityImagePath))
            {
                return Path.GetFullPath(unityImagePath);
            }
            if (AudioLibraryService.IsUnityProjectPath(UnityProjectPath))
            {
                return Path.GetFullPath(Path.Combine(UnityProjectPath, unityImagePath));
            }

            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(jsonPath) ?? string.Empty);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "ProjectSettings")))
                {
                    return Path.GetFullPath(Path.Combine(directory.FullName, unityImagePath));
                }
                directory = directory.Parent;
            }
            return string.Empty;
        }

        private void LoadDefinitionCatalog()
        {
            try
            {
                ExpressionDefinitions.Clear();
                foreach (ExpressionDefinition expression in definitionCatalogService.LoadExpressionDefinitionFile().Expressions)
                {
                    ExpressionDefinitions.Add(expression);
                }

                CostumeDefinitions.Clear();
                foreach (CostumeDefinition costume in definitionCatalogService.LoadCostumeDefinitionFile().Costumes)
                {
                    CostumeDefinitions.Add(costume);
                }

                LayerAssetDefinitions.Clear();
                foreach (LayerAssetDefinition layer in definitionCatalogService.LoadLayerAssetDefinitionFile().Layers)
                {
                    LayerAssetDefinitions.Add(layer);
                }

                SelectedExpressionDefinition = ExpressionDefinitions.FirstOrDefault();
                SelectedCostumeDefinition = CostumeDefinitions.FirstOrDefault();
                SelectedLayerAssetDefinition = LayerAssetDefinitions.FirstOrDefault();
                RefreshDefinitionCatalogOptions();
                DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
                RefreshLayerPreviewOptions();
                RefreshLayerPreview();
                RefreshConversationExpressionSuggestionsFromDefinitions();
                RefreshProductionStatus();
                StatusMessage = "差分定義を読み込みました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"差分定義の読み込みに失敗しました: {ex.Message}";
            }
        }

        private void ReloadDefinitionCatalog()
        {
            LoadDefinitionCatalog();
            stillDefinitionService = new StillDefinitionService(characterProjectService.WorkspaceRoot);
            LoadStillDefinitions();
        }

        private void SaveDefinitionCatalog()
        {
            try
            {
                RefreshDefinitionCatalogOptions();
                DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
                if (!string.IsNullOrWhiteSpace(DefinitionCatalogValidationMessage))
                {
                    StatusMessage = "差分定義に警告があります。保存前に修正してください。";
                    return;
                }

                definitionCatalogService.SaveExpressionDefinitionFile(ExpressionDefinitions);
                definitionCatalogService.SaveCostumeDefinitionFile(CostumeDefinitions);
                definitionCatalogService.SaveLayerAssetDefinitionFile(LayerAssetDefinitions);
                LoadDefinitionCatalog();
                stillDefinitionService = new StillDefinitionService(characterProjectService.WorkspaceRoot);
                LoadStillDefinitions();
                StatusMessage = "差分定義を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"差分定義の保存に失敗しました: {ex.Message}";
            }
        }

        private void AddExpressionDefinition()
        {
            ExpressionDefinition expression = new ExpressionDefinition
            {
                ExpressionId = BuildUniqueId(
                    "Expression",
                    ExpressionDefinitions.Where(item => item != null).Select(item => item.ExpressionId)),
                DisplayName = "新しい表情",
                Prompt = "neutral expression",
                UnityExpressionId = string.Empty
            };
            ExpressionDefinitions.Add(expression);
            SelectedExpressionDefinition = expression;
            RefreshDefinitionCatalogOptions();
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            StatusMessage = "表情定義を追加しました。";
        }

        private void RemoveExpressionDefinition()
        {
            if (SelectedExpressionDefinition == null)
            {
                return;
            }

            ExpressionDefinitions.Remove(SelectedExpressionDefinition);
            SelectedExpressionDefinition = ExpressionDefinitions.FirstOrDefault();
            RefreshDefinitionCatalogOptions();
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            RefreshLayerPreview();
            StatusMessage = "表情定義を削除しました。";
        }

        private void AddCostumeDefinition()
        {
            CostumeDefinition costume = new CostumeDefinition
            {
                CostumeId = BuildUniqueId(
                    "Costume",
                    CostumeDefinitions.Where(item => item != null).Select(item => item.CostumeId)),
                DisplayName = "新しい衣装",
                Prompt = "default outfit",
                UnityCostumeId = string.Empty
            };
            CostumeDefinitions.Add(costume);
            SelectedCostumeDefinition = costume;
            RefreshDefinitionCatalogOptions();
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            StatusMessage = "衣装定義を追加しました。";
        }

        private void RemoveCostumeDefinition()
        {
            if (SelectedCostumeDefinition == null)
            {
                return;
            }

            CostumeDefinitions.Remove(SelectedCostumeDefinition);
            SelectedCostumeDefinition = CostumeDefinitions.FirstOrDefault();
            RefreshDefinitionCatalogOptions();
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            RefreshLayerPreview();
            StatusMessage = "衣装定義を削除しました。";
        }

        private void AddLayerAssetDefinition()
        {
            string expressionId = SelectedExpressionDefinition?.ExpressionId ?? string.Empty;
            string costumeId = SelectedCostumeDefinition?.CostumeId ?? string.Empty;
            string layerKind = string.IsNullOrWhiteSpace(expressionId) ? "CostumeBody" : "HeadExpression";
            string baseId = layerKind == "HeadExpression"
                ? "Expression_" + (string.IsNullOrWhiteSpace(expressionId) ? "New" : expressionId)
                : "Costume_" + (string.IsNullOrWhiteSpace(costumeId) ? "New" : costumeId);
            LayerAssetDefinition layer = new LayerAssetDefinition
            {
                AssetId = BuildUniqueId(
                    baseId,
                    LayerAssetDefinitions.Where(item => item != null).Select(item => item.AssetId)),
                LayerKind = layerKind,
                CostumeId = layerKind == "CostumeBody" ? costumeId : string.Empty,
                ExpressionId = layerKind == "HeadExpression" ? expressionId : string.Empty,
                DisplayName = layerKind == "HeadExpression" ? "レイヤー: 頭・表情" : "レイヤー: 衣装・身体",
                FileName = baseId + ".png",
                DrawOrder = layerKind == "HeadExpression" ? 50 : 40,
                Prompt = "transparent background, isolated sprite layer"
            };
            LayerAssetDefinitions.Add(layer);
            SelectedLayerAssetDefinition = layer;
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            StatusMessage = "レイヤー素材定義を追加しました。";
        }

        private void RemoveLayerAssetDefinition()
        {
            if (SelectedLayerAssetDefinition == null)
            {
                return;
            }

            LayerAssetDefinitions.Remove(SelectedLayerAssetDefinition);
            SelectedLayerAssetDefinition = LayerAssetDefinitions.FirstOrDefault();
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            RefreshLayerPreview();
            StatusMessage = "レイヤー素材定義を削除しました。";
        }

        private void RefreshDefinitionCatalogOptions()
        {
            RefreshStringOptions(
                ExpressionIdOptions,
                ExpressionDefinitions.Where(expression => expression != null).Select(expression => expression.ExpressionId),
                includeEmpty: true);
            RefreshStringOptions(
                CostumeIdOptions,
                CostumeDefinitions.Where(costume => costume != null).Select(costume => costume.CostumeId),
                includeEmpty: true);
        }

        private void RefreshLayerPreviewOptions()
        {
            string previousBaseBody = SelectedLayerPreviewBaseBodyId;
            string previousCostume = SelectedLayerPreviewCostumeId;
            string previousExpression = SelectedLayerPreviewExpressionId;

            RefreshStringOptions(
                LayerPreviewBaseBodyOptions,
                LayerAssetDefinitions
                    .Where(layer => layer != null &&
                        (IsLayerKind(layer, "BackHair") || IsLayerKind(layer, "BaseBody")))
                    .Select(layer => layer.AssetId),
                includeEmpty: true);
            RefreshStringOptions(
                LayerPreviewCostumeOptions,
                LayerAssetDefinitions
                    .Where(layer => layer != null &&
                        (IsLayerKind(layer, "CostumeBody") || IsLayerKind(layer, "Costume")))
                    .Select(layer => layer.CostumeId),
                includeEmpty: true);
            RefreshStringOptions(
                LayerPreviewExpressionOptions,
                LayerAssetDefinitions
                    .Where(layer => layer != null &&
                        (IsLayerKind(layer, "HeadExpression") || IsLayerKind(layer, "Expression")))
                    .Select(layer => layer.ExpressionId),
                includeEmpty: true);

            selectedLayerPreviewBaseBodyId = SelectExistingOrFirst(previousBaseBody, LayerPreviewBaseBodyOptions);
            selectedLayerPreviewCostumeId = SelectPreferredOrFirst(previousCostume, "Default", LayerPreviewCostumeOptions);
            selectedLayerPreviewExpressionId = SelectPreferredOrFirst(previousExpression, "Neutral", LayerPreviewExpressionOptions);
            OnPropertyChanged(nameof(SelectedLayerPreviewBaseBodyId));
            OnPropertyChanged(nameof(SelectedLayerPreviewCostumeId));
            OnPropertyChanged(nameof(SelectedLayerPreviewExpressionId));
        }

        private void RefreshOutfitLayerAssetOptions()
        {
            string[] previous =
            {
                selectedOutfitCostumeBodyAssetId,
                selectedOutfitBackAccessoryAssetId,
                selectedOutfitFrontAccessoryAssetId
            };
            RefreshStringOptions(
                OutfitLayerAssetOptions,
                (SelectedProfile?.Assets ?? new ObservableCollection<HeroineAsset>())
                    .Where(asset => asset != null && asset.Usage == AssetUsage.Sprites &&
                        asset.Status == AssetStatus.Accepted)
                    .Select(asset => asset.AssetId),
                includeEmpty: true);

            // 現在設定中のIDは、画像が一時的に未登録でも選択表示を維持する。
            foreach (string id in previous.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                if (!OutfitLayerAssetOptions.Contains(id)) OutfitLayerAssetOptions.Add(id);
            }
        }

        private void RefreshOutfitCompositionEditor()
        {
            OutfitCompositionSelection selection = SelectedCostumeDefinition == null
                ? new OutfitCompositionSelection()
                : OutfitCompositionService.Read(
                    LayerAssetDefinitions,
                    SelectedCostumeDefinition.CostumeId);
            selectedOutfitCostumeBodyAssetId = selection.CostumeBodyAssetId;
            selectedOutfitBackAccessoryAssetId = selection.BackAccessoryAssetId;
            selectedOutfitFrontAccessoryAssetId = selection.FrontAccessoryAssetId;
            OnPropertyChanged(nameof(SelectedOutfitCostumeBodyAssetId));
            OnPropertyChanged(nameof(SelectedOutfitBackAccessoryAssetId));
            OnPropertyChanged(nameof(SelectedOutfitFrontAccessoryAssetId));
            RefreshOutfitLayerAssetOptions();
            RefreshOutfitCompositionPreview();
        }

        private void ApplyOutfitComposition()
        {
            if (SelectedProfile == null || SelectedCostumeDefinition == null)
            {
                return;
            }

            OutfitCompositionService.Apply(
                LayerAssetDefinitions,
                SelectedCostumeDefinition.CostumeId,
                new OutfitCompositionSelection
                {
                    CostumeBodyAssetId = SelectedOutfitCostumeBodyAssetId,
                    BackAccessoryAssetId = SelectedOutfitBackAccessoryAssetId,
                    FrontAccessoryAssetId = SelectedOutfitFrontAccessoryAssetId
                });
            definitionCatalogService.SaveLayerAssetDefinitionFile(LayerAssetDefinitions);
            DefinitionCatalogValidationMessage = BuildDefinitionCatalogValidationMessage();
            RefreshLayerPreviewOptions();
            selectedLayerPreviewCostumeId = SelectedCostumeDefinition.CostumeId;
            OnPropertyChanged(nameof(SelectedLayerPreviewCostumeId));
            RefreshLayerPreview();
            RefreshProfilePreview();
            RefreshOutfitCompositionEditor();
            RefreshProductionStatus();
            OutfitCompositionMessage = $"{SelectedCostumeDefinition.DisplayName} の服装セットを保存しました。";
        }

        private void AddOutfitAccessoryAssetTemplates()
        {
            if (SelectedProfile == null || SelectedCostumeDefinition == null)
            {
                return;
            }

            try
            {
                SelectedProfile.Assets ??= new ObservableCollection<HeroineAsset>();
                IReadOnlyList<OutfitAccessoryAssetTemplate> templates =
                    OutfitCompositionService.CreateAccessoryTemplates(SelectedCostumeDefinition.CostumeId);
                int addedCount = 0;
                HeroineAsset firstAsset = null;
                foreach (OutfitAccessoryAssetTemplate template in templates)
                {
                    HeroineAsset asset = SelectedProfile.Assets.FirstOrDefault(item => item != null &&
                        string.Equals(item.AssetId, template.AssetId, StringComparison.OrdinalIgnoreCase));
                    if (asset == null)
                    {
                        asset = new HeroineAsset
                        {
                            AssetId = template.AssetId,
                            Usage = AssetUsage.Sprites,
                            Status = AssetStatus.Pending,
                            FileName = template.AssetId + ".png",
                            PromptRecordPath = Path.Combine("Prompts", template.AssetId + ".prompt.json"),
                            Memo = template.Memo
                        };
                        SelectedProfile.Assets.Add(asset);
                        addedCount++;
                    }
                    else
                    {
                        asset.Usage = AssetUsage.Sprites;
                        if (string.IsNullOrWhiteSpace(asset.FileName)) asset.FileName = template.AssetId + ".png";
                        if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
                            asset.PromptRecordPath = Path.Combine("Prompts", template.AssetId + ".prompt.json");
                        if (string.IsNullOrWhiteSpace(asset.Memo)) asset.Memo = template.Memo;
                    }

                    PromptRecord prompt = promptRecordService.LoadOrCreatePromptRecord(SelectedProfile, asset);
                    if (string.IsNullOrWhiteSpace(prompt.PositivePrompt))
                    {
                        prompt.PositivePrompt = MergePromptParts(
                            SelectedProfile.AppearancePrompt,
                            SelectedProfile.StillCommonPositivePrompt,
                            template.Prompt);
                        prompt.RevisionMemo = template.Memo;
                        promptRecordService.SavePromptRecord(SelectedProfile, asset, prompt);
                    }
                    firstAsset ??= asset;
                }

                SelectedOutfitBackAccessoryAssetId = templates.First(item =>
                    item.LayerKind == "BackAccessory").AssetId;
                SelectedOutfitFrontAccessoryAssetId = templates.First(item =>
                    item.LayerKind == "FrontAccessory").AssetId;
                ApplyOutfitComposition();
                characterProjectService.SaveProfile(SelectedProfile);
                SelectedAssetStatusFilter = "All";
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                SelectedAsset = firstAsset;
                if (SelectedAsset != null)
                {
                    AssetIdInput = SelectedAsset.AssetId;
                    SelectedAssetUsage = SelectedAsset.Usage;
                    SelectedAssetStatus = SelectedAsset.Status;
                }
                OutfitCompositionMessage =
                    $"{SelectedCostumeDefinition.DisplayName} の前後アクセサリー画像枠を準備しました。追加 {addedCount} 件。";
            }
            catch (Exception ex)
            {
                OutfitCompositionMessage = $"アクセサリー画像枠の準備に失敗しました: {ex.Message}";
            }
        }

        private void RefreshOutfitCompositionPreview()
        {
            OutfitCompositionPreviewItems.Clear();
            if (SelectedProfile == null || SelectedCostumeDefinition == null)
            {
                OutfitCompositionMessage = "キャラクターと衣装を選択してください。";
                return;
            }

            Dictionary<string, HeroineAsset> assets = BuildAcceptedAssetDictionary();
            int missingCount = 0;
            AddOutfitCompositionPreviewItem(
                SelectedOutfitBackAccessoryAssetId, "BackAccessory", 10, assets, ref missingCount);
            AddOutfitCompositionPreviewItem(
                SelectedOutfitCostumeBodyAssetId, "CostumeBody", 40, assets, ref missingCount);
            AddOutfitCompositionPreviewItem(
                SelectedOutfitFrontAccessoryAssetId, "FrontAccessory", 60, assets, ref missingCount);
            OutfitCompositionMessage = OutfitCompositionPreviewItems.Count == 0
                ? "表示できる服装画像がありません。"
                : $"{OutfitCompositionPreviewItems.Count}枚を重ねて表示しています。" +
                    (missingCount > 0 ? $" 未登録または画像なし: {missingCount}件" : string.Empty);
        }

        private void AddOutfitCompositionPreviewItem(
            string assetId,
            string layerKind,
            int drawOrder,
            Dictionary<string, HeroineAsset> assets,
            ref int missingCount)
        {
            if (string.IsNullOrWhiteSpace(assetId)) return;
            if (!assets.TryGetValue(assetId, out HeroineAsset asset))
            {
                missingCount++;
                return;
            }
            string imagePath = BuildStoredImagePath(asset);
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                missingCount++;
                return;
            }
            OutfitCompositionPreviewItems.Add(new LayerPreviewItem
            {
                AssetId = assetId,
                DisplayName = asset.FileName,
                LayerKind = layerKind,
                DrawOrder = drawOrder,
                ImagePath = imagePath
            });
        }

        private void RefreshLayerPreview()
        {
            string selectedAssetId = SelectedLayerPreviewItem?.AssetId;
            LayerPreviewItems.Clear();

            if (SelectedProfile == null)
            {
                LayerPreviewMessage = "キャラクターを選択してください。";
                return;
            }

            List<LayerAssetDefinition> targetLayers = BuildSelectedLayerPreviewDefinitions();
            if (targetLayers.Count == 0)
            {
                LayerPreviewMessage = "プレビュー対象のレイヤーを選択してください。";
                return;
            }

            List<string> warnings = new List<string>();
            Dictionary<string, HeroineAsset> acceptedAssets = BuildAcceptedAssetDictionary();
            foreach (LayerAssetDefinition layer in targetLayers.OrderBy(layer => layer.DrawOrder))
            {
                if (!acceptedAssets.TryGetValue(layer.AssetId, out HeroineAsset asset))
                {
                    if (TryAddUnregisteredLayerPreview(layer))
                    {
                        warnings.Add(
                            $"{layer.AssetId}: 保存済み画像を未登録プレビューとして表示しています。" +
                            "Exportするには画像制作で登録して採用してください。");
                    }
                    else
                    {
                        warnings.Add($"{layer.AssetId}: Accepted 画像が登録されていません。");
                    }
                    continue;
                }

                string imagePath = BuildStoredImagePath(asset);
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                {
                    warnings.Add($"{layer.AssetId}: 画像ファイルが見つかりません。");
                    continue;
                }

                LayerPreviewItems.Add(new LayerPreviewItem
                {
                    AssetId = layer.AssetId,
                    DisplayName = layer.DisplayName,
                    LayerKind = layer.LayerKind,
                    DrawOrder = layer.DrawOrder,
                    ImagePath = imagePath
                });
            }

            if (LayerPreviewItems.Count == 0)
            {
                LayerPreviewMessage = warnings.Count == 0
                    ? "表示できるレイヤー画像がありません。"
                    : string.Join(Environment.NewLine, warnings);
                return;
            }

            string summary = $"{LayerPreviewItems.Count} 件のレイヤーを表示しています。";
            LayerPreviewMessage = warnings.Count == 0
                ? summary
                : summary + Environment.NewLine + string.Join(Environment.NewLine, warnings);
            SelectedLayerPreviewItem = LayerPreviewItems.FirstOrDefault(item =>
                string.Equals(item.AssetId, selectedAssetId, StringComparison.OrdinalIgnoreCase));
        }

        private bool CanMoveSelectedLayerPreview(int offset)
        {
            if (SelectedLayerPreviewItem == null || LayerPreviewItems.Count < 2) return false;

            int index = LayerPreviewItems.IndexOf(SelectedLayerPreviewItem);
            int targetIndex = index + offset;
            if (index < 0 || targetIndex < 0 || targetIndex >= LayerPreviewItems.Count) return false;

            return LayerAssetDefinitions.Any(layer => layer != null && string.Equals(
                layer.AssetId,
                SelectedLayerPreviewItem.AssetId,
                StringComparison.OrdinalIgnoreCase));
        }

        private void MoveSelectedLayerPreview(int offset)
        {
            if (!CanMoveSelectedLayerPreview(offset)) return;

            string selectedAssetId = SelectedLayerPreviewItem.AssetId;
            List<LayerPreviewItem> orderedItems = LayerPreviewItems
                .OrderBy(item => item.DrawOrder)
                .ToList();
            int currentIndex = orderedItems.FindIndex(item => string.Equals(
                item.AssetId,
                selectedAssetId,
                StringComparison.OrdinalIgnoreCase));
            int targetIndex = currentIndex + offset;
            LayerPreviewItem movedItem = orderedItems[currentIndex];
            orderedItems.RemoveAt(currentIndex);
            orderedItems.Insert(targetIndex, movedItem);

            for (int index = 0; index < orderedItems.Count; index++)
            {
                LayerAssetDefinition definition = LayerAssetDefinitions.FirstOrDefault(layer =>
                    layer != null && string.Equals(
                        layer.AssetId,
                        orderedItems[index].AssetId,
                        StringComparison.OrdinalIgnoreCase));
                if (definition != null)
                {
                    definition.DrawOrder = (index + 1) * 10;
                }
            }

            definitionCatalogService.SaveLayerAssetDefinitionFile(LayerAssetDefinitions);
            RefreshLayerPreview();
            SelectedLayerPreviewItem = LayerPreviewItems.FirstOrDefault(item => string.Equals(
                item.AssetId,
                selectedAssetId,
                StringComparison.OrdinalIgnoreCase));
            LayerPreviewMessage =
                $"{SelectedLayerPreviewItem?.DisplayName ?? selectedAssetId} の描画順を変更し、共通レイヤー定義へ保存しました。";
        }

        private bool TryAddUnregisteredLayerPreview(LayerAssetDefinition layer)
        {
            if (SelectedProfile == null || layer == null || string.IsNullOrWhiteSpace(layer.FileName))
            {
                return false;
            }

            string imagePath = Path.Combine(
                characterProjectService.GetImageUsageDirectory(SelectedProfile.HeroineId, AssetUsage.Sprites),
                layer.FileName);
            if (!File.Exists(imagePath))
            {
                return false;
            }

            LayerPreviewItems.Add(new LayerPreviewItem
            {
                AssetId = layer.AssetId,
                DisplayName = layer.DisplayName + "（未登録）",
                LayerKind = layer.LayerKind,
                DrawOrder = layer.DrawOrder,
                ImagePath = imagePath
            });
            return true;
        }

        private void RefreshProfilePreview()
        {
            ProfilePreviewItems.Clear();
            ProfilePreviewImagePath = string.Empty;

            if (SelectedProfile == null)
            {
                ProfilePreviewMessage = "キャラクターを選択してください。";
                return;
            }

            List<LayerPreviewItem> defaultLayerItems = BuildDefaultProfilePreviewItems();
            if (defaultLayerItems.Count > 0)
            {
                foreach (LayerPreviewItem item in defaultLayerItems)
                {
                    ProfilePreviewItems.Add(item);
                }

                ProfilePreviewMessage = "着せ替えデータの Default 衣装 / Neutral 表情を表示しています。";
                return;
            }

            HeroineAsset normalAsset = FindProfileFallbackImageAsset();
            if (normalAsset == null)
            {
                ProfilePreviewMessage = "表示できる通常立ち絵がありません。";
                return;
            }

            string imagePath = BuildStoredImagePath(normalAsset);
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                ProfilePreviewMessage = "通常立ち絵の画像ファイルが見つかりません。";
                return;
            }

            ProfilePreviewImagePath = imagePath;
            ProfilePreviewMessage = $"{normalAsset.AssetId} を表示しています。";
        }

        private List<LayerPreviewItem> BuildDefaultProfilePreviewItems()
        {
            List<LayerPreviewItem> items = new List<LayerPreviewItem>();
            Dictionary<string, HeroineAsset> profileAssets = BuildProfilePreviewAssetDictionary();

            LayerAssetDefinition baseBody = FindDefaultLayer(layer => IsLayerKind(layer, "BaseBody"));
            LayerAssetDefinition defaultCostume = FindDefaultLayer(layer => IsLayerKind(layer, "Costume")
                && string.Equals(layer.CostumeId, "Default", StringComparison.OrdinalIgnoreCase));
            LayerAssetDefinition neutralExpression = FindDefaultLayer(layer => IsLayerKind(layer, "Expression")
                && string.Equals(layer.ExpressionId, "Neutral", StringComparison.OrdinalIgnoreCase));

            if (baseBody == null || defaultCostume == null || neutralExpression == null)
            {
                return items;
            }

            if (!TryAddProfilePreviewItem(items, baseBody, profileAssets)
                || !TryAddProfilePreviewItem(items, defaultCostume, profileAssets)
                || !TryAddProfilePreviewItem(items, neutralExpression, profileAssets))
            {
                items.Clear();
                return items;
            }

            foreach (LayerAssetDefinition accessory in LayerAssetDefinitions
                .Where(layer => layer != null && IsOptionalEightLayerKind(layer) &&
                    MatchesLayerPreviewCondition(layer, "Default", "Neutral"))
                .OrderBy(layer => layer.DrawOrder))
            {
                TryAddProfilePreviewItem(items, accessory, profileAssets);
            }

            return items
                .GroupBy(item => item.AssetId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(item => item.DrawOrder)
                .ToList();
        }

        private LayerAssetDefinition FindDefaultLayer(Func<LayerAssetDefinition, bool> predicate)
        {
            return LayerAssetDefinitions
                .Where(layer => layer != null)
                .OrderBy(layer => layer.DrawOrder)
                .FirstOrDefault(predicate);
        }

        private bool TryAddProfilePreviewItem(
            List<LayerPreviewItem> items,
            LayerAssetDefinition layer,
            Dictionary<string, HeroineAsset> profileAssets)
        {
            if (layer == null
                || string.IsNullOrWhiteSpace(layer.AssetId)
                || !profileAssets.TryGetValue(layer.AssetId, out HeroineAsset asset))
            {
                return false;
            }

            string imagePath = BuildStoredImagePath(asset);
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                return false;
            }

            items.Add(new LayerPreviewItem
            {
                AssetId = layer.AssetId,
                DisplayName = layer.DisplayName,
                LayerKind = layer.LayerKind,
                DrawOrder = layer.DrawOrder,
                ImagePath = imagePath
            });
            return true;
        }

        private Dictionary<string, HeroineAsset> BuildProfilePreviewAssetDictionary()
        {
            if (SelectedProfile == null || SelectedProfile.Assets == null)
            {
                return new Dictionary<string, HeroineAsset>(StringComparer.OrdinalIgnoreCase);
            }

            return SelectedProfile.Assets
                .Where(asset => asset != null && !string.IsNullOrWhiteSpace(asset.AssetId))
                .OrderBy(asset => asset.Status == AssetStatus.Accepted ? 0 : 1)
                .GroupBy(asset => asset.AssetId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        }

        private HeroineAsset FindProfileFallbackImageAsset()
        {
            if (SelectedProfile == null || SelectedProfile.Assets == null)
            {
                return null;
            }

            return SelectedProfile.Assets
                .Where(asset => asset != null)
                .OrderBy(asset => GetProfileFallbackImagePriority(asset))
                .ThenBy(asset => asset.Status == AssetStatus.Accepted ? 0 : 1)
                .ThenBy(asset => asset.AssetId)
                .FirstOrDefault(asset => GetProfileFallbackImagePriority(asset) < int.MaxValue);
        }

        private static int GetProfileFallbackImagePriority(HeroineAsset asset)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.AssetId))
            {
                return int.MaxValue;
            }

            if (string.Equals(asset.AssetId, "Heroine_Normal", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (asset.Usage == AssetUsage.Sprites
                && asset.AssetId.IndexOf("normal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 1;
            }

            return asset.Usage == AssetUsage.Sprites ? 2 : int.MaxValue;
        }

        private List<LayerAssetDefinition> BuildSelectedLayerPreviewDefinitions()
        {
            List<LayerAssetDefinition> layers = new List<LayerAssetDefinition>();
            AddPreferredSelectedLayer(
                layers,
                layer => IsExactLayerKind(layer, "BackHair") &&
                    string.Equals(layer.AssetId, SelectedLayerPreviewBaseBodyId, StringComparison.OrdinalIgnoreCase),
                layer => IsExactLayerKind(layer, "BaseBody") &&
                    string.Equals(layer.AssetId, SelectedLayerPreviewBaseBodyId, StringComparison.OrdinalIgnoreCase));
            AddPreferredSelectedLayer(
                layers,
                layer => IsExactLayerKind(layer, "CostumeBody") &&
                    string.Equals(layer.CostumeId, SelectedLayerPreviewCostumeId, StringComparison.OrdinalIgnoreCase),
                layer => IsExactLayerKind(layer, "Costume") &&
                    string.Equals(layer.CostumeId, SelectedLayerPreviewCostumeId, StringComparison.OrdinalIgnoreCase));
            AddPreferredSelectedLayer(
                layers,
                layer => IsExactLayerKind(layer, "HeadExpression") &&
                    string.Equals(layer.ExpressionId, SelectedLayerPreviewExpressionId, StringComparison.OrdinalIgnoreCase),
                layer => IsExactLayerKind(layer, "Expression") &&
                    string.Equals(layer.ExpressionId, SelectedLayerPreviewExpressionId, StringComparison.OrdinalIgnoreCase));

            foreach (LayerAssetDefinition accessory in LayerAssetDefinitions
                .Where(layer => layer != null && IsOptionalEightLayerKind(layer) &&
                    MatchesLayerPreviewCondition(
                        layer,
                        SelectedLayerPreviewCostumeId,
                        SelectedLayerPreviewExpressionId))
                .OrderBy(layer => layer.DrawOrder))
            {
                layers.Add(accessory);
            }

            return layers
                .Where(layer => layer != null)
                .GroupBy(layer => layer.AssetId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(layer => layer.DrawOrder)
                .ToList();
        }

        private static bool IsOptionalEightLayerKind(LayerAssetDefinition layer)
        {
            return IsLayerKind(layer, "Accessory") || IsLayerKind(layer, "Background") ||
                IsLayerKind(layer, "BackAccessory") || IsLayerKind(layer, "FrontAccessory") ||
                IsLayerKind(layer, "FrontArm") || IsLayerKind(layer, "Effect");
        }

        private static bool MatchesLayerPreviewCondition(
            LayerAssetDefinition layer,
            string costumeId,
            string expressionId)
        {
            return layer != null &&
                (string.IsNullOrWhiteSpace(layer.CostumeId) ||
                    string.Equals(layer.CostumeId, costumeId, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(layer.ExpressionId) ||
                    string.Equals(layer.ExpressionId, expressionId, StringComparison.OrdinalIgnoreCase));
        }

        private void AddSelectedLayer(List<LayerAssetDefinition> layers, Func<LayerAssetDefinition, bool> predicate)
        {
            LayerAssetDefinition layer = LayerAssetDefinitions
                .Where(item => item != null)
                .OrderBy(item => item.DrawOrder)
                .FirstOrDefault(predicate);
            if (layer != null)
            {
                layers.Add(layer);
            }
        }

        private void AddPreferredSelectedLayer(
            List<LayerAssetDefinition> layers,
            Func<LayerAssetDefinition, bool> preferredPredicate,
            Func<LayerAssetDefinition, bool> fallbackPredicate)
        {
            LayerAssetDefinition preferred = LayerAssetDefinitions
                .Where(layer => layer != null)
                .OrderBy(layer => layer.DrawOrder)
                .FirstOrDefault(preferredPredicate);
            if (preferred != null)
            {
                layers.Add(preferred);
                return;
            }

            AddSelectedLayer(layers, fallbackPredicate);
        }

        private Dictionary<string, HeroineAsset> BuildAcceptedAssetDictionary()
        {
            if (SelectedProfile == null || SelectedProfile.Assets == null)
            {
                return new Dictionary<string, HeroineAsset>(StringComparer.OrdinalIgnoreCase);
            }

            return SelectedProfile.Assets
                .Where(asset => asset != null
                    && asset.Status == AssetStatus.Accepted
                    && !string.IsNullOrWhiteSpace(asset.AssetId))
                .GroupBy(asset => asset.AssetId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        }

        private string BuildStoredImagePath(HeroineAsset asset)
        {
            if (SelectedProfile == null || asset == null || string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                return string.Empty;
            }

            return Path.Combine(
                characterProjectService.GetCharacterDirectory(SelectedProfile.HeroineId),
                asset.StoredPath);
        }

        private static bool IsLayerKind(LayerAssetDefinition layer, string layerKind)
        {
            if (layer == null)
            {
                return false;
            }

            string actual = layer.LayerKind?.Trim() ?? string.Empty;
            if (string.Equals(actual, layerKind, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(layerKind, "BaseBody", StringComparison.OrdinalIgnoreCase))
                return string.Equals(actual, "BackHair", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(layerKind, "Costume", StringComparison.OrdinalIgnoreCase))
                return string.Equals(actual, "CostumeBody", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(layerKind, "Expression", StringComparison.OrdinalIgnoreCase))
                return string.Equals(actual, "HeadExpression", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(layerKind, "Accessory", StringComparison.OrdinalIgnoreCase))
                return string.Equals(actual, "BackAccessory", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(actual, "FrontAccessory", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private static bool IsExactLayerKind(LayerAssetDefinition layer, string layerKind)
        {
            return layer != null && string.Equals(
                layer.LayerKind?.Trim(),
                layerKind,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string SelectExistingOrFirst(string previousValue, ObservableCollection<string> options)
        {
            if (!string.IsNullOrWhiteSpace(previousValue)
                && options.Contains(previousValue, StringComparer.OrdinalIgnoreCase))
            {
                return previousValue;
            }

            return options.FirstOrDefault(option => !string.IsNullOrWhiteSpace(option))
                ?? options.FirstOrDefault()
                ?? string.Empty;
        }

        private static string SelectPreferredOrFirst(
            string previousValue,
            string preferredValue,
            ObservableCollection<string> options)
        {
            if (!string.IsNullOrWhiteSpace(previousValue) &&
                options.Contains(previousValue, StringComparer.OrdinalIgnoreCase))
            {
                return previousValue;
            }

            string preferred = options.FirstOrDefault(option =>
                string.Equals(option, preferredValue, StringComparison.OrdinalIgnoreCase));
            return preferred ?? SelectExistingOrFirst(string.Empty, options);
        }

        private static void RefreshStringOptions(ObservableCollection<string> target, IEnumerable<string> values, bool includeEmpty)
        {
            target.Clear();
            if (includeEmpty)
            {
                target.Add(string.Empty);
            }

            foreach (string value in values
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item))
            {
                target.Add(value);
            }
        }

        private string BuildDefinitionCatalogValidationMessage()
        {
            List<string> warnings = new List<string>();
            ValidateExpressionDefinitions(warnings);
            ValidateCostumeDefinitions(warnings);
            ValidateLayerAssetDefinitions(warnings);

            if (warnings.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(Environment.NewLine, warnings);
        }

        private void ValidateExpressionDefinitions(List<string> warnings)
        {
            foreach (ExpressionDefinition expression in ExpressionDefinitions)
            {
                if (expression == null)
                {
                    warnings.Add("表情定義: 空の定義があります。");
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(expression.ExpressionId)
                    ? "(ExpressionId 未入力)"
                    : expression.ExpressionId.Trim();
                if (string.IsNullOrWhiteSpace(expression.ExpressionId))
                {
                    warnings.Add("表情定義: ExpressionId が空です。");
                }

                if (string.IsNullOrWhiteSpace(expression.DisplayName))
                {
                    warnings.Add($"表情定義 {label}: 表示名が空です。");
                }

                if (string.IsNullOrWhiteSpace(expression.Prompt))
                {
                    warnings.Add($"表情定義 {label}: Prompt が空です。");
                }
            }

            AddDuplicateWarnings(
                warnings,
                "表情定義",
                "ExpressionId",
                ExpressionDefinitions.Where(expression => expression != null).Select(expression => expression.ExpressionId));
        }

        private void ValidateCostumeDefinitions(List<string> warnings)
        {
            foreach (CostumeDefinition costume in CostumeDefinitions)
            {
                if (costume == null)
                {
                    warnings.Add("衣装定義: 空の定義があります。");
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(costume.CostumeId)
                    ? "(CostumeId 未入力)"
                    : costume.CostumeId.Trim();
                if (string.IsNullOrWhiteSpace(costume.CostumeId))
                {
                    warnings.Add("衣装定義: CostumeId が空です。");
                }

                if (string.IsNullOrWhiteSpace(costume.DisplayName))
                {
                    warnings.Add($"衣装定義 {label}: 表示名が空です。");
                }

                if (string.IsNullOrWhiteSpace(costume.Prompt))
                {
                    warnings.Add($"衣装定義 {label}: Prompt が空です。");
                }
            }

            AddDuplicateWarnings(
                warnings,
                "衣装定義",
                "CostumeId",
                CostumeDefinitions.Where(costume => costume != null).Select(costume => costume.CostumeId));
        }

        private void ValidateLayerAssetDefinitions(List<string> warnings)
        {
            HashSet<string> expressionIds = new HashSet<string>(
                ExpressionDefinitions
                    .Where(expression => expression != null)
                    .Select(expression => expression.ExpressionId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim()),
                StringComparer.OrdinalIgnoreCase);
            HashSet<string> costumeIds = new HashSet<string>(
                CostumeDefinitions
                    .Where(costume => costume != null)
                    .Select(costume => costume.CostumeId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim()),
                StringComparer.OrdinalIgnoreCase);
            HashSet<string> layerKinds = new HashSet<string>(LayerKindOptions, StringComparer.OrdinalIgnoreCase);

            foreach (LayerAssetDefinition layer in LayerAssetDefinitions)
            {
                if (layer == null)
                {
                    warnings.Add("レイヤー素材定義: 空の定義があります。");
                    continue;
                }

                DefinitionCatalogService.NormalizeLayerAssetDefinition(layer);

                string label = string.IsNullOrWhiteSpace(layer.AssetId)
                    ? "(AssetId 未入力)"
                    : layer.AssetId.Trim();
                string layerKind = layer.LayerKind?.Trim() ?? string.Empty;
                string expressionId = layer.ExpressionId?.Trim() ?? string.Empty;
                string costumeId = layer.CostumeId?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(layer.AssetId))
                {
                    warnings.Add("レイヤー素材定義: AssetId が空です。");
                }

                if (string.IsNullOrWhiteSpace(layerKind))
                {
                    warnings.Add($"レイヤー素材定義 {label}: 種類が空です。");
                }
                else if (!layerKinds.Contains(layerKind))
                {
                    warnings.Add($"レイヤー素材定義 {label}: 種類 '{layerKind}' は候補外です。");
                }

                if (string.IsNullOrWhiteSpace(layer.DisplayName))
                {
                    warnings.Add($"レイヤー素材定義 {label}: 表示名が空です。");
                }

                if (string.IsNullOrWhiteSpace(layer.FileName))
                {
                    warnings.Add($"レイヤー素材定義 {label}: ファイル名が空です。");
                }
                else if (!layer.FileName.Trim().EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    warnings.Add($"レイヤー素材定義 {label}: ファイル名は .png を推奨します。");
                }

                if (string.IsNullOrWhiteSpace(layer.Prompt))
                {
                    warnings.Add($"レイヤー素材定義 {label}: Prompt が空です。");
                }

                if (!string.IsNullOrWhiteSpace(expressionId) && !expressionIds.Contains(expressionId))
                {
                    warnings.Add($"レイヤー素材定義 {label}: 表情ID '{expressionId}' が表情定義にありません。");
                }

                if (!string.IsNullOrWhiteSpace(costumeId) && !costumeIds.Contains(costumeId))
                {
                    warnings.Add($"レイヤー素材定義 {label}: 衣装ID '{costumeId}' が衣装定義にありません。");
                }

                if ((string.Equals(layerKind, "Expression", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(layerKind, "HeadExpression", StringComparison.OrdinalIgnoreCase))
                    && string.IsNullOrWhiteSpace(expressionId))
                {
                    warnings.Add($"レイヤー素材定義 {label}: HeadExpression / Expressionには表情IDが必要です。");
                }

                if ((string.Equals(layerKind, "Costume", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(layerKind, "CostumeBody", StringComparison.OrdinalIgnoreCase))
                    && string.IsNullOrWhiteSpace(costumeId))
                {
                    warnings.Add($"レイヤー素材定義 {label}: CostumeBody / Costumeには衣装IDが必要です。");
                }
            }

            AddDuplicateWarnings(
                warnings,
                "レイヤー素材定義",
                "AssetId",
                LayerAssetDefinitions.Where(layer => layer != null).Select(layer => layer.AssetId));
        }

        private static void AddDuplicateWarnings(
            List<string> warnings,
            string targetName,
            string keyName,
            IEnumerable<string> values)
        {
            foreach (string duplicate in values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key))
            {
                warnings.Add($"{targetName}: {keyName} '{duplicate}' が重複しています。");
            }
        }

        private void RefreshConversationExpressionSuggestionsFromDefinitions()
        {
            string previous = SelectedConversationExpressionSuggestion;
            ConversationExpressionSuggestions.Clear();

            IEnumerable<string> expressionIds = ExpressionDefinitions
                .Where(expression => expression != null)
                .Select(expression => expression.ExpressionId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (string expressionId in expressionIds)
            {
                ConversationExpressionSuggestions.Add(expressionId);
            }

            if (ConversationExpressionSuggestions.Count == 0)
            {
                foreach (string expressionId in ConversationValueCatalog.Expressions)
                {
                    ConversationExpressionSuggestions.Add(expressionId);
                }
            }

            SelectedConversationExpressionSuggestion = ConversationExpressionSuggestions.Contains(previous)
                ? previous
                : ConversationExpressionSuggestions.FirstOrDefault() ?? string.Empty;
        }

        private static string BuildUniqueId(string baseId, IEnumerable<string> existingIds)
        {
            string normalizedBaseId = string.IsNullOrWhiteSpace(baseId) ? "New" : baseId.Trim();
            HashSet<string> existing = new HashSet<string>(
                existingIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()),
                StringComparer.OrdinalIgnoreCase);

            if (!existing.Contains(normalizedBaseId))
            {
                return normalizedBaseId;
            }

            int index = 2;
            while (existing.Contains(normalizedBaseId + "_" + index))
            {
                index++;
            }

            return normalizedBaseId + "_" + index;
        }

        private void LoadStillDefinitions()
        {
            StillDefinitions.Clear();
            foreach (StillDefinition definition in stillDefinitionService.GetDefaultDefinitions())
            {
                StillDefinitions.Add(definition);
            }

            ApplyStillWorkItemsToDefinitions();
            RefreshStringOptions(BattleStillIdOptions, StillDefinitions.Where(x => x != null).Select(x => x.AssetId), includeEmpty: true);
            RefreshFilteredStillDefinitions();
        }

        private void RefreshFilteredAssets()
        {
            HeroineAsset previousSelection = SelectedAsset;
            FilteredAssets.Clear();
            RefreshTrainingAssets();

            if (SelectedProfile != null && SelectedProfile.Assets != null)
            {
                foreach (HeroineAsset asset in SelectedProfile.Assets.Where(MatchesAssetStatusFilter))
                {
                    FilteredAssets.Add(asset);
                }
            }

            if (previousSelection != null && FilteredAssets.Contains(previousSelection))
            {
                SelectedAsset = previousSelection;
                return;
            }

            SelectedAsset = FilteredAssets.Count > 0 ? FilteredAssets[0] : null;
        }

        private void RefreshTrainingAssets()
        {
            HeroineAsset previousSelection = selectedTrainingAsset;
            TrainingAssets.Clear();

            if (SelectedProfile?.Assets != null)
            {
                foreach (HeroineAsset asset in SelectedProfile.Assets.Where(asset => asset?.Usage == AssetUsage.Training))
                {
                    TrainingAssets.Add(asset);
                }
            }

            HeroineAsset nextSelection = previousSelection != null && TrainingAssets.Contains(previousSelection)
                ? previousSelection
                : TrainingAssets.FirstOrDefault();
            if (selectedTrainingAsset != nextSelection)
            {
                selectedTrainingAsset = nextSelection;
                OnPropertyChanged(nameof(SelectedTrainingAsset));
            }
        }

        private void RefreshAcceptedAssets()
        {
            AcceptedAssets.Clear();

            if (SelectedProfile == null || SelectedProfile.Assets == null)
            {
                return;
            }

            foreach (HeroineAsset asset in SelectedProfile.Assets.Where(asset => asset.Status == AssetStatus.Accepted))
            {
                AcceptedAssets.Add(asset);
            }

            RefreshLayerPreview();
            RefreshProfilePreview();
            RefreshOutfitLayerAssetOptions();
            RefreshOutfitCompositionPreview();
            RefreshFilteredConversationEntries();
        }

        private bool MatchesAssetStatusFilter(HeroineAsset asset)
        {
            if (asset == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedAssetStatusFilter) || SelectedAssetStatusFilter == "All")
            {
                return true;
            }

            return asset.Status.ToString() == SelectedAssetStatusFilter;
        }

        private void ApplyStillWorkItemsToDefinitions()
        {
            if (SelectedProfile == null || SelectedProfile.StillWorkItems == null)
            {
                return;
            }

            foreach (StillDefinition definition in StillDefinitions)
            {
                StillWorkItem workItem = SelectedProfile.StillWorkItems
                    .FirstOrDefault(item => item.AssetId == definition.AssetId);
                if (workItem == null)
                {
                    continue;
                }

                definition.Status = workItem.Status;
                if (!string.IsNullOrWhiteSpace(workItem.SpecificPrompt))
                {
                    definition.SpecificPrompt = workItem.SpecificPrompt;
                }

                if (!string.IsNullOrWhiteSpace(workItem.NegativePromptAddition))
                {
                    definition.NegativePromptAddition = workItem.NegativePromptAddition;
                }
            }
        }

        private void ApplySelectedStillWorkItem()
        {
            if (SelectedProfile == null || SelectedStillDefinition == null || SelectedProfile.StillWorkItems == null)
            {
                return;
            }

            StillWorkItem workItem = SelectedProfile.StillWorkItems
                .FirstOrDefault(item => item.AssetId == SelectedStillDefinition.AssetId);
            if (workItem == null)
            {
                return;
            }

            SelectedStillDefinition.Status = workItem.Status;
            if (!string.IsNullOrWhiteSpace(workItem.SpecificPrompt))
            {
                SelectedStillDefinition.SpecificPrompt = workItem.SpecificPrompt;
            }

            if (!string.IsNullOrWhiteSpace(workItem.NegativePromptAddition))
            {
                SelectedStillDefinition.NegativePromptAddition = workItem.NegativePromptAddition;
            }
        }

        private void UpdateStillWorkItemFromDefinition()
        {
            if (SelectedProfile == null || SelectedStillDefinition == null)
            {
                return;
            }

            SelectedProfile.StillWorkItems ??= new ObservableCollection<StillWorkItem>();

            StillWorkItem workItem = SelectedProfile.StillWorkItems
                .FirstOrDefault(item => item.AssetId == SelectedStillDefinition.AssetId);
            if (workItem == null)
            {
                workItem = new StillWorkItem
                {
                    AssetId = SelectedStillDefinition.AssetId
                };
                SelectedProfile.StillWorkItems.Add(workItem);
            }

            workItem.Status = SelectedStillDefinition.Status;
            workItem.SpecificPrompt = SelectedStillDefinition.SpecificPrompt ?? string.Empty;
            workItem.NegativePromptAddition = SelectedStillDefinition.NegativePromptAddition ?? string.Empty;
        }

        private void RefreshFilteredStillDefinitions()
        {
            StillDefinition previousSelection = SelectedStillDefinition;
            FilteredStillDefinitions.Clear();

            foreach (StillDefinition definition in StillDefinitions.Where(MatchesStillUsageFilter))
            {
                FilteredStillDefinitions.Add(definition);
            }

            if (previousSelection != null && FilteredStillDefinitions.Contains(previousSelection))
            {
                SelectedStillDefinition = previousSelection;
                return;
            }

            SelectedStillDefinition = FilteredStillDefinitions.Count > 0 ? FilteredStillDefinitions[0] : null;
        }

        private bool MatchesStillUsageFilter(StillDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(SelectedStillUsageFilter) || SelectedStillUsageFilter == "All")
            {
                return true;
            }

            return definition.Usage.ToString() == SelectedStillUsageFilter;
        }

        private void ApplyPromptTemplate()
        {
            if (SelectedProfile == null || SelectedPromptTemplate == null || CurrentPromptRecord == null)
            {
                return;
            }

            try
            {
                CurrentPromptRecord.PositivePrompt = promptTemplateService.BuildPositivePrompt(SelectedProfile, SelectedPromptTemplate);
                OnPropertyChanged(nameof(CurrentPromptRecord));
                StatusMessage = $"{SelectedPromptTemplate.DisplayName} を positive prompt に反映しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"テンプレート適用に失敗しました: {ex.Message}";
            }
        }

        private void ApplyStillPrompt()
        {
            if (SelectedProfile == null || SelectedStillDefinition == null)
            {
                return;
            }

            try
            {
                HeroineAsset asset = EnsureAssetForStill(SelectedProfile, SelectedStillDefinition);
                SelectedAsset = asset;
                if (CurrentPromptRecord == null)
                {
                    CurrentPromptRecord = promptRecordService.LoadOrCreatePromptRecord(SelectedProfile, asset);
                }

                CurrentPromptRecord.PositivePrompt = BuildStillPositivePrompt(SelectedProfile, SelectedStillDefinition);
                characterProjectService.SaveProfile(SelectedProfile);
                AssetIdInput = asset.AssetId;
                SelectedAssetUsage = asset.Usage;
                SelectedAssetStatusFilter = asset.Status.ToString();
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                SelectedAsset = asset;
                RefreshSelectedStillStatus();
                StatusMessage = $"{SelectedStillDefinition.DisplayName} の positive prompt を Prompt タブに反映しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"スチル prompt 反映に失敗しました: {ex.Message}";
            }
        }

        private void PrepareImageRegistrationForStill()
        {
            if (SelectedProfile == null || SelectedStillDefinition == null)
            {
                return;
            }

            AssetIdInput = SelectedStillDefinition.AssetId;
            SelectedAssetUsage = SelectedStillDefinition.Usage;
            SelectedAssetStatus = AssetStatus.Pending;

            HeroineAsset asset = FindAssetForStill(SelectedProfile, SelectedStillDefinition);
            if (asset != null)
            {
                SelectedAsset = asset;
                StatusMessage = $"{SelectedStillDefinition.DisplayName} の既存 Asset を選択しました。画像タブで状態確認または画像情報保存を行えます。";
            }
            else
            {
                StatusMessage = $"{SelectedStillDefinition.DisplayName} の画像登録欄を準備しました。画像タブで元画像を選択して登録してください。";
            }

            RefreshSelectedStillStatus();
        }

        private HeroineAsset EnsureAssetForStill(HeroineProfile profile, StillDefinition stillDefinition)
        {
            profile.Assets ??= new ObservableCollection<HeroineAsset>();

            HeroineAsset existingAsset = profile.Assets
                .FirstOrDefault(asset => asset.AssetId == stillDefinition.AssetId);
            if (existingAsset != null)
            {
                existingAsset.Usage = stillDefinition.Usage;
                if (string.IsNullOrWhiteSpace(existingAsset.FileName))
                {
                    existingAsset.FileName = stillDefinition.FileName;
                }

                if (string.IsNullOrWhiteSpace(existingAsset.PromptRecordPath))
                {
                    existingAsset.PromptRecordPath = Path.Combine("Prompts", stillDefinition.AssetId + ".prompt.json");
                }

                return existingAsset;
            }

            HeroineAsset asset = new HeroineAsset
            {
                AssetId = stillDefinition.AssetId,
                Usage = stillDefinition.Usage,
                Status = AssetStatus.Pending,
                FileName = stillDefinition.FileName,
                StoredPath = string.Empty,
                PromptRecordPath = Path.Combine("Prompts", stillDefinition.AssetId + ".prompt.json"),
                Memo = "スチル一覧から作成した prompt 作業用レコード"
            };

            profile.Assets.Add(asset);
            characterProjectService.SaveProfile(profile);
            return asset;
        }

        private static string BuildStillPositivePrompt(HeroineProfile profile, StillDefinition stillDefinition)
        {
            string[] promptParts =
            {
                NormalizePromptPart(profile.AppearancePrompt),
                NormalizePromptPart(profile.StillCommonPositivePrompt),
                NormalizePromptPart(stillDefinition.SpecificPrompt)
            };

            return string.Join(", ", promptParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string NormalizePromptPart(string prompt)
        {
            return (prompt ?? string.Empty).Trim().Trim(',').Trim();
        }

        private void SavePromptRecord()
        {
            if (SelectedProfile == null || SelectedAsset == null || CurrentPromptRecord == null)
            {
                return;
            }

            try
            {
                promptRecordService.SavePromptRecord(SelectedProfile, SelectedAsset, CurrentPromptRecord);
                characterProjectService.SaveProfile(SelectedProfile);
                RefreshSelectedStillStatus();
                StatusMessage = $"{SelectedAsset.AssetId} の prompt 記録を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"prompt 保存に失敗しました: {ex.Message}";
            }
        }

        private void SaveSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                characterProjectService.SaveProfile(SelectedProfile);
                RefreshStillPromptAfterProfilePromptChanged();
                RefreshSelectedStillStatus();
                ValidateMenuActions();
                RefreshProductionStatus();
                StatusMessage = $"{SelectedProfile.HeroineId} を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存に失敗しました: {ex.Message}";
            }
        }

        private void AddOutfitMessageOverride()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            SelectedProfile.OutfitMessageOverrides ??= new ObservableCollection<OutfitMessageOverride>();
            OutfitMessageOverride item = new OutfitMessageOverride
            {
                OutfitId = BuildUniqueId(
                    "Outfit",
                    SelectedProfile.OutfitMessageOverrides
                        .Where(overrideItem => overrideItem != null)
                        .Select(overrideItem => overrideItem.OutfitId)),
                LockedMessage = string.Empty,
                ChangedMessage = string.Empty
            };
            SelectedProfile.OutfitMessageOverrides.Add(item);
            SelectedOutfitMessageOverride = item;
            StatusMessage = "衣装メッセージ override を追加しました。";
        }

        private void RemoveOutfitMessageOverride()
        {
            if (SelectedProfile == null || SelectedOutfitMessageOverride == null)
            {
                return;
            }

            SelectedProfile.OutfitMessageOverrides ??= new ObservableCollection<OutfitMessageOverride>();
            SelectedProfile.OutfitMessageOverrides.Remove(SelectedOutfitMessageOverride);
            SelectedOutfitMessageOverride = SelectedProfile.OutfitMessageOverrides.FirstOrDefault();
            StatusMessage = "衣装メッセージ override を削除しました。";
        }

        private void AddOutfitReactionMessageOverride()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            SelectedProfile.OutfitReactionMessageOverrides ??= new ObservableCollection<OutfitReactionMessageOverride>();
            OutfitReactionMessageOverride item = new OutfitReactionMessageOverride
            {
                ReactionType = BuildUniqueId(
                    "Reaction",
                    SelectedProfile.OutfitReactionMessageOverrides
                        .Where(overrideItem => overrideItem != null)
                        .Select(overrideItem => overrideItem.ReactionType)),
                Message = string.Empty
            };
            SelectedProfile.OutfitReactionMessageOverrides.Add(item);
            SelectedOutfitReactionMessageOverride = item;
            StatusMessage = "衣装反応メッセージ override を追加しました。";
        }

        private void RemoveOutfitReactionMessageOverride()
        {
            if (SelectedProfile == null || SelectedOutfitReactionMessageOverride == null)
            {
                return;
            }

            SelectedProfile.OutfitReactionMessageOverrides ??= new ObservableCollection<OutfitReactionMessageOverride>();
            SelectedProfile.OutfitReactionMessageOverrides.Remove(SelectedOutfitReactionMessageOverride);
            SelectedOutfitReactionMessageOverride = SelectedProfile.OutfitReactionMessageOverrides.FirstOrDefault();
            StatusMessage = "衣装反応メッセージ override を削除しました。";
        }

        private void ImportHeroineProfileFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "heroine_profile_from_unity.json を選択",
                Filter = "heroine_profile_from_unity.json|heroine_profile_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity profile import をキャンセルしました。";
                return;
            }

            try
            {
                ImportHeroineProfileFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity profile import に失敗しました: {ex.Message}";
            }
        }

        private void ImportHeroineProfileFromUnityFile(string filePath)
        {
            if (SelectedProfile == null)
            {
                return;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            FromUnityHeroineProfileData profileData = JsonSerializer.Deserialize<FromUnityHeroineProfileData>(
                File.ReadAllText(filePath),
                options);

            if (profileData == null)
            {
                throw new InvalidOperationException("heroine_profile_from_unity.json を読み込めませんでした。");
            }

            if (profileData.SchemaVersion != 1)
            {
                throw new InvalidOperationException($"未対応の schemaVersion です: {profileData.SchemaVersion}");
            }

            if (!string.IsNullOrWhiteSpace(profileData.HeroineId)
                && !string.Equals(profileData.HeroineId, SelectedProfile.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"HeroineId が選択中のキャラクターと一致しません。JSON: {profileData.HeroineId} / Selected: {SelectedProfile.HeroineId}");
            }

            ApplyOptionalProfileText(profileData.DisplayName, value => SelectedProfile.DisplayName = value);
            ApplyOptionalProfileText(profileData.HeroineFirstPerson, value => SelectedProfile.FirstPerson = value);
            ApplyOptionalProfileText(profileData.PlayerSecondPerson, value => SelectedProfile.SecondPerson = value);
            ApplyOptionalProfileText(profileData.InitialDialogueMessage, value => SelectedProfile.InitialDialogueMessage = value);
            ApplyOptionalProfileText(profileData.NextActionPrompt, value => SelectedProfile.NextActionPrompt = value);
            ApplyOptionalProfileText(profileData.MorningGreeting, value => SelectedProfile.MorningGreeting = value);
            ApplyOptionalProfileText(profileData.GoodNightGreeting, value => SelectedProfile.GoodNightGreeting = value);
            ApplyOptionalProfileText(profileData.GameStartFallbackMessage, value => SelectedProfile.GameStartFallbackMessage = value);
            ApplyOptionalProfileText(profileData.GameStartFollowUpMessage, value => SelectedProfile.GameStartFollowUpMessage = value);
            ApplyOptionalProfileText(profileData.ConversationResourcePath, value => SelectedProfile.ConversationResourcePath = value);
            ApplyOptionalProfileText(profileData.GameEventResourcePath, value => SelectedProfile.GameEventResourcePath = value);
            ApplyOptionalProfileText(profileData.ActionResourcePath, value => SelectedProfile.ActionResourcePath = value);
            ApplyOptionalProfileText(profileData.ScheduledEventResourcePath, value => SelectedProfile.ScheduledEventResourcePath = value);
            ApplyOptionalProfileText(profileData.BattleResultEventResourcePath, value => SelectedProfile.BattleResultEventResourcePath = value);
            ApplyOptionalProfileText(profileData.BattlePanelResultMessageResourcePath, value => SelectedProfile.BattlePanelResultMessageResourcePath = value);
            ApplyOptionalProfileText(profileData.EndingResourcePath, value => SelectedProfile.EndingResourcePath = value);

            SelectedProfile.OutfitMessageOverrides ??= new ObservableCollection<OutfitMessageOverride>();
            SelectedProfile.OutfitReactionMessageOverrides ??= new ObservableCollection<OutfitReactionMessageOverride>();
            SelectedProfile.BattleSkills ??= new ObservableCollection<HeroineBattleSkill>();
            int addedOutfitCount = 0;
            int skippedOutfitCount = 0;
            int addedReactionCount = 0;
            int skippedReactionCount = 0;

            if (profileData.OutfitMessageOverrides != null)
            {
                SelectedProfile.OutfitMessageOverrides.Clear();
                foreach (OutfitMessageOverride item in profileData.OutfitMessageOverrides)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.OutfitId))
                    {
                        skippedOutfitCount++;
                        continue;
                    }

                    SelectedProfile.OutfitMessageOverrides.Add(new OutfitMessageOverride
                    {
                        OutfitId = item.OutfitId.Trim(),
                        LockedMessage = item.LockedMessage ?? string.Empty,
                        ChangedMessage = item.ChangedMessage ?? string.Empty
                    });
                    addedOutfitCount++;
                }
            }

            if (profileData.OutfitReactionMessageOverrides != null)
            {
                SelectedProfile.OutfitReactionMessageOverrides.Clear();
                foreach (OutfitReactionMessageOverride item in profileData.OutfitReactionMessageOverrides)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.ReactionType))
                    {
                        skippedReactionCount++;
                        continue;
                    }

                    SelectedProfile.OutfitReactionMessageOverrides.Add(new OutfitReactionMessageOverride
                    {
                        ReactionType = item.ReactionType.Trim(),
                        Message = item.Message ?? string.Empty
                    });
                    addedReactionCount++;
                }
            }

            BattleSkillSyncService.ApplyImportedValues(SelectedProfile, profileData.BattleSkills);

            string heroineSkillsPath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, "heroine_skills_from_unity.json");
            bool importedHeroineSkills = File.Exists(heroineSkillsPath);
            if (importedHeroineSkills)
            {
                HeroineSkillTreeSyncService.ApplyImportedValues(
                    SelectedProfile,
                    HeroineSkillTreeSyncService.Deserialize(File.ReadAllText(heroineSkillsPath)));
            }

            string fromUnityFolder = Path.GetDirectoryName(filePath) ?? string.Empty;
            string battleEventsPath = Path.Combine(fromUnityFolder, "battle_result_events_from_unity.json");
            string battlePanelMessagesPath = Path.Combine(fromUnityFolder, "battle_panel_result_messages_from_unity.json");
            BattleResultEventEntry[] beforeBattleEvents = SelectedProfile.BattleMessages?.ResultEvents?.ToArray()
                ?? Array.Empty<BattleResultEventEntry>();
            BattlePanelResultMessageEntry[] beforePanelMessages = SelectedProfile.BattleMessages?.PanelMessages?.ToArray()
                ?? Array.Empty<BattlePanelResultMessageEntry>();
            bool importedBattleMessages = false;
            if (File.Exists(battleEventsPath))
            {
                BattleMessageSyncService.ApplyResultEvents(
                    SelectedProfile,
                    BattleMessageSyncService.DeserializeResultEvents(File.ReadAllText(battleEventsPath)));
                importedBattleMessages = true;
            }

            if (File.Exists(battlePanelMessagesPath))
            {
                BattleMessageSyncService.ApplyPanelMessages(
                    SelectedProfile,
                    BattleMessageSyncService.DeserializePanelMessages(File.ReadAllText(battlePanelMessagesPath)));
                importedBattleMessages = true;
            }

            BattleMessageChangeSummary battleSummary = BattleMessageSyncService.AnalyzeChanges(
                beforeBattleEvents,
                SelectedProfile.BattleMessages?.ResultEvents,
                beforePanelMessages,
                SelectedProfile.BattleMessages?.PanelMessages);
            List<string> battleFiles = new List<string>();
            if (File.Exists(battleEventsPath)) battleFiles.Add(Path.GetFileName(battleEventsPath));
            if (File.Exists(battlePanelMessagesPath)) battleFiles.Add(Path.GetFileName(battlePanelMessagesPath));
            int skippedBattleFiles = 2 - battleFiles.Count;
            LastBattleMessageImportReport = importedBattleMessages
                ? $"読込: {string.Join(", ", battleFiles)}\n" +
                  $"戦闘結果: 追加 {battleSummary.ResultAdded} / 更新 {battleSummary.ResultUpdated} / 削除 {battleSummary.ResultDeleted} / 維持 {battleSummary.ResultUnchanged}\n" +
                  $"戦闘パネル文: 追加 {battleSummary.PanelAdded} / 更新 {battleSummary.PanelUpdated} / 削除 {battleSummary.PanelDeleted} / 維持 {battleSummary.PanelUnchanged}\n" +
                  $"詳細変更: 話者 {battleSummary.SpeakerChanged} / 表情 {battleSummary.ExpressionChanged} / 表示方式 {battleSummary.VisualModeChanged}" +
                  (skippedBattleFiles > 0 ? $"\nスキップ: 同じフォルダに存在しない戦闘メッセージファイル {skippedBattleFiles} 件" : string.Empty)
                : "戦闘メッセージ用FromUnity JSONが同じフォルダにないため、戦闘文は維持しました。";

            characterProjectService.SaveProfile(SelectedProfile);
            SelectedOutfitMessageOverride = SelectedProfile.OutfitMessageOverrides.FirstOrDefault();
            SelectedOutfitReactionMessageOverride = SelectedProfile.OutfitReactionMessageOverrides.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedProfile));
            RefreshProductionStatus();
            StatusMessage = $"FromUnity profile を取り込みました。衣装 {addedOutfitCount} 件追加/{skippedOutfitCount} 件スキップ、反応 {addedReactionCount} 件追加/{skippedReactionCount} 件スキップ、ヒロインスキル {(importedHeroineSkills ? "更新" : "維持")}、戦闘文 {(importedBattleMessages ? "更新" : "維持")}。";
        }

        private static void ApplyOptionalProfileText(string value, Action<string> apply)
        {
            if (value != null)
            {
                apply(value);
            }
        }

        private void ExportSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                characterProjectService.SaveProfile(SelectedProfile);
                ExportValidationResult validation = exportService.Validate(SelectedProfile);
                if (validation.ErrorCount > 0)
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"Export前検証で Error {validation.ErrorCount} 件、Warning {validation.WarningCount} 件が見つかりました。\n" +
                        "Errorがあるため一部データや画像を出力できない可能性があります。続行しますか？",
                        "Export前検証",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                    {
                        RefreshProductionStatus();
                        SelectedMainTabIndex = 14;
                        StatusMessage = "Exportを中止しました。制作状況のExport準備から修正対象を確認してください。";
                        return;
                    }
                }
                LastExportReport = exportService.ExportHeroine(SelectedProfile);
                StatusMessage = $"{SelectedProfile.HeroineId} を export しました。画像 {LastExportReport.ExportedImageCount}/{LastExportReport.AcceptedAssetCount} 件、prompt {LastExportReport.ExportedPromptCount} 件、会話データ {LastExportReport.TotalConversationDataCount} 件、警告 {LastExportReport.Warnings.Count} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"export に失敗しました: {ex.Message}";
            }
        }

        private void AddConversationEntry()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            SelectedProfile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            ConversationEntry entry = CreateConversationEntry(SelectedConversationDataKind, SelectedProfile.ConversationEntries);
            SelectedProfile.ConversationEntries.Add(entry);
            RefreshFilteredConversationEntries();
            SelectedConversationEntry = entry;
            StatusMessage = $"{GetConversationKindDisplayName(SelectedConversationDataKind)}を追加しました。";
        }

        private void RemoveConversationEntry()
        {
            if (SelectedProfile == null || SelectedConversationEntry == null)
            {
                return;
            }

            ConversationEntry entry = SelectedConversationEntry;
            SelectedProfile.ConversationEntries.Remove(entry);
            RefreshFilteredConversationEntries();
            SelectedConversationEntry = FilteredConversationEntries.FirstOrDefault();
            StatusMessage = $"{entry.Id} を削除しました。保存すると profile.json に反映されます。";
        }

        private void AddConversationLine()
        {
            if (SelectedConversationEntry == null)
            {
                return;
            }

            SelectedConversationEntry.Lines ??= new ObservableCollection<ConversationLine>();
            ConversationLine line = new ConversationLine();
            SelectedConversationEntry.Lines.Add(line);
            SelectedConversationLine = line;
            OnPropertyChanged(nameof(SelectedConversationEntry));
            StatusMessage = "台詞行を追加しました。";
        }

        private void RemoveConversationLine()
        {
            if (SelectedConversationEntry == null || SelectedConversationLine == null)
            {
                return;
            }

            ConversationLine line = SelectedConversationLine;
            SelectedConversationEntry.Lines.Remove(line);
            SelectedConversationLine = SelectedConversationEntry.Lines.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedConversationEntry));
            StatusMessage = "台詞行を削除しました。保存すると profile.json に反映されます。";
        }

        private void AddConversationChoice()
        {
            if (SelectedConversationEntry == null)
            {
                return;
            }

            SelectedConversationEntry.Choices ??= new ObservableCollection<ConversationChoice>();
            ConversationChoice choice = new ConversationChoice();
            SelectedConversationEntry.Choices.Add(choice);
            SelectedConversationChoice = choice;
            OnPropertyChanged(nameof(SelectedConversationEntry));
            StatusMessage = "選択肢を追加しました。";
        }

        private void RemoveConversationChoice()
        {
            if (SelectedConversationEntry == null || SelectedConversationChoice == null)
            {
                return;
            }

            ConversationChoice choice = SelectedConversationChoice;
            SelectedConversationEntry.Choices.Remove(choice);
            SelectedConversationChoice = SelectedConversationEntry.Choices.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedConversationEntry));
            StatusMessage = "選択肢を削除しました。保存すると profile.json に反映されます。";
        }

        private void SaveConversationData()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                characterProjectService.SaveProfile(SelectedProfile);
                RefreshProductionStatus();
                StatusMessage = $"{SelectedProfile.HeroineId} の会話データを保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"会話データ保存に失敗しました: {ex.Message}";
            }
        }

        private void PrepareStandardMenuActions()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                int added = MenuActionDefinitionService.AddMissingStandardActions(SelectedProfile);
                characterProjectService.SaveProfile(SelectedProfile);
                OnPropertyChanged(nameof(SelectedProfile));
                ValidateMenuActions();
                StatusMessage = added > 0
                    ? $"標準メニュー項目を {added} 件追加して保存しました。既存項目は維持されています。"
                    : "標準メニュー項目はすべて登録済みです。既存項目は変更していません。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"標準メニュー項目の準備に失敗しました: {ex.Message}";
            }
        }

        private void ApplyStandardMenuLayout()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "標準13項目の表示名・配置列・表示順・実行種別を標準値へ戻します。\n" +
                "独自に追加した項目は変更しません。実行しますか？",
                "標準メニュー配置へ戻す",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "標準メニュー配置の適用をキャンセルしました。";
                return;
            }

            int updated = MenuActionDefinitionService.ApplyStandardLayout(SelectedProfile);
            characterProjectService.SaveProfile(SelectedProfile);
            OnPropertyChanged(nameof(SelectedProfile));
            SelectedMenuAction = SelectedProfile.MenuActions.FirstOrDefault();
            ValidateMenuActions();
            RefreshProductionStatus();
            StatusMessage = $"標準メニュー配置を適用して保存しました。更新: {updated} 件。";
        }

        private void AddMenuAction()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            SelectedProfile.MenuActions ??= new ObservableCollection<MenuActionDefinition>();
            MenuActionDefinition action = new MenuActionDefinition
            {
                ActionId = "NewAction",
                DisplayName = "新しい項目",
                DisplayColumn = 0,
                SortOrder = SelectedProfile.MenuActions.Count == 0
                    ? 10
                    : SelectedProfile.MenuActions.Max(x => x?.SortOrder ?? 0) + 10,
                ExecutionType = "SimpleAction",
                IsEnabled = true,
                IsRequired = false
            };
            SelectedProfile.MenuActions.Add(action);
            SelectedMenuAction = action;
            ValidateMenuActions();
            StatusMessage = "メニュー項目を追加しました。編集後に保存してください。";
        }

        private void RemoveMenuAction()
        {
            if (SelectedProfile?.MenuActions == null || SelectedMenuAction == null)
            {
                return;
            }

            string actionId = SelectedMenuAction.ActionId;
            SelectedProfile.MenuActions.Remove(SelectedMenuAction);
            SelectedMenuAction = SelectedProfile.MenuActions.FirstOrDefault();
            ValidateMenuActions();
            StatusMessage = $"メニュー項目 `{actionId}` を削除しました。保存すると確定します。";
        }

        private void ValidateMenuActions()
        {
            MenuActionWarnings.Clear();
            if (SelectedProfile == null)
            {
                return;
            }

            foreach (string warning in MenuActionDefinitionService.Validate(SelectedProfile))
            {
                MenuActionWarnings.Add(warning);
            }

            StatusMessage = MenuActionWarnings.Count == 0
                ? "メニュー設定に問題はありません。"
                : $"メニュー設定に {MenuActionWarnings.Count} 件の確認事項があります。";
        }

        private void ImportActionsFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "actions_from_unity.json を選択",
                Filter = "actions_from_unity.json|actions_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity actions import をキャンセルしました。";
                return;
            }

            try
            {
                ImportActionsFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity actions import に失敗しました: {ex.Message}";
            }
        }

        private void ImportActionsFromUnityFile(string filePath)
        {
            if (SelectedProfile == null)
            {
                return;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            FromUnityActionDataFile actionData = JsonSerializer.Deserialize<FromUnityActionDataFile>(
                File.ReadAllText(filePath),
                options);

            if (actionData == null)
            {
                throw new InvalidOperationException("actions_from_unity.json を読み込めませんでした。");
            }

            if (actionData.SchemaVersion != 1)
            {
                throw new InvalidOperationException($"未対応の schemaVersion です: {actionData.SchemaVersion}");
            }

            if (!string.IsNullOrWhiteSpace(actionData.HeroineId)
                && !string.Equals(actionData.HeroineId, SelectedProfile.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"HeroineId が選択中のキャラクターと一致しません。JSON: {actionData.HeroineId} / Selected: {SelectedProfile.HeroineId}");
            }

            SelectedProfile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            List<ConversationEntry> importedEntries = new List<ConversationEntry>();
            int skippedCount = 0;

            foreach (FromUnityActionDataItem item in actionData.Items ?? new List<FromUnityActionDataItem>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    skippedCount++;
                    continue;
                }

                string actionId = item.Id.Trim();
                List<FromUnityActionReaction> reactions = item.Reactions ?? new List<FromUnityActionReaction>();
                if (reactions.Count == 0)
                {
                    if (HasExistingActionReaction(actionId, null))
                    {
                        skippedCount++;
                        continue;
                    }

                    ConversationEntry fallbackEntry = CreateActionReactionFromUnityAction(item);
                    SelectedProfile.ConversationEntries.Add(fallbackEntry);
                    importedEntries.Add(fallbackEntry);
                    continue;
                }

                foreach (FromUnityActionReaction reaction in reactions)
                {
                    if (reaction == null || HasExistingActionReaction(actionId, reaction.Id))
                    {
                        skippedCount++;
                        continue;
                    }

                    ConversationEntry entry = CreateActionReactionFromUnityReaction(item, reaction);
                    SelectedProfile.ConversationEntries.Add(entry);
                    importedEntries.Add(entry);
                }
            }

            characterProjectService.SaveProfile(SelectedProfile);
            SelectedConversationDataKind = ConversationDataKind.ActionReactions;
            RefreshConversationCategorySuggestions();
            RefreshFilteredConversationEntries();
            if (importedEntries.Count > 0)
            {
                SelectedConversationEntry = importedEntries[0];
            }

            StatusMessage = $"FromUnity actions を取り込みました。追加 {importedEntries.Count} 件、スキップ {skippedCount} 件。";
        }

        private bool HasExistingActionReaction(string actionId, string reactionId)
        {
            if (SelectedProfile == null || SelectedProfile.ConversationEntries == null)
            {
                return false;
            }

            string generatedId = string.IsNullOrWhiteSpace(reactionId)
                ? CreateActionReactionEntryId(actionId)
                : reactionId.Trim();
            return SelectedProfile.ConversationEntries.Any(entry => entry != null
                && entry.Kind == ConversationDataKind.ActionReactions
                && string.Equals(entry.Id, generatedId, StringComparison.OrdinalIgnoreCase));
        }

        private static ConversationEntry CreateActionReactionFromUnityReaction(
            FromUnityActionDataItem action,
            FromUnityActionReaction reaction)
        {
            FromUnityActionReactionCondition condition = reaction.Conditions ?? new FromUnityActionReactionCondition();
            ConversationEntry entry = new ConversationEntry
            {
                Kind = ConversationDataKind.ActionReactions,
                Id = string.IsNullOrWhiteSpace(reaction.Id)
                    ? CreateActionReactionEntryId(action.Id.Trim())
                    : reaction.Id.Trim(),
                Title = string.IsNullOrWhiteSpace(action.DisplayName) ? action.Id.Trim() : action.DisplayName.Trim(),
                Category = string.IsNullOrWhiteSpace(action.Category) ? action.Id.Trim() : action.Category.Trim(),
                ImageAssetIdsText = JoinImportList(reaction.ImageAssetIds),
                Priority = reaction.Priority,
                Memo = BuildFromUnityActionMemo(action),
                Conditions = new ConversationCondition
                {
                    ActionId = action.Id.Trim(),
                    MinAffection = condition.MinAffection,
                    MaxAffection = condition.MaxAffection > 0 ? condition.MaxAffection : 9999,
                    TimeOfDay = FirstImportValue(condition.TimeSlots),
                    Weather = FirstImportValue(condition.Weathers),
                    Season = FirstImportValue(condition.Seasons),
                    CostumeId = condition.CostumeId ?? string.Empty,
                    Once = condition.Once,
                    RequiredFlagIdsText = JoinImportList(condition.RequiredFlagIds),
                    RequiredSkillIdsText = JoinImportList(condition.RequiredSkillIds),
                    RequiredSkillIdsSpecified = condition.RequiredSkillIds != null
                }
            };
            entry.Lines.Clear();
            foreach (FromUnityActionLine line in reaction.ResultLines ?? new List<FromUnityActionLine>())
            {
                if (line != null)
                    entry.Lines.Add(new ConversationLine
                    {
                        Speaker = string.IsNullOrWhiteSpace(line.Speaker) ? "Heroine" : line.Speaker.Trim(),
                        Text = line.Text ?? string.Empty,
                        Expression = line.Expression ?? string.Empty,
                        VoiceId = line.VoiceId ?? string.Empty
                    });
            }
            if (entry.Lines.Count == 0) entry.Lines.Add(new ConversationLine());
            return entry;
        }

        private static string FirstImportValue(IEnumerable<string> values)
        {
            return values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
                ?? string.Empty;
        }

        private static ConversationEntry CreateActionReactionFromUnityAction(FromUnityActionDataItem item)
        {
            string actionId = item.Id.Trim();
            ConversationEntry entry = new ConversationEntry
            {
                Kind = ConversationDataKind.ActionReactions,
                Id = CreateActionReactionEntryId(actionId),
                Title = string.IsNullOrWhiteSpace(item.DisplayName) ? actionId : item.DisplayName.Trim(),
                Category = string.IsNullOrWhiteSpace(item.Category) ? actionId : item.Category.Trim(),
                ImageAssetIdsText = JoinImportList(item.ImageAssetIds),
                Priority = item.Priority,
                Memo = BuildFromUnityActionMemo(item)
            };
            entry.Conditions.ActionId = actionId;
            entry.Conditions.RequiredItemId = item.RequiredItemId ?? string.Empty;
            entry.Conditions.RequiredFlagIdsText = JoinImportList(item.RequiredFlagIds);

            entry.Lines.Clear();
            foreach (FromUnityActionLine line in item.ResultLines ?? new List<FromUnityActionLine>())
            {
                if (line == null)
                {
                    continue;
                }

                entry.Lines.Add(new ConversationLine
                {
                    Speaker = string.IsNullOrWhiteSpace(line.Speaker) ? "Heroine" : line.Speaker.Trim(),
                    Text = line.Text ?? string.Empty,
                    Expression = line.Expression ?? string.Empty,
                    VoiceId = line.VoiceId ?? string.Empty
                });
            }

            if (entry.Lines.Count == 0)
            {
                entry.Lines.Add(new ConversationLine
                {
                    Speaker = "Heroine",
                    Text = string.Empty,
                    Expression = string.Empty
                });
            }

            return entry;
        }

        private static string CreateActionReactionEntryId(string actionId)
        {
            return "Reaction_" + NormalizeIdPart(actionId) + "_01";
        }

        private static string JoinImportList(IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return string.Join(
                Environment.NewLine,
                values
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static string BuildFromUnityActionMemo(FromUnityActionDataItem item)
        {
            List<string> parts = new List<string> { "FromUnity actions import" };
            if (!string.IsNullOrWhiteSpace(item.Memo))
            {
                parts.Add(item.Memo.Trim());
            }

            return string.Join(Environment.NewLine, parts);
        }

        private void ImportConversationsFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "conversations_from_unity.json を選択",
                Filter = "conversations_from_unity.json|conversations_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity conversations import をキャンセルしました。";
                return;
            }

            try
            {
                ImportConversationsFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity conversations import に失敗しました: {ex.Message}";
            }
        }

        private void ImportConversationsFromUnityFile(string filePath)
        {
            if (SelectedProfile == null)
            {
                return;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            FromUnityConversationDataFile conversationData = JsonSerializer.Deserialize<FromUnityConversationDataFile>(
                File.ReadAllText(filePath),
                options);

            if (conversationData == null)
            {
                throw new InvalidOperationException("conversations_from_unity.json を読み込めませんでした。");
            }

            if (conversationData.SchemaVersion != 1)
            {
                throw new InvalidOperationException($"未対応の schemaVersion です: {conversationData.SchemaVersion}");
            }

            if (!string.IsNullOrWhiteSpace(conversationData.HeroineId)
                && !string.Equals(conversationData.HeroineId, SelectedProfile.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"HeroineId が選択中のキャラクターと一致しません。JSON: {conversationData.HeroineId} / Selected: {SelectedProfile.HeroineId}");
            }

            SelectedProfile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            List<ConversationEntry> importedEntries = new List<ConversationEntry>();
            int skippedCount = 0;

            foreach (FromUnityConversationItem item in conversationData.Items ?? new List<FromUnityConversationItem>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    skippedCount++;
                    continue;
                }

                if (HasExistingConversationEntry(ConversationDataKind.Conversations, item.Id.Trim()))
                {
                    skippedCount++;
                    continue;
                }

                ConversationEntry entry = CreateConversationFromUnityConversation(item);
                SelectedProfile.ConversationEntries.Add(entry);
                importedEntries.Add(entry);
            }

            characterProjectService.SaveProfile(SelectedProfile);
            SelectedConversationDataKind = ConversationDataKind.Conversations;
            RefreshConversationCategorySuggestions();
            RefreshFilteredConversationEntries();
            if (importedEntries.Count > 0)
            {
                SelectedConversationEntry = importedEntries[0];
            }

            StatusMessage = $"FromUnity conversations を取り込みました。追加 {importedEntries.Count} 件、スキップ {skippedCount} 件。";
        }

        private bool HasExistingConversationEntry(ConversationDataKind kind, string id)
        {
            return FindConversationEntry(kind, id) != null;
        }

        private ConversationEntry FindConversationEntry(ConversationDataKind kind, string id)
        {
            if (SelectedProfile == null || SelectedProfile.ConversationEntries == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return SelectedProfile.ConversationEntries.FirstOrDefault(entry => entry != null
                && entry.Kind == kind
                && string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        private static ConversationEntry CreateConversationFromUnityConversation(FromUnityConversationItem item)
        {
            ConversationEntry entry = new ConversationEntry
            {
                Kind = ConversationDataKind.Conversations,
                Id = item.Id.Trim(),
                Title = string.IsNullOrWhiteSpace(item.Title) ? item.Id.Trim() : item.Title.Trim(),
                Category = string.IsNullOrWhiteSpace(item.Category) ? "Daily" : item.Category.Trim(),
                ImageAssetIdsText = JoinImportList(item.ImageAssetIds),
                Priority = item.Priority,
                Memo = BuildFromUnityConversationMemo(item)
            };

            ApplyFromUnityConversationCondition(entry.Conditions, item.Conditions);
            ApplyFromUnityConversationChoices(entry.Choices, item.SourceMetadata?.Choices);

            entry.Lines.Clear();
            foreach (FromUnityConversationLine line in item.Lines ?? new List<FromUnityConversationLine>())
            {
                if (line == null)
                {
                    continue;
                }

                entry.Lines.Add(new ConversationLine
                {
                    Speaker = string.IsNullOrWhiteSpace(line.Speaker) ? "Heroine" : line.Speaker.Trim(),
                    Text = line.Text ?? string.Empty,
                    Expression = line.Expression ?? string.Empty,
                    VoiceId = line.VoiceId ?? string.Empty
                });
            }

            if (entry.Lines.Count == 0)
            {
                entry.Lines.Add(new ConversationLine
                {
                    Speaker = "Heroine",
                    Text = string.Empty,
                    Expression = string.Empty
                });
            }

            return entry;
        }

        private static void ApplyFromUnityConversationCondition(
            ConversationCondition target,
            FromUnityConversationCondition source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.LocationId = source.LocationId ?? string.Empty;
            target.MinAffection = source.MinAffection;
            target.MaxAffection = source.MaxAffection == 0 ? 9999 : source.MaxAffection;
            target.Weather = source.Weather ?? string.Empty;
            target.Season = source.Season ?? string.Empty;
            target.TimeOfDay = source.TimeOfDay ?? string.Empty;
            target.ActionId = source.ActionId ?? string.Empty;
            target.CostumeId = source.CostumeId ?? string.Empty;
            target.RequiredItemId = source.RequiredItemId ?? string.Empty;
            target.Once = source.Once;
            target.RequiredFlagIdsText = JoinImportList(source.RequiredFlagIds);
        }

        private static string BuildFromUnityConversationMemo(FromUnityConversationItem item)
        {
            List<string> parts = new List<string> { "FromUnity conversations import" };
            if (!string.IsNullOrWhiteSpace(item.Memo))
            {
                parts.Add(item.Memo.Trim());
            }

            return string.Join(Environment.NewLine, parts);
        }

        private static void ApplyFromUnityConversationChoices(
            ObservableCollection<ConversationChoice> target,
            IEnumerable<FromUnityConversationChoice> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.Clear();
            foreach (FromUnityConversationChoice choice in source)
            {
                if (choice == null)
                {
                    continue;
                }

                target.Add(new ConversationChoice
                {
                    ChoiceText = choice.ChoiceText ?? string.Empty,
                    ResponseText = choice.ResponseText ?? string.Empty,
                    AffectionChange = choice.AffectionChange
                });
            }
        }

        private void ImportGameEventsFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "game_events_from_unity.json を選択",
                Filter = "game_events_from_unity.json|game_events_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity game events import をキャンセルしました。";
                return;
            }

            try
            {
                ImportGameEventsFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity game events import に失敗しました: {ex.Message}";
            }
        }

        private void ImportGameEventsFromUnityFile(string filePath)
        {
            if (SelectedProfile == null)
            {
                return;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            FromUnityGameEventDataFile eventData = JsonSerializer.Deserialize<FromUnityGameEventDataFile>(
                File.ReadAllText(filePath),
                options);

            if (eventData == null)
            {
                throw new InvalidOperationException("game_events_from_unity.json を読み込めませんでした。");
            }

            if (eventData.SchemaVersion != 1)
            {
                throw new InvalidOperationException($"未対応の schemaVersion です: {eventData.SchemaVersion}");
            }

            if (!string.IsNullOrWhiteSpace(eventData.HeroineId)
                && !string.Equals(eventData.HeroineId, SelectedProfile.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"HeroineId が選択中のキャラクターと一致しません。JSON: {eventData.HeroineId} / Selected: {SelectedProfile.HeroineId}");
            }

            SelectedProfile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            List<ConversationEntry> importedEntries = new List<ConversationEntry>();
            int skippedCount = 0;

            foreach (FromUnityGameEventItem item in eventData.Items ?? new List<FromUnityGameEventItem>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    skippedCount++;
                    continue;
                }

                if (HasExistingConversationEntry(ConversationDataKind.GameEvents, item.Id.Trim()))
                {
                    skippedCount++;
                    continue;
                }

                ConversationEntry entry = CreateGameEventFromUnityGameEvent(item);
                SelectedProfile.ConversationEntries.Add(entry);
                importedEntries.Add(entry);
            }

            characterProjectService.SaveProfile(SelectedProfile);
            SelectedConversationDataKind = ConversationDataKind.GameEvents;
            RefreshConversationCategorySuggestions();
            RefreshFilteredConversationEntries();
            if (importedEntries.Count > 0)
            {
                SelectedConversationEntry = importedEntries[0];
            }

            StatusMessage = $"FromUnity game events を取り込みました。追加 {importedEntries.Count} 件、スキップ {skippedCount} 件。";
        }

        private static ConversationEntry CreateGameEventFromUnityGameEvent(FromUnityGameEventItem item)
        {
            ConversationEntry entry = new ConversationEntry
            {
                Kind = ConversationDataKind.GameEvents,
                Id = item.Id.Trim(),
                Title = string.IsNullOrWhiteSpace(item.Title) ? item.Id.Trim() : item.Title.Trim(),
                Category = string.IsNullOrWhiteSpace(item.Category) ? "Manual" : item.Category.Trim(),
                ImageAssetIdsText = JoinImportList(item.ImageAssetIds),
                Priority = item.Priority,
                AffectionChange = item.AffectionChange,
                Memo = BuildFromUnityGameEventMemo(item)
            };

            ApplyFromUnityGameEventCondition(entry.Conditions, item.Conditions);
            ApplyFromUnityGameEventChoices(entry.Choices, item.SourceMetadata?.Choices);

            entry.Lines.Clear();
            foreach (FromUnityGameEventLine line in item.Lines ?? new List<FromUnityGameEventLine>())
            {
                if (line == null)
                {
                    continue;
                }

                entry.Lines.Add(new ConversationLine
                {
                    Speaker = string.IsNullOrWhiteSpace(line.Speaker) ? "Heroine" : line.Speaker.Trim(),
                    Text = line.Text ?? string.Empty,
                    Expression = line.Expression ?? string.Empty,
                    VoiceId = line.VoiceId ?? string.Empty
                });
            }

            if (entry.Lines.Count == 0)
            {
                entry.Lines.Add(new ConversationLine
                {
                    Speaker = "Heroine",
                    Text = string.Empty,
                    Expression = string.Empty
                });
            }

            return entry;
        }

        private static void ApplyFromUnityGameEventCondition(
            ConversationCondition target,
            FromUnityGameEventCondition source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.LocationId = source.LocationId ?? string.Empty;
            target.GameEventTriggerType = source.TriggerType ?? string.Empty;
            target.TriggerContextId = source.TriggerContextId ?? string.Empty;
            target.MinAffection = source.MinAffection;
            target.MaxAffection = source.MaxAffection == 0 ? 9999 : source.MaxAffection;
            target.Weather = source.Weather ?? string.Empty;
            target.Season = source.Season ?? string.Empty;
            target.TimeOfDay = source.TimeOfDay ?? string.Empty;
            target.ActionId = source.ActionId ?? string.Empty;
            target.CostumeId = source.CostumeId ?? string.Empty;
            target.RequiredItemId = source.RequiredItemId ?? string.Empty;
            target.Once = source.Once;
            target.RequiredFlagIdsText = JoinImportList(source.RequiredFlagIds);
            RequiredSkillIdSyncService.ApplyImportedValues(target, source.RequiredSkillIds);
        }

        private static string BuildFromUnityGameEventMemo(FromUnityGameEventItem item)
        {
            List<string> parts = new List<string> { "FromUnity game events import" };
            if (!string.IsNullOrWhiteSpace(item.Memo))
            {
                parts.Add(item.Memo.Trim());
            }

            return string.Join(Environment.NewLine, parts);
        }

        private static void ApplyFromUnityGameEventChoices(
            ObservableCollection<ConversationChoice> target,
            IEnumerable<FromUnityGameEventChoice> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.Clear();
            foreach (FromUnityGameEventChoice choice in source)
            {
                if (choice == null)
                {
                    continue;
                }

                target.Add(new ConversationChoice
                {
                    ChoiceText = choice.ChoiceText ?? string.Empty,
                    ResponseText = choice.ResponseText ?? string.Empty,
                    AffectionChange = choice.AffectionChange
                });
            }
        }

        private void ImportScheduledEventsFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "scheduled_events_from_unity.json を選択",
                Filter = "scheduled_events_from_unity.json|scheduled_events_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity scheduled events import をキャンセルしました。";
                return;
            }

            try
            {
                ImportScheduledEventsFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity scheduled events import に失敗しました: {ex.Message}";
            }
        }

        private void ImportScheduledEventsFromUnityFile(string filePath)
        {
            if (SelectedProfile == null)
            {
                return;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            FromUnityScheduledEventDataFile scheduledEventData = JsonSerializer.Deserialize<FromUnityScheduledEventDataFile>(
                File.ReadAllText(filePath),
                options);

            if (scheduledEventData == null)
            {
                throw new InvalidOperationException("scheduled_events_from_unity.json を読み込めませんでした。");
            }

            if (scheduledEventData.SchemaVersion != 1)
            {
                throw new InvalidOperationException($"未対応の schemaVersion です: {scheduledEventData.SchemaVersion}");
            }

            if (!string.IsNullOrWhiteSpace(scheduledEventData.HeroineId)
                && !string.Equals(scheduledEventData.HeroineId, SelectedProfile.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"HeroineId が選択中のキャラクターと一致しません。JSON: {scheduledEventData.HeroineId} / Selected: {SelectedProfile.HeroineId}");
            }

            SelectedProfile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            List<ConversationEntry> importedEntries = new List<ConversationEntry>();
            List<ConversationEntry> existingEntries = new List<ConversationEntry>();
            int skippedCount = 0;

            foreach (FromUnityScheduledEventItem item in GetFromUnityScheduledEventItems(scheduledEventData))
            {
                string scheduledEventId = GetFromUnityScheduledEventId(item);
                string scheduleType = GetFromUnityScheduleType(item);
                if (item == null || string.IsNullOrWhiteSpace(scheduledEventId))
                {
                    skippedCount++;
                    continue;
                }

                ConversationEntry existingEntry = FindExistingScheduledEventEntry(scheduledEventId, scheduleType);
                if (existingEntry != null)
                {
                    existingEntries.Add(existingEntry);
                    skippedCount++;
                    continue;
                }

                ConversationEntry entry = CreateScheduledEventFromUnityScheduledEvent(item);
                SelectedProfile.ConversationEntries.Add(entry);
                importedEntries.Add(entry);
            }

            characterProjectService.SaveProfile(SelectedProfile);
            SelectedConversationDataKind = ConversationDataKind.ScheduledEvents;
            ResetConversationListFilters();
            RefreshConversationCategorySuggestions();
            RefreshFilteredConversationEntries();
            if (importedEntries.Count > 0)
            {
                SelectedConversationEntry = importedEntries[0];
            }
            else if (existingEntries.Count > 0)
            {
                SelectedConversationEntry = existingEntries[0];
            }

            StatusMessage = $"FromUnity scheduled events を取り込みました。追加 {importedEntries.Count} 件、スキップ {skippedCount} 件。";
        }

        private ConversationEntry FindExistingScheduledEventEntry(string id, string scheduleType)
        {
            ConversationEntry existingEntry = FindConversationEntry(ConversationDataKind.ScheduledEvents, id);
            if (existingEntry != null || string.IsNullOrWhiteSpace(scheduleType) || SelectedProfile?.ConversationEntries == null)
            {
                return existingEntry;
            }

            return SelectedProfile.ConversationEntries.FirstOrDefault(entry => entry != null
                && entry.Kind == ConversationDataKind.ScheduledEvents
                && string.Equals(entry.Category, scheduleType, StringComparison.OrdinalIgnoreCase));
        }

        private static ConversationEntry CreateScheduledEventFromUnityScheduledEvent(FromUnityScheduledEventItem item)
        {
            string scheduledEventId = GetFromUnityScheduledEventId(item);
            string scheduleType = GetFromUnityScheduleType(item);
            ConversationEntry entry = new ConversationEntry
            {
                Kind = ConversationDataKind.ScheduledEvents,
                Id = scheduledEventId,
                Title = GetFromUnityScheduledEventTitle(item, scheduledEventId),
                Category = string.IsNullOrWhiteSpace(scheduleType) ? "StayHome" : scheduleType,
                ImageAssetIdsText = JoinImportList(GetFromUnityScheduledEventImageAssetIds(item)),
                Priority = item.Priority,
                Memo = BuildFromUnityScheduledEventMemo(item)
            };

            ApplyFromUnityScheduledEventCondition(entry.Conditions, item);

            entry.Lines.Clear();
            bool hasDirectMessages = !string.IsNullOrWhiteSpace(item.PreparationMessage)
                || !string.IsNullOrWhiteSpace(item.EventMessage);
            if (hasDirectMessages)
            {
                if (!string.IsNullOrWhiteSpace(item.PreparationMessage))
                {
                    entry.Lines.Add(new ConversationLine
                    {
                        Speaker = "System",
                        Text = item.PreparationMessage,
                        Expression = string.Empty
                    });
                }

                if (!string.IsNullOrWhiteSpace(item.EventMessage))
                {
                    entry.Lines.Add(new ConversationLine
                    {
                        Speaker = string.IsNullOrWhiteSpace(item.EventSpeakerType) ? "Heroine" : item.EventSpeakerType,
                        Text = item.EventMessage,
                        Expression = string.Empty
                    });
                }
            }
            else
            {
                foreach (FromUnityScheduledEventLine line in item.Lines ?? new List<FromUnityScheduledEventLine>())
                {
                    if (line == null)
                    {
                        continue;
                    }

                    entry.Lines.Add(new ConversationLine
                    {
                        Speaker = string.IsNullOrWhiteSpace(line.Speaker) ? "Heroine" : line.Speaker.Trim(),
                        Text = line.Text ?? string.Empty,
                        Expression = line.Expression ?? string.Empty,
                        VoiceId = line.VoiceId ?? string.Empty
                    });
                }
            }

            if (entry.Lines.Count == 0)
            {
                entry.Lines.Add(new ConversationLine
                {
                    Speaker = "System",
                    Text = string.Empty,
                    Expression = string.Empty
                });
            }

            return entry;
        }

        private static IEnumerable<FromUnityScheduledEventItem> GetFromUnityScheduledEventItems(FromUnityScheduledEventDataFile scheduledEventData)
        {
            if (scheduledEventData == null)
            {
                return Array.Empty<FromUnityScheduledEventItem>();
            }

            return scheduledEventData.Items ?? scheduledEventData.ScheduledEvents ?? new List<FromUnityScheduledEventItem>();
        }

        private static string GetFromUnityScheduledEventId(FromUnityScheduledEventItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            string scheduleType = GetFromUnityScheduleType(item);
            return FirstNonEmpty(item.Id, item.ScheduledEventId, scheduleType).Trim();
        }

        private static string GetFromUnityScheduleType(FromUnityScheduledEventItem item)
        {
            return FirstNonEmpty(item?.ScheduleType, item?.Conditions?.ScheduleType, item?.Category).Trim();
        }

        private static string GetFromUnityScheduledEventTitle(FromUnityScheduledEventItem item, string fallbackId)
        {
            return FirstNonEmpty(item?.Title, item?.DisplayName, item?.ScheduleType, item?.Category, fallbackId).Trim();
        }

        private static IEnumerable<string> GetFromUnityScheduledEventImageAssetIds(FromUnityScheduledEventItem item)
        {
            List<string> values = new List<string>();
            if (item?.ImageAssetIds != null)
            {
                values.AddRange(item.ImageAssetIds);
            }

            values.Add(item?.StillId);
            values.Add(item?.StillAssetId);
            return values;
        }

        private static void ApplyFromUnityScheduledEventCondition(
            ConversationCondition target,
            FromUnityScheduledEventItem item)
        {
            if (target == null || item == null)
            {
                return;
            }

            FromUnityScheduledEventCondition source = item.Conditions;
            target.LocationId = source?.LocationId ?? string.Empty;
            target.MinAffection = source?.MinAffection ?? 0;
            target.MaxAffection = source == null || source.MaxAffection == 0 ? 100 : source.MaxAffection;
            target.Weather = source?.Weather ?? string.Empty;
            target.Season = source?.Season ?? string.Empty;
            target.TimeOfDay = FirstNonEmpty(item.TriggerTimeSlot, source?.TriggerTimeSlot, source?.TimeOfDay);
            target.ActionId = FirstNonEmpty(item.ActionId, source?.ActionId);
            target.CostumeId = FirstNonEmpty(item.CostumeId, source?.CostumeId);
            target.RequiredItemId = source?.RequiredItemId ?? string.Empty;
            target.Once = source != null && source.Once;
            target.RequiredFlagIdsText = JoinImportList(source?.RequiredFlagIds);
        }

        private static string BuildFromUnityScheduledEventMemo(FromUnityScheduledEventItem item)
        {
            List<string> parts = new List<string> { "FromUnity scheduled events import" };
            if (!string.IsNullOrWhiteSpace(item.Memo))
            {
                parts.Add(item.Memo.Trim());
            }

            if (!string.IsNullOrWhiteSpace(item.OutfitPromptMode))
            {
                parts.Add("outfitPromptMode: " + item.OutfitPromptMode.Trim());
            }

            if (!string.IsNullOrWhiteSpace(item.EventSpeakerType))
            {
                parts.Add("eventSpeakerType: " + item.EventSpeakerType.Trim());
            }

            if (item.AffectionChange != 0)
            {
                parts.Add("affectionChange: " + item.AffectionChange);
            }

            return string.Join(Environment.NewLine, parts);
        }

        private void ImportEndingsFromUnity()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "endings_from_unity.json を選択",
                Filter = "endings_from_unity.json|endings_from_unity.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                StatusMessage = "FromUnity endings import をキャンセルしました。";
                return;
            }

            try
            {
                ImportEndingsFromUnityFile(dialog.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FromUnity endings import に失敗しました: {ex.Message}";
            }
        }

        private void ImportEndingsFromUnityFile(string filePath)
        {
            if (SelectedProfile == null)
            {
                return;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            FromUnityEndingDataFile endingData = JsonSerializer.Deserialize<FromUnityEndingDataFile>(
                File.ReadAllText(filePath),
                options);

            if (endingData == null)
            {
                throw new InvalidOperationException("endings_from_unity.json を読み込めませんでした。");
            }

            if (endingData.SchemaVersion != 1)
            {
                throw new InvalidOperationException($"未対応の schemaVersion です: {endingData.SchemaVersion}");
            }

            if (!string.IsNullOrWhiteSpace(endingData.HeroineId)
                && !string.Equals(endingData.HeroineId, SelectedProfile.HeroineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"HeroineId が選択中のキャラクターと一致しません。JSON: {endingData.HeroineId} / Selected: {SelectedProfile.HeroineId}");
            }

            SelectedProfile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            List<ConversationEntry> importedEntries = new List<ConversationEntry>();
            List<ConversationEntry> existingEntries = new List<ConversationEntry>();
            int skippedCount = 0;

            foreach (FromUnityEndingItem item in GetFromUnityEndingItems(endingData))
            {
                string endingId = GetFromUnityEndingId(item);
                if (item == null || string.IsNullOrWhiteSpace(endingId))
                {
                    skippedCount++;
                    continue;
                }

                ConversationEntry existingEntry = FindConversationEntry(ConversationDataKind.Endings, endingId);
                if (existingEntry != null)
                {
                    existingEntries.Add(existingEntry);
                    skippedCount++;
                    continue;
                }

                ConversationEntry entry = CreateEndingFromUnityEnding(item);
                SelectedProfile.ConversationEntries.Add(entry);
                importedEntries.Add(entry);
            }

            characterProjectService.SaveProfile(SelectedProfile);
            SelectedConversationDataKind = ConversationDataKind.Endings;
            ResetConversationListFilters();
            RefreshConversationCategorySuggestions();
            RefreshFilteredConversationEntries();
            if (importedEntries.Count > 0)
            {
                SelectedConversationEntry = importedEntries[0];
            }
            else if (existingEntries.Count > 0)
            {
                SelectedConversationEntry = existingEntries[0];
            }

            StatusMessage = $"FromUnity endings を取り込みました。追加 {importedEntries.Count} 件、スキップ {skippedCount} 件。";
        }

        private static ConversationEntry CreateEndingFromUnityEnding(FromUnityEndingItem item)
        {
            string endingId = GetFromUnityEndingId(item);
            string endingMessage = GetFromUnityEndingMessage(item);
            ConversationEntry entry = new ConversationEntry
            {
                Kind = ConversationDataKind.Endings,
                Id = endingId,
                Title = GetFromUnityEndingTitle(item, endingId),
                Category = GetFromUnityEndingCategory(item),
                ImageAssetIdsText = JoinImportList(GetFromUnityEndingImageAssetIds(item)),
                Priority = item.Priority,
                EndingVisualMode = string.IsNullOrWhiteSpace(item.VisualMode)
                    ? "Auto"
                    : item.VisualMode.Trim(),
                KeepEndingStillAcrossPages = item.KeepStillAcrossPages,
                Memo = BuildFromUnityEndingMemo(item)
            };

            ApplyFromUnityEndingCondition(entry.Conditions, item);

            entry.Lines.Clear();
            foreach (FromUnityEndingLine line in item.Lines ?? new List<FromUnityEndingLine>())
            {
                if (line == null)
                {
                    continue;
                }

                entry.Lines.Add(new ConversationLine
                {
                    Speaker = string.IsNullOrWhiteSpace(line.Speaker) ? "Heroine" : line.Speaker.Trim(),
                    Text = string.IsNullOrWhiteSpace(line.Text) ? line.Message ?? string.Empty : line.Text,
                    Expression = line.Expression ?? string.Empty,
                    VoiceId = line.VoiceId ?? string.Empty
                });
            }

            if (entry.Lines.Count == 0)
            {
                entry.Lines.Add(new ConversationLine
                {
                    Speaker = "Heroine",
                    Text = endingMessage,
                    Expression = string.Empty
                });
            }
            else if (!string.IsNullOrWhiteSpace(endingMessage)
                && entry.Lines.All(line => string.IsNullOrWhiteSpace(line.Text)))
            {
                entry.Lines[0].Text = endingMessage;
            }

            return entry;
        }

        private static IEnumerable<FromUnityEndingItem> GetFromUnityEndingItems(FromUnityEndingDataFile endingData)
        {
            if (endingData == null)
            {
                return Array.Empty<FromUnityEndingItem>();
            }

            return endingData.Items ?? endingData.Endings ?? new List<FromUnityEndingItem>();
        }

        private static string GetFromUnityEndingId(FromUnityEndingItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            return FirstNonEmpty(item.Id, item.EndingId).Trim();
        }

        private static string GetFromUnityEndingTitle(FromUnityEndingItem item, string fallbackId)
        {
            return FirstNonEmpty(item?.Title, item?.DisplayName, item?.Name, fallbackId).Trim();
        }

        private static string GetFromUnityEndingCategory(FromUnityEndingItem item)
        {
            string category = FirstNonEmpty(item?.Category, item?.EndingType);
            if (!string.IsNullOrWhiteSpace(category))
            {
                return category.Trim();
            }

            string source = FirstNonEmpty(item?.EndingId, item?.Id, item?.DisplayName, item?.Title, item?.Name);
            if (source.IndexOf("Good", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Good";
            }

            if (source.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Normal";
            }

            if (source.IndexOf("Bad", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Bad";
            }

            return "Ending";
        }

        private static IEnumerable<string> GetFromUnityEndingImageAssetIds(FromUnityEndingItem item)
        {
            List<string> values = new List<string>();
            if (item?.ImageAssetIds != null)
            {
                values.AddRange(item.ImageAssetIds);
            }

            values.Add(item?.StillId);
            values.Add(item?.StillAssetId);
            values.Add(item?.StillSpriteName);
            values.Add(GetFromUnityStillSpriteName(item?.StillSprite));
            return values;
        }

        private static string GetFromUnityStillSpriteName(JsonElement? stillSprite)
        {
            if (stillSprite == null || stillSprite.Value.ValueKind == JsonValueKind.Undefined || stillSprite.Value.ValueKind == JsonValueKind.Null)
            {
                return string.Empty;
            }

            JsonElement element = stillSprite.Value;
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString() ?? string.Empty;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (TryGetJsonString(element, "name", out string name)
                    || TryGetJsonString(element, "assetId", out name)
                    || TryGetJsonString(element, "spriteName", out name))
                {
                    return name;
                }
            }

            return string.Empty;
        }

        private static string GetFromUnityEndingMessage(FromUnityEndingItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(item.Message))
            {
                return item.Message;
            }

            if (!string.IsNullOrWhiteSpace(item.SourceMetadata?.Message))
            {
                return item.SourceMetadata.Message;
            }

            return string.Empty;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static bool TryGetJsonString(JsonElement element, string propertyName, out string value)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.String)
                {
                    value = property.Value.GetString() ?? string.Empty;
                    return !string.IsNullOrWhiteSpace(value);
                }
            }

            value = string.Empty;
            return false;
        }

        private static void ApplyFromUnityEndingCondition(
            ConversationCondition target,
            FromUnityEndingItem item)
        {
            if (target == null || item == null)
            {
                return;
            }

            FromUnityEndingCondition source = item.Conditions;
            if (source == null)
            {
                target.MinAffection = item.RequiredAffection;
                target.MaxAffection = 100;
                target.RequiredFlagIdsText = JoinImportList(GetFromUnityEndingRequiredFlagIds(item));
                return;
            }

            target.LocationId = source.LocationId ?? string.Empty;
            target.MinAffection = source.MinAffection == 0 ? item.RequiredAffection : source.MinAffection;
            target.MaxAffection = source.MaxAffection == 0 ? 100 : source.MaxAffection;
            target.Weather = source.Weather ?? string.Empty;
            target.Season = source.Season ?? string.Empty;
            target.TimeOfDay = source.TimeOfDay ?? string.Empty;
            target.ActionId = source.ActionId ?? string.Empty;
            target.CostumeId = source.CostumeId ?? string.Empty;
            target.RequiredItemId = source.RequiredItemId ?? string.Empty;
            target.Once = source.Once;
            target.RequiredFlagIdsText = JoinImportList(GetFromUnityEndingRequiredFlagIds(item));
        }

        private static IEnumerable<string> GetFromUnityEndingRequiredFlagIds(FromUnityEndingItem item)
        {
            List<string> values = new List<string>();
            if (item?.Conditions?.RequiredFlagIds != null)
            {
                values.AddRange(item.Conditions.RequiredFlagIds);
            }

            if (item?.RequiredFlagIds != null)
            {
                values.AddRange(item.RequiredFlagIds);
            }

            if (item?.RequiredShownEventIds != null)
            {
                values.AddRange(item.RequiredShownEventIds);
            }

            return values;
        }

        private static string BuildFromUnityEndingMemo(FromUnityEndingItem item)
        {
            List<string> parts = new List<string> { "FromUnity endings import" };
            if (!string.IsNullOrWhiteSpace(item.Memo))
            {
                parts.Add(item.Memo.Trim());
            }

            return string.Join(Environment.NewLine, parts);
        }

        private void ApplyConversationCategorySuggestion()
        {
            if (SelectedConversationEntry == null || string.IsNullOrWhiteSpace(SelectedConversationCategorySuggestion))
            {
                return;
            }

            SelectedConversationEntry.Category = SelectedConversationCategorySuggestion;
            OnPropertyChanged(nameof(SelectedConversationEntry));
            StatusMessage = "カテゴリ候補を反映しました。";
        }

        private void ApplyConversationEventTemplate()
        {
            if (SelectedProfile == null || SelectedConversationEntry == null)
            {
                return;
            }

            if (SelectedConversationDataKind == ConversationDataKind.ScheduledEvents)
            {
                ApplyScheduledEventTemplate();
                return;
            }

            string category = string.IsNullOrWhiteSpace(SelectedConversationCategorySuggestion)
                ? CreateDefaultConversationCategory(ConversationDataKind.GameEvents)
                : SelectedConversationCategorySuggestion;

            SelectedConversationEntry.Kind = ConversationDataKind.GameEvents;
            SelectedConversationEntry.Category = category;
            SelectedConversationEntry.Conditions ??= new ConversationCondition();
            ApplyEventTemplateConditions(SelectedConversationEntry.Conditions, category);

            SelectedConversationEntry.Id = CreateConversationEntryId(
                ConversationDataKind.GameEvents,
                category,
                SelectedProfile.ConversationEntries ?? new ObservableCollection<ConversationEntry>(),
                SelectedConversationEntry);

            if (string.IsNullOrWhiteSpace(SelectedConversationEntry.Title)
                || SelectedConversationEntry.Title.StartsWith(GetConversationKindDisplayName(ConversationDataKind.GameEvents), StringComparison.OrdinalIgnoreCase))
            {
                SelectedConversationEntry.Title = CreateEventTemplateTitle(category);
            }

            if (IsConversationLinesEmpty(SelectedConversationEntry.Lines))
            {
                SelectedConversationEntry.Lines = CreateEventTemplateLines(SelectedProfile, category);
                SelectedConversationLine = SelectedConversationEntry.Lines.FirstOrDefault();
            }

            ConversationEntry appliedEntry = SelectedConversationEntry;
            OnPropertyChanged(nameof(SelectedConversationEntry));
            RefreshFilteredConversationEntries();
            SelectedConversationEntry = appliedEntry;
            StatusMessage = $"{category} のイベント雛形を反映しました。";
        }

        private void ApplyScheduledEventTemplate()
        {
            if (SelectedProfile == null || SelectedConversationEntry == null)
            {
                return;
            }

            string scheduleType = string.IsNullOrWhiteSpace(SelectedConversationCategorySuggestion)
                ? CreateDefaultConversationCategory(ConversationDataKind.ScheduledEvents)
                : SelectedConversationCategorySuggestion;

            ScheduledEventTemplate template = CreateScheduledEventTemplate(scheduleType);
            SelectedConversationEntry.Kind = ConversationDataKind.ScheduledEvents;
            SelectedConversationEntry.Category = template.ScheduleType;
            SelectedConversationEntry.Conditions ??= new ConversationCondition();
            SelectedConversationEntry.Conditions.ActionId = template.ActionId;
            SelectedConversationEntry.Conditions.TimeOfDay = template.TriggerTimeSlot;
            SelectedConversationEntry.Conditions.LocationId = template.LocationId;
            SelectedConversationEntry.Conditions.Once = false;

            SelectedConversationEntry.Id = CreateConversationEntryId(
                ConversationDataKind.ScheduledEvents,
                template.ScheduleType,
                SelectedProfile.ConversationEntries ?? new ObservableCollection<ConversationEntry>(),
                SelectedConversationEntry);

            if (string.IsNullOrWhiteSpace(SelectedConversationEntry.Title)
                || SelectedConversationEntry.Title.StartsWith(GetConversationKindDisplayName(ConversationDataKind.ScheduledEvents), StringComparison.OrdinalIgnoreCase))
            {
                SelectedConversationEntry.Title = template.Title;
            }

            if (IsConversationLinesEmpty(SelectedConversationEntry.Lines))
            {
                SelectedConversationEntry.Lines = new ObservableCollection<ConversationLine>
                {
                    new ConversationLine
                    {
                        Speaker = "System",
                        Expression = string.Empty,
                        Text = template.PreparationMessage
                    },
                    new ConversationLine
                    {
                        Speaker = "Heroine",
                        Expression = "Smile",
                        Text = template.EventMessage
                    }
                };
                SelectedConversationLine = SelectedConversationEntry.Lines.FirstOrDefault();
            }

            ConversationEntry appliedEntry = SelectedConversationEntry;
            OnPropertyChanged(nameof(SelectedConversationEntry));
            RefreshFilteredConversationEntries();
            SelectedConversationEntry = appliedEntry;
            StatusMessage = $"{template.ScheduleType} の予定イベント雛形を反映しました。";
        }

        private static void ApplyEventTemplateConditions(ConversationCondition conditions, string category)
        {
            string normalizedCategory = NormalizeIdPart(category);
            conditions.Once = string.Equals(normalizedCategory, "Intro", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedCategory, "Quest", StringComparison.OrdinalIgnoreCase);

            switch (normalizedCategory)
            {
                case "Intro":
                    conditions.LocationId = "Room";
                    conditions.TimeOfDay = "Morning";
                    break;
                case "DayStart":
                    conditions.TimeOfDay = "Morning";
                    break;
                case "Location":
                    conditions.LocationId = string.IsNullOrWhiteSpace(conditions.LocationId) ? "Forest" : conditions.LocationId;
                    conditions.TimeOfDay = string.IsNullOrWhiteSpace(conditions.TimeOfDay) ? "Day" : conditions.TimeOfDay;
                    break;
                case "Date":
                    conditions.ActionId = string.IsNullOrWhiteSpace(conditions.ActionId) ? "Walk" : conditions.ActionId;
                    conditions.TimeOfDay = string.IsNullOrWhiteSpace(conditions.TimeOfDay) ? "Day" : conditions.TimeOfDay;
                    break;
                case "Quest":
                    conditions.ActionId = string.IsNullOrWhiteSpace(conditions.ActionId) ? "Talk" : conditions.ActionId;
                    conditions.RequiredFlagIdsText = string.IsNullOrWhiteSpace(conditions.RequiredFlagIdsText)
                        ? "QuestAvailable"
                        : conditions.RequiredFlagIdsText;
                    break;
                case "Weather":
                    conditions.Weather = string.IsNullOrWhiteSpace(conditions.Weather) ? "Rainy" : conditions.Weather;
                    break;
                case "Season":
                    conditions.Season = string.IsNullOrWhiteSpace(conditions.Season) ? "Spring" : conditions.Season;
                    break;
                case "Scheduled":
                    conditions.TimeOfDay = string.IsNullOrWhiteSpace(conditions.TimeOfDay) ? "Day" : conditions.TimeOfDay;
                    break;
            }
        }

        private static string CreateEventTemplateTitle(string category)
        {
            switch (NormalizeIdPart(category))
            {
                case "Intro":
                    return "導入イベント";
                case "DayStart":
                    return "一日の開始イベント";
                case "Location":
                    return "場所イベント";
                case "Date":
                    return "デートイベント";
                case "Quest":
                    return "クエストイベント";
                case "Weather":
                    return "天候イベント";
                case "Season":
                    return "季節イベント";
                case "Scheduled":
                    return "予定イベント";
                default:
                    return "イベント";
            }
        }

        private static ScheduledEventTemplate CreateScheduledEventTemplate(string scheduleType)
        {
            string normalizedScheduleType = NormalizeIdPart(scheduleType);
            switch (normalizedScheduleType)
            {
                case "SoloCave":
                    return new ScheduledEventTemplate("SoloCave", "洞窟への外出", "AutoWalkCave", "Noon", "Cave", "今日は昼に洞窟へ出かける予定です。", "洞窟を慎重に進みながら、少し緊張した時間を過ごしました。");
                case "SoloLake":
                    return new ScheduledEventTemplate("SoloLake", "湖への外出", "AutoWalkLake", "Noon", "Lake", "今日は昼に湖へ出かける予定です。", "湖のほとりで、静かな時間を過ごしました。");
                case "SoloShopping":
                    return new ScheduledEventTemplate("SoloShopping", "街への買い物", "AutoWalkShopping", "Noon", "Town", "今日は昼に街へ買い物に出かける予定です。", "街を歩きながら、気になる店をいくつか見て回りました。");
                case "DuoForest":
                    return new ScheduledEventTemplate("DuoForest", "森への同行外出", "AutoDuoForest", "Noon", "Forest", "今日は昼に二人で森へ出かける予定です。", "二人で森を歩きながら、少しだけ距離が近づいた気がしました。");
                case "DuoCave":
                    return new ScheduledEventTemplate("DuoCave", "洞窟への同行外出", "AutoDuoCave", "Noon", "Cave", "今日は昼に二人で洞窟へ出かける予定です。", "二人で洞窟を進み、危ない場所では自然と声を掛け合いました。");
                case "DuoLake":
                    return new ScheduledEventTemplate("DuoLake", "湖への同行外出", "AutoDuoLake", "Noon", "Lake", "今日は昼に二人で湖へ出かける予定です。", "湖のほとりで、二人だけの落ち着いた時間を過ごしました。");
                case "DuoShopping":
                    return new ScheduledEventTemplate("DuoShopping", "街への同行買い物", "AutoDuoShopping", "Noon", "Town", "今日は昼に二人で街へ買い物に出かける予定です。", "街で買い物をしながら、相手の好みを少し知ることができました。");
                case "StayHome":
                    return new ScheduledEventTemplate("StayHome", "家で過ごす予定", "AutoStayHome", "Noon", "Room", "今日は昼を家で過ごす予定です。", "部屋でゆっくり過ごし、穏やかな時間になりました。");
                default:
                    return new ScheduledEventTemplate("SoloForest", "森への外出", "AutoWalkForest", "Noon", "Forest", "今日は昼に森へ出かける予定です。", "森を歩きながら、静かな時間を過ごしました。");
            }
        }

        private static bool IsConversationLinesEmpty(ObservableCollection<ConversationLine> lines)
        {
            return lines == null
                || lines.Count == 0
                || lines.All(line => line == null
                    || (string.IsNullOrWhiteSpace(line.Speaker)
                        && string.IsNullOrWhiteSpace(line.Text)
                        && string.IsNullOrWhiteSpace(line.Expression)));
        }

        private static ObservableCollection<ConversationLine> CreateEventTemplateLines(HeroineProfile profile, string category)
        {
            string heroineName = string.IsNullOrWhiteSpace(profile?.DisplayName) ? "ヒロイン" : profile.DisplayName;
            string categoryName = CreateEventTemplateTitle(category);
            return new ObservableCollection<ConversationLine>
            {
                new ConversationLine
                {
                    Speaker = "主人公",
                    Expression = "Neutral",
                    Text = categoryName + "を開始する。"
                },
                new ConversationLine
                {
                    Speaker = heroineName,
                    Expression = "Smile",
                    Text = "ここにヒロインの反応を入力する。"
                },
                new ConversationLine
                {
                    Speaker = heroineName,
                    Expression = "Neutral",
                    Text = "ここにイベントの締めの台詞を入力する。"
                }
            };
        }

        private void ApplyConversationConditionSuggestions()
        {
            if (SelectedConversationEntry == null)
            {
                return;
            }

            SelectedConversationEntry.Conditions ??= new ConversationCondition();
            if (!string.IsNullOrWhiteSpace(SelectedConversationLocationSuggestion))
            {
                SelectedConversationEntry.Conditions.LocationId = SelectedConversationLocationSuggestion;
            }

            if (!string.IsNullOrWhiteSpace(SelectedConversationActionSuggestion))
            {
                SelectedConversationEntry.Conditions.ActionId = SelectedConversationActionSuggestion;
            }

            SelectedConversationEntry.Conditions.Weather = SelectedConversationWeatherSuggestion ?? string.Empty;
            SelectedConversationEntry.Conditions.Season = SelectedConversationSeasonSuggestion ?? string.Empty;
            SelectedConversationEntry.Conditions.TimeOfDay = SelectedConversationTimeOfDaySuggestion ?? string.Empty;
            SelectedConversationEntry.Conditions.CostumeId = SelectedConversationCostumeSuggestion ?? string.Empty;
            OnPropertyChanged(nameof(SelectedConversationEntry));
            StatusMessage = "条件候補を反映しました。";
        }

        private void ApplyConversationExpressionSuggestion()
        {
            if (SelectedConversationLine == null || string.IsNullOrWhiteSpace(SelectedConversationExpressionSuggestion))
            {
                return;
            }

            SelectedConversationLine.Expression = SelectedConversationExpressionSuggestion;
            OnPropertyChanged(nameof(SelectedConversationLine));
            OnPropertyChanged(nameof(SelectedConversationEntry));
            StatusMessage = "表情候補を反映しました。";
        }

        private void AddConversationImageAsset()
        {
            if (SelectedConversationEntry == null || SelectedConversationImageAsset == null)
            {
                return;
            }

            string assetId = SelectedConversationImageAsset.AssetId;
            string[] existingIds = SplitConversationList(SelectedConversationEntry.ImageAssetIdsText).ToArray();
            if (!existingIds.Contains(assetId, StringComparer.OrdinalIgnoreCase))
            {
                SelectedConversationEntry.ImageAssetIdsText = string.Join(Environment.NewLine, existingIds.Concat(new[] { assetId }));
                OnPropertyChanged(nameof(SelectedConversationEntry));
                StatusMessage = $"{assetId} を関連画像 AssetId に追加しました。";
                return;
            }

            StatusMessage = $"{assetId} は既に関連画像 AssetId に含まれています。";
        }

        private void GenerateConversationId()
        {
            if (SelectedProfile == null || SelectedConversationEntry == null)
            {
                return;
            }

            string category = string.IsNullOrWhiteSpace(SelectedConversationEntry.Category)
                ? CreateDefaultConversationCategory(SelectedConversationDataKind)
                : SelectedConversationEntry.Category;
            SelectedConversationEntry.Id = CreateConversationEntryId(
                SelectedConversationDataKind,
                category,
                SelectedProfile.ConversationEntries ?? new ObservableCollection<ConversationEntry>(),
                SelectedConversationEntry);
            OnPropertyChanged(nameof(SelectedConversationEntry));
            RefreshFilteredConversationEntries();
            StatusMessage = $"Id を {SelectedConversationEntry.Id} に更新しました。";
        }

        private void RefreshFilteredConversationEntries()
        {
            FilteredConversationEntries.Clear();
            if (SelectedProfile == null)
            {
                SelectedConversationEntry = null;
                return;
            }

            SelectedProfile.ConversationEntries ??= new ObservableCollection<ConversationEntry>();
            foreach (ConversationEntry entry in SelectedProfile.ConversationEntries.Where(entry => entry.Kind == SelectedConversationDataKind))
            {
                entry.Conditions ??= new ConversationCondition();
                entry.Lines ??= new ObservableCollection<ConversationLine>();
                entry.ValidationWarningText = BuildConversationWarningText(entry);
                bool matchesGameEventTest = MatchesGameEventTestConditions(entry);
                entry.TriggerCandidateText = SelectedConversationDataKind == ConversationDataKind.GameEvents
                    ? matchesGameEventTest ? "候補" : "対象外"
                    : string.Empty;
                if (MatchesConversationCategoryFilter(entry)
                    && MatchesConversationImageFilter(entry)
                    && MatchesConversationWarningFilter(entry)
                    && MatchesGameEventCandidateFilter(entry, matchesGameEventTest)
                    && MatchesConversationSearch(entry))
                {
                    FilteredConversationEntries.Add(entry);
                }
            }

            if (SelectedConversationEntry == null || SelectedConversationEntry.Kind != SelectedConversationDataKind || !FilteredConversationEntries.Contains(SelectedConversationEntry))
            {
                SelectedConversationEntry = FilteredConversationEntries.FirstOrDefault();
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void ResetConversationListFilters()
        {
            conversationSearchText = string.Empty;
            selectedConversationCategoryFilter = "All";
            selectedConversationImageFilter = "All";
            showOnlyConversationWarnings = false;
            showOnlyMatchingGameEvents = false;
            OnPropertyChanged(nameof(ConversationSearchText));
            OnPropertyChanged(nameof(SelectedConversationCategoryFilter));
            OnPropertyChanged(nameof(SelectedConversationImageFilter));
            OnPropertyChanged(nameof(ShowOnlyConversationWarnings));
            OnPropertyChanged(nameof(ShowOnlyMatchingGameEvents));
        }

        private bool MatchesConversationCategoryFilter(ConversationEntry entry)
        {
            return string.IsNullOrWhiteSpace(SelectedConversationCategoryFilter)
                || SelectedConversationCategoryFilter == "All"
                || string.Equals(entry.Category, SelectedConversationCategoryFilter, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesConversationImageFilter(ConversationEntry entry)
        {
            bool hasImages = SplitConversationList(entry.ImageAssetIdsText).Length > 0;
            switch (SelectedConversationImageFilter)
            {
                case "画像あり":
                    return hasImages;
                case "画像なし":
                    return !hasImages;
                default:
                    return true;
            }
        }

        private bool MatchesConversationWarningFilter(ConversationEntry entry)
        {
            return !ShowOnlyConversationWarnings || !string.IsNullOrWhiteSpace(entry.ValidationWarningText);
        }

        private bool MatchesGameEventCandidateFilter(ConversationEntry entry, bool matchesGameEventTest)
        {
            return !ShowOnlyMatchingGameEvents
                || SelectedConversationDataKind != ConversationDataKind.GameEvents
                || matchesGameEventTest;
        }

        private bool MatchesGameEventTestConditions(ConversationEntry entry)
        {
            if (entry == null || entry.Kind != ConversationDataKind.GameEvents)
            {
                return false;
            }

            ConversationCondition conditions = entry.Conditions ?? new ConversationCondition();
            if (!MatchesOptionalCondition(conditions.LocationId, GameEventTestLocationId))
            {
                return false;
            }

            if (!MatchesOptionalCondition(conditions.Weather, GameEventTestWeather)
                || !MatchesOptionalCondition(conditions.Season, GameEventTestSeason)
                || !MatchesOptionalCondition(conditions.TimeOfDay, GameEventTestTimeOfDay)
                || !MatchesOptionalCondition(conditions.ActionId, GameEventTestActionId)
                || !MatchesOptionalCondition(conditions.CostumeId, GameEventTestCostumeId)
                || !MatchesOptionalCondition(conditions.RequiredItemId, GameEventTestItemId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(GameEventTestAffection))
            {
                if (!int.TryParse(GameEventTestAffection.Trim(), out int affection))
                {
                    return false;
                }

                if (affection < conditions.MinAffection || affection > conditions.MaxAffection)
                {
                    return false;
                }
            }

            HashSet<string> currentFlagIds = new HashSet<string>(
                SplitConversationList(GameEventTestFlagIdsText),
                StringComparer.OrdinalIgnoreCase);
            foreach (string requiredFlagId in SplitConversationList(conditions.RequiredFlagIdsText))
            {
                if (!currentFlagIds.Contains(requiredFlagId))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesOptionalCondition(string requiredValue, string currentValue)
        {
            return string.IsNullOrWhiteSpace(requiredValue)
                || string.Equals(requiredValue.Trim(), (currentValue ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesConversationSearch(ConversationEntry entry)
        {
            if (string.IsNullOrWhiteSpace(ConversationSearchText))
            {
                return true;
            }

            string query = ConversationSearchText.Trim();
            return ContainsSearchText(entry.Id, query)
                || ContainsSearchText(entry.Title, query)
                || ContainsSearchText(entry.Category, query)
                || ContainsSearchText(entry.Memo, query)
                || ContainsSearchText(entry.ImageAssetIdsText, query)
                || MatchesConversationConditionSearch(entry.Conditions, query)
                || (entry.Lines ?? new ObservableCollection<ConversationLine>())
                    .Any(line => ContainsSearchText(line.Speaker, query)
                        || ContainsSearchText(line.Text, query)
                        || ContainsSearchText(line.Expression, query))
                || (entry.Choices ?? new ObservableCollection<ConversationChoice>())
                    .Any(choice => ContainsSearchText(choice.ChoiceText, query)
                        || ContainsSearchText(choice.ResponseText, query)
                        || ContainsSearchText(choice.AffectionChange.ToString(), query));
        }

        private static bool MatchesConversationConditionSearch(ConversationCondition condition, string query)
        {
            if (condition == null)
            {
                return false;
            }

            return ContainsSearchText(condition.LocationId, query)
                || ContainsSearchText(condition.Weather, query)
                || ContainsSearchText(condition.Season, query)
                || ContainsSearchText(condition.TimeOfDay, query)
                || ContainsSearchText(condition.ActionId, query)
                || ContainsSearchText(condition.CostumeId, query)
                || ContainsSearchText(condition.RequiredItemId, query)
                || ContainsSearchText(condition.RequiredFlagIdsText, query);
        }

        private static bool ContainsSearchText(string value, string query)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string BuildConversationWarningText(ConversationEntry entry)
        {
            return string.Join(" / ", BuildConversationWarningMessages(entry));
        }

        private IReadOnlyList<string> BuildConversationWarningMessages(ConversationEntry entry)
        {
            List<string> warnings = new List<string>();
            if (entry == null)
            {
                warnings.Add("データが空です");
                return warnings;
            }

            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                warnings.Add("Id 空欄");
            }

            if (string.IsNullOrWhiteSpace(entry.Title))
            {
                warnings.Add("タイトル空欄");
            }

            if (string.IsNullOrWhiteSpace(entry.Category))
            {
                warnings.Add("カテゴリ空欄");
            }

            if (HasDuplicateConversationId(entry))
            {
                warnings.Add("Id 重複");
            }

            if (entry.Priority < 0)
            {
                warnings.Add("優先度が 0 未満");
            }

            ConversationCondition conditions = entry.Conditions;
            if (conditions != null)
            {
                if (conditions.MinAffection > conditions.MaxAffection)
                {
                    warnings.Add("好感度範囲が不正");
                }

                if (entry.Kind == ConversationDataKind.GameEvents
                    && conditions.Once
                    && string.IsNullOrWhiteSpace(conditions.RequiredFlagIdsText))
                {
                    warnings.Add("一度だけイベントの必要フラグ空欄");
                }

                AddUnexpectedValueWarning(warnings, "場所", conditions.LocationId, ConversationValueCatalog.Locations);
                if (entry.Kind == ConversationDataKind.ScheduledEvents)
                {
                    AddUnexpectedValueWarning(warnings, "予定種別", entry.Category, ConversationValueCatalog.ScheduledEventTypes);
                    AddUnexpectedValueWarning(warnings, "行動", conditions.ActionId, ConversationValueCatalog.ScheduledEventActions);
                    AddUnexpectedValueWarning(warnings, "時間", conditions.TimeOfDay, ConversationValueCatalog.ScheduledTimeSlots);
                }
                else
                {
                    AddUnexpectedValueWarning(warnings, "行動", conditions.ActionId, ConversationValueCatalog.Actions);
                    AddUnexpectedValueWarning(warnings, "時間", conditions.TimeOfDay, ConversationValueCatalog.TimeOfDay);
                }
                AddUnexpectedValueWarning(warnings, "天候", conditions.Weather, ConversationValueCatalog.Weather);
                AddUnexpectedValueWarning(warnings, "季節", conditions.Season, ConversationValueCatalog.Seasons);
            }

            ObservableCollection<ConversationLine> lines = entry.Lines ?? new ObservableCollection<ConversationLine>();
            if (lines.Count == 0)
            {
                warnings.Add("台詞行なし");
            }

            for (int index = 0; index < lines.Count; index++)
            {
                ConversationLine line = lines[index];
                if (line == null)
                {
                    warnings.Add($"{index + 1} 行目が空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line.Speaker))
                {
                    warnings.Add($"{index + 1} 行目 話者空欄");
                }

                if (string.IsNullOrWhiteSpace(line.Text))
                {
                    warnings.Add($"{index + 1} 行目 本文空欄");
                }

                AddUnexpectedValueWarning(warnings, $"{index + 1} 行目 表情", line.Expression, ConversationValueCatalog.Expressions);
            }

            HashSet<string> acceptedAssetIds = new HashSet<string>(
                AcceptedAssets.Select(asset => asset.AssetId).Where(assetId => !string.IsNullOrWhiteSpace(assetId)),
                StringComparer.OrdinalIgnoreCase);
            foreach (string assetId in SplitConversationList(entry.ImageAssetIdsText))
            {
                if (!acceptedAssetIds.Contains(assetId))
                {
                    warnings.Add($"画像未Accepted: {assetId}");
                }
            }

            ObservableCollection<ConversationChoice> choices = entry.Choices ?? new ObservableCollection<ConversationChoice>();
            for (int index = 0; index < choices.Count; index++)
            {
                ConversationChoice choice = choices[index];
                if (choice == null)
                {
                    warnings.Add($"{index + 1} 番目の選択肢が空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(choice.ChoiceText))
                {
                    warnings.Add($"{index + 1} 番目の選択肢本文空欄");
                }

                if (string.IsNullOrWhiteSpace(choice.ResponseText))
                {
                    warnings.Add($"{index + 1} 番目の選択後返答空欄");
                }
            }

            return warnings;
        }

        private static void AddUnexpectedValueWarning(List<string> warnings, string label, string value, string[] allowedValues)
        {
            if (IsUnexpectedConversationValue(value, allowedValues))
            {
                warnings.Add($"{label}候補外: {value}");
            }
        }

        private bool HasDuplicateConversationId(ConversationEntry entry)
        {
            if (SelectedProfile == null
                || SelectedProfile.ConversationEntries == null
                || string.IsNullOrWhiteSpace(entry.Id))
            {
                return false;
            }

            return SelectedProfile.ConversationEntries
                .Count(other => other.Kind == entry.Kind
                    && string.Equals(other.Id, entry.Id, StringComparison.OrdinalIgnoreCase)) > 1;
        }

        private static bool IsUnexpectedConversationValue(string value, string[] allowedValues)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !allowedValues.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        private void RefreshConversationCategorySuggestions()
        {
            ConversationCategorySuggestions.Clear();
            ConversationCategoryFilters.Clear();
            ConversationCategoryFilters.Add("All");
            foreach (string category in GetConversationCategorySuggestions(SelectedConversationDataKind))
            {
                ConversationCategorySuggestions.Add(category);
                ConversationCategoryFilters.Add(category);
            }

            if (SelectedProfile != null && SelectedProfile.ConversationEntries != null)
            {
                foreach (string category in SelectedProfile.ConversationEntries
                    .Where(entry => entry.Kind == SelectedConversationDataKind)
                    .Select(entry => entry.Category)
                    .Where(category => !string.IsNullOrWhiteSpace(category))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(category => category))
                {
                    if (!ConversationCategoryFilters.Contains(category, StringComparer.OrdinalIgnoreCase))
                    {
                        ConversationCategoryFilters.Add(category);
                    }
                }
            }

            SelectedConversationCategorySuggestion = ConversationCategorySuggestions.FirstOrDefault() ?? string.Empty;
            SelectedConversationCategoryFilter = "All";
        }

        private void RefreshConversationActionSuggestions()
        {
            ConversationActionSuggestions.Clear();
            IEnumerable<string> actions = SelectedConversationDataKind == ConversationDataKind.ScheduledEvents
                ? ConversationValueCatalog.ScheduledEventActions
                : ConversationValueCatalog.Actions;

            foreach (string action in actions)
            {
                ConversationActionSuggestions.Add(action);
            }

            SelectedConversationActionSuggestion = ConversationActionSuggestions.FirstOrDefault() ?? string.Empty;
        }

        private static ConversationEntry CreateConversationEntry(ConversationDataKind kind, ObservableCollection<ConversationEntry> existingEntries)
        {
            int nextNumber = existingEntries.Count(entry => entry.Kind == kind) + 1;
            ConversationEntry entry = new ConversationEntry
            {
                Kind = kind,
                Id = CreateConversationEntryId(kind, nextNumber),
                Title = GetConversationKindDisplayName(kind) + " " + nextNumber,
                Category = CreateDefaultConversationCategory(kind),
                Priority = 100
            };
            if (kind == ConversationDataKind.Conversations)
            {
                entry.Id = $"Conv_Daily_General_{nextNumber:D2}";
                entry.Conditions.MaxAffection = 9999;
            }
            entry.Lines.Add(new ConversationLine());
            if (kind == ConversationDataKind.ScheduledEvents)
            {
                entry.Conditions.ActionId = "AutoWalkForest";
                entry.Conditions.TimeOfDay = "Noon";
                entry.Lines[0].Text = "今日は昼に森へ出かける予定です。";
                entry.Lines.Add(new ConversationLine
                {
                    Speaker = "Heroine",
                    Text = "森を歩きながら、静かな時間を過ごしました。",
                    Expression = "Neutral"
                });
            }
            return entry;
        }

        private static string CreateConversationEntryId(ConversationDataKind kind, int number)
        {
            string prefix;
            switch (kind)
            {
                case ConversationDataKind.GameEvents:
                    prefix = "Event";
                    break;
                case ConversationDataKind.ScheduledEvents:
                    prefix = "Scheduled";
                    break;
                case ConversationDataKind.ActionReactions:
                    prefix = "Reaction";
                    break;
                case ConversationDataKind.Endings:
                    prefix = "Ending";
                    break;
                default:
                    prefix = "Conv";
                    break;
            }

            return prefix + "_" + number.ToString("D2");
        }

        private static string CreateConversationEntryId(ConversationDataKind kind, string category, ObservableCollection<ConversationEntry> existingEntries, ConversationEntry currentEntry)
        {
            string prefix;
            switch (kind)
            {
                case ConversationDataKind.GameEvents:
                    prefix = "Event";
                    break;
                case ConversationDataKind.ScheduledEvents:
                    prefix = "Scheduled";
                    break;
                case ConversationDataKind.ActionReactions:
                    prefix = "Reaction";
                    break;
                case ConversationDataKind.Endings:
                    prefix = "Ending";
                    break;
                default:
                    prefix = "Conv";
                    break;
            }

            string normalizedCategory = NormalizeIdPart(category);
            int nextNumber = existingEntries.Count(entry => !ReferenceEquals(entry, currentEntry) && entry.Kind == kind && (entry.Id ?? string.Empty).StartsWith(prefix + "_" + normalizedCategory, StringComparison.OrdinalIgnoreCase)) + 1;
            return prefix + "_" + normalizedCategory + "_" + nextNumber.ToString("D2");
        }

        private static string NormalizeIdPart(string value)
        {
            string text = string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();
            char[] chars = text
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '_')
                .ToArray();
            return chars.Length == 0 ? "General" : new string(chars);
        }

        private static string[] GetConversationCategorySuggestions(ConversationDataKind kind)
        {
            switch (kind)
            {
                case ConversationDataKind.GameEvents:
                    return new[] { "Intro", "DayStart", "Location", "Date", "Quest", "Weather", "Season", "Scheduled" };
                case ConversationDataKind.ScheduledEvents:
                    return ConversationValueCatalog.ScheduledEventTypes;
                case ConversationDataKind.ActionReactions:
                    return new[] { "Tea", "Rest", "Walk", "Gift", "Talk" };
                case ConversationDataKind.Endings:
                    return new[] { "Good", "Normal", "Bad" };
                default:
                    return ConversationValueCatalog.ConversationGenres;
            }
        }

        private static string[] SplitConversationList(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<string>();
            }

            return text
                .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct()
                .ToArray();
        }

        private static string CreateDefaultConversationCategory(ConversationDataKind kind)
        {
            switch (kind)
            {
                case ConversationDataKind.GameEvents:
                    return "Intro";
                case ConversationDataKind.ScheduledEvents:
                    return "SoloForest";
                case ConversationDataKind.ActionReactions:
                    return "Action";
                case ConversationDataKind.Endings:
                    return "Good";
                default:
                    return "Daily";
            }
        }

        private class ScheduledEventTemplate
        {
            public ScheduledEventTemplate(
                string scheduleType,
                string title,
                string actionId,
                string triggerTimeSlot,
                string locationId,
                string preparationMessage,
                string eventMessage)
            {
                ScheduleType = scheduleType;
                Title = title;
                ActionId = actionId;
                TriggerTimeSlot = triggerTimeSlot;
                LocationId = locationId;
                PreparationMessage = preparationMessage;
                EventMessage = eventMessage;
            }

            public string ScheduleType { get; }

            public string Title { get; }

            public string ActionId { get; }

            public string TriggerTimeSlot { get; }

            public string LocationId { get; }

            public string PreparationMessage { get; }

            public string EventMessage { get; }
        }

        private static string GetConversationKindDisplayName(ConversationDataKind kind)
        {
            switch (kind)
            {
                case ConversationDataKind.GameEvents:
                    return "イベント";
                case ConversationDataKind.ScheduledEvents:
                    return "予定イベント";
                case ConversationDataKind.ActionReactions:
                    return "行動反応";
                case ConversationDataKind.Endings:
                    return "エンディング";
                default:
                    return "会話";
            }
        }

        private void OpenExportDirectory()
        {
            try
            {
                string directory = LastExportReport != null && !string.IsNullOrWhiteSpace(LastExportReport.ExportPath)
                    ? LastExportReport.ExportPath
                    : ExportPath;

                Directory.CreateDirectory(directory);
                Process.Start(new ProcessStartInfo(directory)
                {
                    UseShellExecute = true
                });
                StatusMessage = $"Export フォルダを開きました: {directory}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export フォルダを開けませんでした: {ex.Message}";
            }
        }

        private void LoadProfiles()
        {
            try
            {
                string previousHeroineId = SelectedProfile == null ? string.Empty : SelectedProfile.HeroineId;
                Profiles.Clear();
                foreach (HeroineProfile profile in characterProjectService.LoadProfiles())
                {
                    Profiles.Add(profile);
                }

                SelectedProfile = null;
                if (!string.IsNullOrWhiteSpace(previousHeroineId))
                {
                    SelectProfile(previousHeroineId);
                }

                if (SelectedProfile == null && Profiles.Count > 0)
                {
                    SelectedProfile = Profiles[0];
                }

                RefreshProductionStatus();
            }
            catch (Exception ex)
            {
                StatusMessage = $"読み込みに失敗しました: {ex.Message}";
            }
        }

        private void RefreshProductionStatus()
        {
            ProductionStatusCategories.Clear();
            if (SelectedProfile == null)
            {
                return;
            }

            CharacterProductionStatusRow row = CharacterProductionStatusService.Evaluate(
                SelectedProfile,
                ExpressionDefinitions,
                CostumeDefinitions,
                LayerAssetDefinitions,
                asset => asset != null && !string.IsNullOrWhiteSpace(asset.StoredPath) && File.Exists(Path.Combine(
                    characterProjectService.GetCharacterDirectory(SelectedProfile.HeroineId), asset.StoredPath)),
                exportService.Validate(SelectedProfile),
                StillDefinitions,
                allAudioLibraryItems,
                AudioLibraryService.IsUnityProjectPath(UnityProjectPath));
            foreach (ProductionStatusCell cell in new[]
            {
                row.BasicInformation,
                row.BattleMessages,
                row.TrainingImages,
                row.TrainingDialogues,
                row.CharacterImages,
                row.Conversations,
                row.Expressions,
                row.Costumes,
                row.BattleSkills,
                row.SkillTree,
                row.Events,
                row.ActionReactions,
                row.MenuActions,
                row.Voice,
                row.ExportReadiness
            })
            {
                if (!ShowOnlyIncompleteProductionStatus || cell.Kind != ProductionStatusKind.Complete)
                {
                    ProductionStatusCategories.Add(cell);
                }
            }
        }

        private void OpenProductionStatusTarget(object targetInfo)
        {
            ProductionStatusCell cell = targetInfo as ProductionStatusCell;
            ProductionStatusCheckItem check = targetInfo as ProductionStatusCheckItem;
            if (cell == null && check == null)
            {
                return;
            }

            string characterId = cell?.CharacterId ?? check.CharacterId;
            int tabIndex = cell?.TargetTabIndex ?? check.TargetTabIndex;
            string details = cell?.Details ?? check.Details;

            HeroineProfile target = Profiles.FirstOrDefault(profile =>
                string.Equals(profile.HeroineId, characterId, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                SelectedProfile = target;
            }

            SelectedMainTabIndex = tabIndex;
            if (check != null)
            {
                SelectProductionStatusDetail(check);
            }
            StatusMessage = $"{characterId} の修正対象へ移動しました。{details}";
        }

        private void SelectProductionStatusDetail(ProductionStatusCheckItem check)
        {
            if (SelectedProfile == null) return;
            switch (check.TargetKind)
            {
                case ProductionStatusTargetKind.Conversation:
                    SelectedConversationDataKind = check.ConversationKind;
                    RefreshFilteredConversationEntries();
                    SelectedConversationEntry = string.IsNullOrWhiteSpace(check.TargetId) ? null :
                        FilteredConversationEntries.FirstOrDefault(x =>
                            string.Equals(x.Id, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    break;
                case ProductionStatusTargetKind.Asset:
                    SelectedAssetStatusFilter = "All";
                    SelectedAsset = SelectedProfile.Assets?.FirstOrDefault(x =>
                        string.Equals(x.AssetId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    break;
                case ProductionStatusTargetKind.BattleSkill:
                    IsBattleSkillEditorExpanded = true;
                    SelectedProductionBattleSkill = SelectedProfile.BattleSkills?.FirstOrDefault(x =>
                        string.Equals(x.SkillId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    break;
                case ProductionStatusTargetKind.TrainingSkill:
                    IsSkillTreeEditorExpanded = true;
                    SelectedProductionTrainingSkill = SelectedProfile.HeroineSkillTree?.TrainingSkills?.FirstOrDefault(x =>
                        string.Equals(x.SkillId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    break;
                case ProductionStatusTargetKind.TrainingDialogue:
                    string normalizedState = TrainingDialogueSyncService.NormalizeVisualState(check.TargetSubId);
                    string assetId = $"Training_{check.TargetId}_{normalizedState}";
                    HeroineAsset trainingAsset = SelectedProfile.Assets?.FirstOrDefault(x =>
                        string.Equals(x.AssetId, assetId, StringComparison.OrdinalIgnoreCase));
                    if (trainingAsset != null) SelectedTrainingAsset = trainingAsset;
                    SelectedTrainingDialogueEntry = SelectedProfile.TrainingDialogues?.Items?.FirstOrDefault(x =>
                        string.Equals(x.TrainingId, check.TargetId, StringComparison.Ordinal) &&
                        string.Equals(TrainingDialogueSyncService.NormalizeVisualState(x.VisualState), normalizedState, StringComparison.Ordinal));
                    SelectedTrainingDialogueMessage = SelectedTrainingDialogueEntry?.Messages?.FirstOrDefault(x =>
                        x == null || string.IsNullOrWhiteSpace(x.Text)) ?? SelectedTrainingDialogueEntry?.Messages?.FirstOrDefault();
                    break;
                case ProductionStatusTargetKind.SkillTreeNode:
                    IsSkillTreeEditorExpanded = true;
                    SelectedProductionSkillTreeNode = SelectedProfile.HeroineSkillTree?.Nodes?.FirstOrDefault(x =>
                        string.Equals(x.NodeId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    break;
                case ProductionStatusTargetKind.Expression:
                    SelectedExpressionDefinition = ExpressionDefinitions.FirstOrDefault(x =>
                        string.Equals(x.ExpressionId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    break;
                case ProductionStatusTargetKind.Costume:
                    SelectedCostumeDefinition = CostumeDefinitions.FirstOrDefault(x =>
                        string.Equals(x.CostumeId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    break;
                case ProductionStatusTargetKind.LayerAsset:
                    SelectedLayerAssetDefinition = LayerAssetDefinitions.FirstOrDefault(x =>
                        string.Equals(x.AssetId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    if (SelectedLayerAssetDefinition != null &&
                        !string.IsNullOrWhiteSpace(SelectedLayerAssetDefinition.CostumeId))
                    {
                        SelectedCostumeDefinition = CostumeDefinitions.FirstOrDefault(x =>
                            string.Equals(
                                x.CostumeId,
                                SelectedLayerAssetDefinition.CostumeId,
                                StringComparison.OrdinalIgnoreCase));
                    }
                    break;
                case ProductionStatusTargetKind.StillDefinition:
                    StillDefinition still = StillDefinitions.FirstOrDefault(x =>
                        string.Equals(x.AssetId, check.TargetId, StringComparison.OrdinalIgnoreCase));
                    if (still != null)
                    {
                        SelectedStillUsageFilter = still.Usage.ToString();
                        SelectedStillDefinition = still;
                    }
                    break;
                case ProductionStatusTargetKind.Audio:
                    ShowOnlyUnusedVoice = false;
                    VoiceAudioSearchText = string.Empty;
                    RefreshFilteredAudioLibrary();
                    SelectedAudioLibraryItem = string.IsNullOrWhiteSpace(check.TargetId)
                        ? null
                        : VoiceAudioItems.FirstOrDefault(item =>
                            string.Equals(
                                item.LogicalId,
                                check.TargetId,
                                StringComparison.OrdinalIgnoreCase));
                    break;
            }
        }

        private void LoadEnemies()
        {
            try
            {
                string previousEnemyId = SelectedEnemyProfile == null ? string.Empty : SelectedEnemyProfile.EnemyId;
                EnemyProfiles.Clear();
                foreach (EnemyProfile profile in enemyProjectService.LoadProfiles())
                {
                    EnemyProfiles.Add(profile);
                }

                SelectedEnemyProfile = null;
                if (!string.IsNullOrWhiteSpace(previousEnemyId))
                {
                    SelectEnemy(previousEnemyId);
                }

                if (SelectedEnemyProfile == null && EnemyProfiles.Count > 0)
                {
                    SelectedEnemyProfile = EnemyProfiles[0];
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"敵キャラ読み込みに失敗しました: {ex.Message}";
            }
        }

        private void SelectProfile(string heroineId)
        {
            foreach (HeroineProfile profile in Profiles)
            {
                if (profile.HeroineId == heroineId)
                {
                    SelectedProfile = profile;
                    return;
                }
            }
        }

        private void SelectEnemy(string enemyId)
        {
            foreach (EnemyProfile profile in EnemyProfiles)
            {
                if (profile.EnemyId == enemyId)
                {
                    SelectedEnemyProfile = profile;
                    return;
                }
            }
        }

        private void RefreshSelectedStillStatus()
        {
            SelectedStillImagePath = string.Empty;

            if (SelectedProfile == null || SelectedStillDefinition == null)
            {
                SelectedStillAssetStatusText = "Asset: 未選択";
                SelectedStillImageStatusText = "画像: 未選択";
                SelectedStillPromptStatusText = "Prompt: 未選択";
                SelectedStillImageMessage = "スチルを選択してください。";
                return;
            }

            HeroineAsset asset = FindAssetForStill(SelectedProfile, SelectedStillDefinition);
            string promptPath = GetStillPromptRecordPath(SelectedProfile, SelectedStillDefinition);
            SelectedStillPromptStatusText = File.Exists(promptPath) ? "Prompt: 保存済み" : "Prompt: 未保存";

            if (asset == null)
            {
                SelectedStillAssetStatusText = "Asset: 未作成";
                SelectedStillImageStatusText = "画像: 未登録";
                SelectedStillImageMessage = "対応する Asset がありません。Prompt に反映すると作業用 Asset を作成します。";
                return;
            }

            SelectedStillAssetStatusText = "AssetStatus: " + asset.Status;

            if (string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                SelectedStillImageStatusText = "画像: 未登録";
                SelectedStillImageMessage = "画像はまだ登録されていません。";
                return;
            }

            string imagePath = Path.Combine(
                characterProjectService.GetCharacterDirectory(SelectedProfile.HeroineId),
                asset.StoredPath);

            if (!File.Exists(imagePath))
            {
                SelectedStillImageStatusText = "画像: ファイルなし";
                SelectedStillImageMessage = "画像ファイルが見つかりません: " + imagePath;
                return;
            }

            SelectedStillImagePath = imagePath;
            SelectedStillImageStatusText = "画像: 登録済み";
            SelectedStillImageMessage = AppendImageInspectionMessage(imagePath, asset);
        }

        private static HeroineAsset FindAssetForStill(HeroineProfile profile, StillDefinition stillDefinition)
        {
            if (profile.Assets == null)
            {
                return null;
            }

            return profile.Assets.FirstOrDefault(asset => asset.AssetId == stillDefinition.AssetId);
        }

        private string GetStillPromptRecordPath(HeroineProfile profile, StillDefinition stillDefinition)
        {
            return Path.Combine(
                characterProjectService.GetCharacterDirectory(profile.HeroineId),
                "Prompts",
                stillDefinition.AssetId + ".prompt.json");
        }

        private class EnemyAssetTemplate
        {
            public string AssetId { get; }

            public string Memo { get; }

            public string Prompt { get; }

            public EnemyAssetTemplate(string assetId, string memo, string prompt)
            {
                AssetId = assetId;
                Memo = memo;
                Prompt = prompt;
            }
        }

        private class PlayerAssetTemplate
        {
            public string AssetId { get; }

            public string Memo { get; }

            public string Prompt { get; }

            public PlayerAssetTemplate(string assetId, string memo, string prompt)
            {
                AssetId = assetId;
                Memo = memo;
                Prompt = prompt;
            }
        }
    }
}
