using FantasyLoveSimAssetTool.Common;
using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.ViewModels
{
    public class MainWindowModel : ObservableObject
    {
        private readonly CharacterProjectService characterProjectService;
        private readonly PromptRecordService promptRecordService;
        private readonly PromptTemplateService promptTemplateService;
        private readonly StillDefinitionService stillDefinitionService;
        private readonly ImageInspectionService imageInspectionService;
        private readonly ComfySettingsService comfySettingsService;
        private readonly ComfyWorkflowService comfyWorkflowService;
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
        private string currentComfyWorkflowPreview;
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
                CurrentComfyWorkflowPreview = string.Empty;
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
                selectedProfile = value;
                OnPropertyChanged(nameof(SelectedProfile));
                OnPropertyChanged(nameof(StillPromptPreview));
                CurrentComfyWorkflowPreview = string.Empty;
                LoadStillDefinitions();
                RefreshFilteredAssets();
                RefreshAcceptedAssets();
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

        public ICommand SaveImageAssetsCommand { get; }

        public ICommand SavePromptRecordCommand { get; }

        public ICommand ApplyPromptTemplateCommand { get; }

        public ICommand ApplyStillPromptCommand { get; }

        public ICommand PrepareImageRegistrationForStillCommand { get; }

        public ICommand ExportSelectedProfileCommand { get; }

        public ICommand OpenExportDirectoryCommand { get; }

        public ICommand ReloadComfySettingsCommand { get; }

        public ICommand BuildComfyWorkflowPreviewCommand { get; }

        public ICommand BuildStillComfyWorkflowPreviewCommand { get; }

        public MainWindowModel()
        {
            characterProjectService = new CharacterProjectService();
            promptRecordService = new PromptRecordService(characterProjectService);
            promptTemplateService = new PromptTemplateService(characterProjectService.WorkspaceRoot);
            stillDefinitionService = new StillDefinitionService();
            imageInspectionService = new ImageInspectionService();
            comfySettingsService = new ComfySettingsService(characterProjectService.WorkspaceRoot);
            comfyWorkflowService = new ComfyWorkflowService(characterProjectService.WorkspaceRoot);
            exportService = new ExportService(characterProjectService, imageInspectionService);
            Profiles = new ObservableCollection<HeroineProfile>();
            FilteredAssets = new ObservableCollection<HeroineAsset>();
            AcceptedAssets = new ObservableCollection<HeroineAsset>();
            AvailablePromptTemplates = new ObservableCollection<PromptTemplate>();
            StillDefinitions = new ObservableCollection<StillDefinition>();
            FilteredStillDefinitions = new ObservableCollection<StillDefinition>();
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
            heroineIdInput = "TestHeroine";
            displayNameInput = "テストヒロイン";
            assetIdInput = "Heroine_Normal";
            imageSourcePathInput = string.Empty;
            selectedAssetUsage = AssetUsage.Sprites;
            selectedAssetStatus = AssetStatus.Accepted;
            selectedAssetStatusFilter = "All";
            selectedPromptTemplateUsage = AssetUsage.Sprites;
            selectedStillUsageFilter = "All";
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
            currentComfyWorkflowPreview = string.Empty;
            statusMessage = string.Empty;

            CreateCharacterCommand = new RelayCommand(CreateCharacter);
            SaveSelectedProfileCommand = new RelayCommand(SaveSelectedProfile, () => SelectedProfile != null);
            RefreshProfilesCommand = new RelayCommand(LoadProfiles);
            BrowseImageCommand = new RelayCommand(BrowseImage);
            AddImageAssetCommand = new RelayCommand(AddImageAsset, () => SelectedProfile != null);
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
            BuildComfyWorkflowPreviewCommand = new RelayCommand(
                BuildComfyWorkflowPreview,
                () => CurrentPromptRecord != null);
            BuildStillComfyWorkflowPreviewCommand = new RelayCommand(
                BuildStillComfyWorkflowPreview,
                () => SelectedProfile != null && SelectedStillDefinition != null);

            ReloadComfySettings();
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
            }
            catch (Exception ex)
            {
                ComfySettings = new ComfySettings();
                ComfySettingsSummary = BuildComfySettingsSummary(ComfySettings);
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
                PromptRecord promptRecord = CurrentPromptRecord ?? new PromptRecord();
                promptRecord.PositivePrompt = BuildStillPositivePrompt(SelectedProfile, SelectedStillDefinition);
                CurrentComfyWorkflowPreview = comfyWorkflowService.BuildWorkflowPreview(ComfySettings, promptRecord);
                StatusMessage = $"{SelectedStillDefinition.DisplayName} の ComfyUI workflow preview を作成しました。";
            }
            catch (Exception ex)
            {
                CurrentComfyWorkflowPreview = string.Empty;
                StatusMessage = $"スチル ComfyUI workflow preview 作成に失敗しました: {ex.Message}";
            }
        }

        private void SelectedStillDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StillDefinition.SpecificPrompt))
            {
                OnPropertyChanged(nameof(StillPromptPreview));
                CurrentComfyWorkflowPreview = string.Empty;
                UpdateStillWorkItemFromDefinition();
            }
            else if (e.PropertyName == nameof(StillDefinition.Status))
            {
                UpdateStillWorkItemFromDefinition();
            }
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
                bool overwriteExisting = ShouldOverwriteExistingAsset();
                if (overwriteExisting == false && HasExistingAssetId())
                {
                    StatusMessage = "画像登録をキャンセルしました。";
                    return;
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"画像登録に失敗しました: {ex.Message}";
            }
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

        private bool ShouldOverwriteExistingAsset()
        {
            if (!HasExistingAssetId())
            {
                return false;
            }

            MessageBoxResult result = MessageBox.Show(
                $"AssetId '{AssetIdInput.Trim()}' はすでに登録されています。画像と登録情報を上書きしますか？",
                "画像登録の上書き確認",
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
            string appearancePrompt = (profile.AppearancePrompt ?? string.Empty).Trim().TrimEnd(',');
            string specificPrompt = (stillDefinition.SpecificPrompt ?? string.Empty).Trim().TrimStart(',');

            if (string.IsNullOrWhiteSpace(appearancePrompt))
            {
                return specificPrompt;
            }

            if (string.IsNullOrWhiteSpace(specificPrompt))
            {
                return appearancePrompt;
            }

            return appearancePrompt + ", " + specificPrompt;
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
                StatusMessage = $"{SelectedProfile.HeroineId} を export しました。画像 {LastExportReport.ExportedImageCount}/{LastExportReport.AcceptedAssetCount} 件、prompt {LastExportReport.ExportedPromptCount} 件、警告 {LastExportReport.Warnings.Count} 件。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"export に失敗しました: {ex.Message}";
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
