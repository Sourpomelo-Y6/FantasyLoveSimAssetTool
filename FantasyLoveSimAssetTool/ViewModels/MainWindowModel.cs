using FantasyLoveSimAssetTool.Common;
using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.ViewModels
{
    public class MainWindowModel : ObservableObject
    {
        private readonly CharacterProjectService characterProjectService;
        private readonly PromptRecordService promptRecordService;
        private readonly PromptTemplateService promptTemplateService;
        private readonly StillDefinitionService stillDefinitionService;
        private readonly ExportService exportService;
        private string heroineIdInput;
        private string displayNameInput;
        private string assetIdInput;
        private string imageSourcePathInput;
        private AssetUsage selectedAssetUsage;
        private AssetStatus selectedAssetStatus;
        private HeroineAsset selectedAsset;
        private PromptTemplate selectedPromptTemplate;
        private StillDefinition selectedStillDefinition;
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
        private string statusMessage;

        public ObservableCollection<HeroineProfile> Profiles { get; }

        public ObservableCollection<AssetUsage> AssetUsages { get; }

        public ObservableCollection<AssetStatus> AssetStatuses { get; }

        public ObservableCollection<PromptTemplate> AvailablePromptTemplates { get; }

        public ObservableCollection<StillDefinition> StillDefinitions { get; }

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

        public HeroineAsset SelectedAsset
        {
            get { return selectedAsset; }
            set
            {
                if (selectedAsset == value) { return; }
                selectedAsset = value;
                OnPropertyChanged(nameof(SelectedAsset));
                RefreshSelectedAssetImagePath();
                RefreshPromptTemplates();
                LoadPromptForSelectedAsset();
                CommandManager.InvalidateRequerySuggested();
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

                OnPropertyChanged(nameof(SelectedStillDefinition));
                OnPropertyChanged(nameof(StillPromptPreview));
                RefreshSelectedStillStatus();
                CommandManager.InvalidateRequerySuggested();
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
                SelectedAsset = null;
                if (selectedProfile != null && selectedProfile.Assets.Count > 0)
                {
                    SelectedAsset = selectedProfile.Assets[0];
                }
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

        public MainWindowModel()
        {
            characterProjectService = new CharacterProjectService();
            promptRecordService = new PromptRecordService(characterProjectService);
            promptTemplateService = new PromptTemplateService();
            stillDefinitionService = new StillDefinitionService();
            exportService = new ExportService(characterProjectService);
            Profiles = new ObservableCollection<HeroineProfile>();
            AvailablePromptTemplates = new ObservableCollection<PromptTemplate>();
            StillDefinitions = new ObservableCollection<StillDefinition>();
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
            lastExportReport = new ExportReport();
            selectedAssetImagePath = string.Empty;
            selectedAssetImageMessage = "画像を選択してください。";
            selectedStillAssetStatusText = "Asset: 未選択";
            selectedStillImageStatusText = "画像: 未選択";
            selectedStillPromptStatusText = "Prompt: 未選択";
            selectedStillImagePath = string.Empty;
            selectedStillImageMessage = "スチルを選択してください。";
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

        private void SelectedStillDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StillDefinition.SpecificPrompt))
            {
                OnPropertyChanged(nameof(StillPromptPreview));
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

        private void AddImageAsset()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            try
            {
                HeroineAsset asset = characterProjectService.AddImageAsset(
                    SelectedProfile,
                    ImageSourcePathInput,
                    SelectedAssetUsage,
                    AssetIdInput,
                    SelectedAssetStatus);

                SelectedAsset = asset;
                RefreshSelectedStillStatus();
                StatusMessage = $"{asset.AssetId} を {asset.Usage} に登録しました。";
            }
            catch (Exception ex)
            {
                StatusMessage = $"画像登録に失敗しました: {ex.Message}";
            }
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
            SelectedAssetImageMessage = imagePath;
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

            if (SelectedAsset == null)
            {
                return;
            }

            foreach (PromptTemplate template in promptTemplateService.GetTemplates(SelectedAsset.Usage))
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

            if (StillDefinitions.Count > 0)
            {
                SelectedStillDefinition = StillDefinitions[0];
            }
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
            SelectedStillImageMessage = imagePath;
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
