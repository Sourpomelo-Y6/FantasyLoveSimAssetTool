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
        private readonly CharacterProjectService characterProjectService;
        private readonly PromptRecordService promptRecordService;
        private readonly PromptTemplateService promptTemplateService;
        private readonly DefinitionCatalogService definitionCatalogService;
        private StillDefinitionService stillDefinitionService;
        private readonly ImageInspectionService imageInspectionService;
        private readonly ComfySettingsService comfySettingsService;
        private readonly ComfyWorkflowService comfyWorkflowService;
        private readonly ComfyClientService comfyClientService;
        private readonly ExportService exportService;
        private string heroineIdInput;
        private string displayNameInput;
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
        private string gameEventTestItemId;
        private string gameEventTestFlagIdsText;
        private string selectedConversationCategorySuggestion;
        private string selectedConversationLocationSuggestion;
        private string selectedConversationActionSuggestion;
        private string selectedConversationWeatherSuggestion;
        private string selectedConversationSeasonSuggestion;
        private string selectedConversationTimeOfDaySuggestion;
        private string selectedConversationExpressionSuggestion;
        private HeroineAsset selectedConversationImageAsset;
        private ExpressionDefinition selectedExpressionDefinition;
        private CostumeDefinition selectedCostumeDefinition;
        private LayerAssetDefinition selectedLayerAssetDefinition;
        private string definitionCatalogValidationMessage;
        private string selectedLayerPreviewBaseBodyId;
        private string selectedLayerPreviewCostumeId;
        private string selectedLayerPreviewExpressionId;
        private string layerPreviewMessage;
        private string profilePreviewImagePath;
        private string profilePreviewMessage;
        private PromptRecord currentPromptRecord;
        private HeroineProfile selectedProfile;
        private ExportReport lastExportReport;
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

        public ObservableCollection<HeroineProfile> Profiles { get; }

        public ObservableCollection<AssetUsage> AssetUsages { get; }

        public ObservableCollection<AssetStatus> AssetStatuses { get; }

        public ObservableCollection<string> AssetStatusFilters { get; }

        public ObservableCollection<HeroineAsset> FilteredAssets { get; }

        public ObservableCollection<HeroineAsset> AcceptedAssets { get; }

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

        public ObservableCollection<string> ConversationExpressionSuggestions { get; }

        public ObservableCollection<ExpressionDefinition> ExpressionDefinitions { get; }

        public ObservableCollection<CostumeDefinition> CostumeDefinitions { get; }

        public ObservableCollection<LayerAssetDefinition> LayerAssetDefinitions { get; }

        public ObservableCollection<string> LayerKindOptions { get; }

        public ObservableCollection<string> ExpressionIdOptions { get; }

        public ObservableCollection<string> CostumeIdOptions { get; }

        public ObservableCollection<string> LayerPreviewBaseBodyOptions { get; }

        public ObservableCollection<string> LayerPreviewCostumeOptions { get; }

        public ObservableCollection<string> LayerPreviewExpressionOptions { get; }

        public ObservableCollection<LayerPreviewItem> LayerPreviewItems { get; }

        public ObservableCollection<LayerPreviewItem> ProfilePreviewItems { get; }

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
                RefreshStillPromptAfterProfilePromptChanged();
                LoadStillDefinitions();
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
                RefreshLayerPreviewOptions();
                RefreshLayerPreview();
                RefreshProfilePreview();
                RefreshConversationCategorySuggestions();
                RefreshFilteredConversationEntries();
                RefreshSelectedStillStatus();
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

        public ICommand CreateCharacterCommand { get; }

        public ICommand SaveSelectedProfileCommand { get; }

        public ICommand RefreshProfilesCommand { get; }

        public ICommand BrowseImageCommand { get; }

        public ICommand AddImageAssetCommand { get; }

        public ICommand UnregisterImageAssetCommand { get; }

        public ICommand SaveImageAssetsCommand { get; }

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

        public ICommand BuildStillComfyWorkflowPreviewCommand { get; }

        public ICommand SubmitStillComfyPromptCommand { get; }

        public ICommand CancelStillComfyPollingCommand { get; }

        public ICommand InterruptComfyGenerationCommand { get; }

        public ICommand CheckStillComfyResultCommand { get; }

        public ICommand FetchStillComfyImageCommand { get; }

        public ICommand AdoptStillComfyImageCommand { get; }

        public ICommand AddConversationEntryCommand { get; }

        public ICommand RemoveConversationEntryCommand { get; }

        public ICommand AddConversationLineCommand { get; }

        public ICommand RemoveConversationLineCommand { get; }

        public ICommand AddConversationChoiceCommand { get; }

        public ICommand RemoveConversationChoiceCommand { get; }

        public ICommand SaveConversationDataCommand { get; }

        public ICommand ImportActionsFromUnityCommand { get; }

        public ICommand ImportConversationsFromUnityCommand { get; }

        public ICommand ImportGameEventsFromUnityCommand { get; }

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

        public MainWindowModel()
        {
            characterProjectService = new CharacterProjectService();
            promptRecordService = new PromptRecordService(characterProjectService);
            promptTemplateService = new PromptTemplateService(characterProjectService.WorkspaceRoot);
            definitionCatalogService = new DefinitionCatalogService(characterProjectService.WorkspaceRoot);
            stillDefinitionService = new StillDefinitionService(characterProjectService.WorkspaceRoot);
            imageInspectionService = new ImageInspectionService();
            comfySettingsService = new ComfySettingsService(characterProjectService.WorkspaceRoot);
            comfyWorkflowService = new ComfyWorkflowService(characterProjectService.WorkspaceRoot);
            comfyClientService = new ComfyClientService();
            exportService = new ExportService(characterProjectService, imageInspectionService);
            Profiles = new ObservableCollection<HeroineProfile>();
            FilteredAssets = new ObservableCollection<HeroineAsset>();
            AcceptedAssets = new ObservableCollection<HeroineAsset>();
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
            ConversationExpressionSuggestions = new ObservableCollection<string>(ConversationValueCatalog.Expressions);
            ExpressionDefinitions = new ObservableCollection<ExpressionDefinition>();
            CostumeDefinitions = new ObservableCollection<CostumeDefinition>();
            LayerAssetDefinitions = new ObservableCollection<LayerAssetDefinition>();
            LayerKindOptions = new ObservableCollection<string>
            {
                "BaseBody",
                "Costume",
                "Expression",
                "Accessory"
            };
            ExpressionIdOptions = new ObservableCollection<string>();
            CostumeIdOptions = new ObservableCollection<string>();
            LayerPreviewBaseBodyOptions = new ObservableCollection<string>();
            LayerPreviewCostumeOptions = new ObservableCollection<string>();
            LayerPreviewExpressionOptions = new ObservableCollection<string>();
            LayerPreviewItems = new ObservableCollection<LayerPreviewItem>();
            ProfilePreviewItems = new ObservableCollection<LayerPreviewItem>();
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
                AssetUsage.Ending.ToString()
            };
            AssetUsages = new ObservableCollection<AssetUsage>
            {
                AssetUsage.Sprites,
                AssetUsage.Event,
                AssetUsage.Actions,
                AssetUsage.Ending
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
            heroineIdInput = "TestHeroine";
            displayNameInput = "テストヒロイン";
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

            CreateCharacterCommand = new RelayCommand(CreateCharacter);
            SaveSelectedProfileCommand = new RelayCommand(SaveSelectedProfile, () => SelectedProfile != null);
            RefreshProfilesCommand = new RelayCommand(LoadProfiles);
            BrowseImageCommand = new RelayCommand(BrowseImage);
            AddImageAssetCommand = new RelayCommand(AddImageAsset, () => SelectedProfile != null);
            UnregisterImageAssetCommand = new RelayCommand(
                UnregisterImageAsset,
                () => SelectedProfile != null && SelectedAsset != null);
            SaveImageAssetsCommand = new RelayCommand(SaveImageAssets, () => SelectedProfile != null);
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
            ImportActionsFromUnityCommand = new RelayCommand(
                ImportActionsFromUnity,
                () => SelectedProfile != null);
            ImportConversationsFromUnityCommand = new RelayCommand(
                ImportConversationsFromUnity,
                () => SelectedProfile != null);
            ImportGameEventsFromUnityCommand = new RelayCommand(
                ImportGameEventsFromUnity,
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

            ReloadComfySettings();
            LoadDefinitionCatalog();
            RefreshConversationCategorySuggestions();
            LoadStillDefinitions();
            LoadProfiles();
            StatusMessage = "キャラクター基本情報の保存準備ができています。";
        }

        private void CreateCharacter()
        {
            try
            {
                HeroineProfile profile = characterProjectService.CreateCharacter(HeroineIdInput, DisplayNameInput);
                LoadProfiles();
                SelectProfile(profile.HeroineId);
                StatusMessage = $"{profile.HeroineId} を作成しました。";
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

            string[] promptParts =
            {
                NormalizePromptPart(commonPrompt),
                NormalizePromptPart(additionPrompt)
            };

            return string.Join(", ", promptParts.Where(part => !string.IsNullOrWhiteSpace(part)));
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
            string layerKind = string.IsNullOrWhiteSpace(expressionId) ? "Costume" : "Expression";
            string baseId = layerKind == "Expression"
                ? "Expression_" + (string.IsNullOrWhiteSpace(expressionId) ? "New" : expressionId)
                : "Costume_" + (string.IsNullOrWhiteSpace(costumeId) ? "New" : costumeId);
            LayerAssetDefinition layer = new LayerAssetDefinition
            {
                AssetId = BuildUniqueId(
                    baseId,
                    LayerAssetDefinitions.Where(item => item != null).Select(item => item.AssetId)),
                LayerKind = layerKind,
                CostumeId = layerKind == "Costume" ? costumeId : string.Empty,
                ExpressionId = layerKind == "Expression" ? expressionId : string.Empty,
                DisplayName = layerKind == "Expression" ? "レイヤー: 表情" : "レイヤー: 衣装",
                FileName = baseId + ".png",
                DrawOrder = layerKind == "Expression" ? 200 : 100,
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
                    .Where(layer => layer != null && IsLayerKind(layer, "BaseBody"))
                    .Select(layer => layer.AssetId),
                includeEmpty: true);
            RefreshStringOptions(
                LayerPreviewCostumeOptions,
                CostumeDefinitions
                    .Where(costume => costume != null)
                    .Select(costume => costume.CostumeId),
                includeEmpty: true);
            RefreshStringOptions(
                LayerPreviewExpressionOptions,
                ExpressionDefinitions
                    .Where(expression => expression != null)
                    .Select(expression => expression.ExpressionId),
                includeEmpty: true);

            selectedLayerPreviewBaseBodyId = SelectExistingOrFirst(previousBaseBody, LayerPreviewBaseBodyOptions);
            selectedLayerPreviewCostumeId = SelectExistingOrFirst(previousCostume, LayerPreviewCostumeOptions);
            selectedLayerPreviewExpressionId = SelectExistingOrFirst(previousExpression, LayerPreviewExpressionOptions);
            OnPropertyChanged(nameof(SelectedLayerPreviewBaseBodyId));
            OnPropertyChanged(nameof(SelectedLayerPreviewCostumeId));
            OnPropertyChanged(nameof(SelectedLayerPreviewExpressionId));
        }

        private void RefreshLayerPreview()
        {
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
                    warnings.Add($"{layer.AssetId}: Accepted 画像が登録されていません。");
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
                .Where(layer => layer != null && IsLayerKind(layer, "Accessory"))
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
            AddSelectedLayer(layers, layer => IsLayerKind(layer, "BaseBody")
                && string.Equals(layer.AssetId, SelectedLayerPreviewBaseBodyId, StringComparison.OrdinalIgnoreCase));
            AddSelectedLayer(layers, layer => IsLayerKind(layer, "Costume")
                && string.Equals(layer.CostumeId, SelectedLayerPreviewCostumeId, StringComparison.OrdinalIgnoreCase));
            AddSelectedLayer(layers, layer => IsLayerKind(layer, "Expression")
                && string.Equals(layer.ExpressionId, SelectedLayerPreviewExpressionId, StringComparison.OrdinalIgnoreCase));

            foreach (LayerAssetDefinition accessory in LayerAssetDefinitions
                .Where(layer => layer != null && IsLayerKind(layer, "Accessory"))
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
            return layer != null
                && string.Equals(layer.LayerKind?.Trim(), layerKind, StringComparison.OrdinalIgnoreCase);
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

                if (string.Equals(layerKind, "Expression", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(expressionId))
                {
                    warnings.Add($"レイヤー素材定義 {label}: 種類が Expression の場合は表情IDを指定してください。");
                }

                if (string.Equals(layerKind, "Costume", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(costumeId))
                {
                    warnings.Add($"レイヤー素材定義 {label}: 種類が Costume の場合は衣装IDを指定してください。");
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
            RefreshFilteredStillDefinitions();
        }

        private void RefreshFilteredAssets()
        {
            HeroineAsset previousSelection = SelectedAsset;
            FilteredAssets.Clear();

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
                StatusMessage = $"{SelectedProfile.HeroineId} を保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存に失敗しました: {ex.Message}";
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
                StatusMessage = $"{SelectedProfile.HeroineId} の会話データを保存しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"会話データ保存に失敗しました: {ex.Message}";
            }
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
                if (HasExistingActionReaction(actionId))
                {
                    skippedCount++;
                    continue;
                }

                ConversationEntry entry = CreateActionReactionFromUnityAction(item);
                SelectedProfile.ConversationEntries.Add(entry);
                importedEntries.Add(entry);
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

        private bool HasExistingActionReaction(string actionId)
        {
            if (SelectedProfile == null || SelectedProfile.ConversationEntries == null)
            {
                return false;
            }

            string generatedId = CreateActionReactionEntryId(actionId);
            return SelectedProfile.ConversationEntries.Any(entry => entry != null
                && entry.Kind == ConversationDataKind.ActionReactions
                && (string.Equals(entry.Id, generatedId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.Conditions?.ActionId, actionId, StringComparison.OrdinalIgnoreCase)));
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
                    Expression = line.Expression ?? string.Empty
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
            if (SelectedProfile == null || SelectedProfile.ConversationEntries == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return SelectedProfile.ConversationEntries.Any(entry => entry != null
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
                Category = string.IsNullOrWhiteSpace(item.Category) ? "LocationTalk" : item.Category.Trim(),
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
                    Expression = line.Expression ?? string.Empty
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
            target.MaxAffection = source.MaxAffection == 0 ? 100 : source.MaxAffection;
            target.Weather = source.Weather ?? string.Empty;
            target.Season = source.Season ?? string.Empty;
            target.TimeOfDay = source.TimeOfDay ?? string.Empty;
            target.ActionId = source.ActionId ?? string.Empty;
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
                    Expression = line.Expression ?? string.Empty
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
            target.MinAffection = source.MinAffection;
            target.MaxAffection = source.MaxAffection == 0 ? 100 : source.MaxAffection;
            target.Weather = source.Weather ?? string.Empty;
            target.Season = source.Season ?? string.Empty;
            target.TimeOfDay = source.TimeOfDay ?? string.Empty;
            target.ActionId = source.ActionId ?? string.Empty;
            target.RequiredItemId = source.RequiredItemId ?? string.Empty;
            target.Once = source.Once;
            target.RequiredFlagIdsText = JoinImportList(source.RequiredFlagIds);
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
            int skippedCount = 0;

            foreach (FromUnityEndingItem item in endingData.Items ?? new List<FromUnityEndingItem>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    skippedCount++;
                    continue;
                }

                if (HasExistingConversationEntry(ConversationDataKind.Endings, item.Id.Trim()))
                {
                    skippedCount++;
                    continue;
                }

                ConversationEntry entry = CreateEndingFromUnityEnding(item);
                SelectedProfile.ConversationEntries.Add(entry);
                importedEntries.Add(entry);
            }

            characterProjectService.SaveProfile(SelectedProfile);
            SelectedConversationDataKind = ConversationDataKind.Endings;
            RefreshConversationCategorySuggestions();
            RefreshFilteredConversationEntries();
            if (importedEntries.Count > 0)
            {
                SelectedConversationEntry = importedEntries[0];
            }

            StatusMessage = $"FromUnity endings を取り込みました。追加 {importedEntries.Count} 件、スキップ {skippedCount} 件。";
        }

        private static ConversationEntry CreateEndingFromUnityEnding(FromUnityEndingItem item)
        {
            ConversationEntry entry = new ConversationEntry
            {
                Kind = ConversationDataKind.Endings,
                Id = item.Id.Trim(),
                Title = string.IsNullOrWhiteSpace(item.Title) ? item.Id.Trim() : item.Title.Trim(),
                Category = string.IsNullOrWhiteSpace(item.Category) ? "Normal" : item.Category.Trim(),
                ImageAssetIdsText = JoinImportList(item.ImageAssetIds),
                Priority = item.Priority,
                Memo = BuildFromUnityEndingMemo(item)
            };

            ApplyFromUnityEndingCondition(entry.Conditions, item.Conditions);

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
                    Text = line.Text ?? string.Empty,
                    Expression = line.Expression ?? string.Empty
                });
            }

            if (entry.Lines.Count == 0)
            {
                entry.Lines.Add(new ConversationLine
                {
                    Speaker = "Heroine",
                    Text = item.Message ?? string.Empty,
                    Expression = string.Empty
                });
            }

            return entry;
        }

        private static void ApplyFromUnityEndingCondition(
            ConversationCondition target,
            FromUnityEndingCondition source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.LocationId = source.LocationId ?? string.Empty;
            target.MinAffection = source.MinAffection;
            target.MaxAffection = source.MaxAffection == 0 ? 100 : source.MaxAffection;
            target.Weather = source.Weather ?? string.Empty;
            target.Season = source.Season ?? string.Empty;
            target.TimeOfDay = source.TimeOfDay ?? string.Empty;
            target.ActionId = source.ActionId ?? string.Empty;
            target.RequiredItemId = source.RequiredItemId ?? string.Empty;
            target.Once = source.Once;
            target.RequiredFlagIdsText = JoinImportList(source.RequiredFlagIds);
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
                    return new ScheduledEventTemplate("SoloShopping", "街への買い物", "AutoShopping", "Noon", "Town", "今日は昼に街へ買い物に出かける予定です。", "街を歩きながら、気になる店をいくつか見て回りました。");
                case "DuoForest":
                    return new ScheduledEventTemplate("DuoForest", "森への同行外出", "DuoWalkForest", "Noon", "Forest", "今日は昼に二人で森へ出かける予定です。", "二人で森を歩きながら、少しだけ距離が近づいた気がしました。");
                case "DuoCave":
                    return new ScheduledEventTemplate("DuoCave", "洞窟への同行外出", "DuoWalkCave", "Noon", "Cave", "今日は昼に二人で洞窟へ出かける予定です。", "二人で洞窟を進み、危ない場所では自然と声を掛け合いました。");
                case "DuoLake":
                    return new ScheduledEventTemplate("DuoLake", "湖への同行外出", "DuoWalkLake", "Noon", "Lake", "今日は昼に二人で湖へ出かける予定です。", "湖のほとりで、二人だけの落ち着いた時間を過ごしました。");
                case "DuoShopping":
                    return new ScheduledEventTemplate("DuoShopping", "街への同行買い物", "DuoShopping", "Noon", "Town", "今日は昼に二人で街へ買い物に出かける予定です。", "街で買い物をしながら、相手の好みを少し知ることができました。");
                case "StayHome":
                    return new ScheduledEventTemplate("StayHome", "家で過ごす予定", "StayHome", "Noon", "Room", "今日は昼を家で過ごす予定です。", "部屋でゆっくり過ごし、穏やかな時間になりました。");
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
                    prefix = "Talk";
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
                    prefix = "Talk";
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
                    return new[] { "LocationTalk", "AffectionTalk", "WeatherTalk", "SeasonTalk", "TimeTalk" };
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
                    return "LocationTalk";
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"読み込みに失敗しました: {ex.Message}";
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
    }
}
