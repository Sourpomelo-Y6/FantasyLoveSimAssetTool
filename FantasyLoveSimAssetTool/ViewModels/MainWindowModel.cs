using FantasyLoveSimAssetTool.Common;
using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.ViewModels
{
    public class MainWindowModel : ObservableObject
    {
        private readonly CharacterProjectService characterProjectService;
        private readonly PromptRecordService promptRecordService;
        private readonly PromptTemplateService promptTemplateService;
        private readonly ExportService exportService;
        private string heroineIdInput;
        private string displayNameInput;
        private string assetIdInput;
        private string imageSourcePathInput;
        private AssetUsage selectedAssetUsage;
        private AssetStatus selectedAssetStatus;
        private HeroineAsset selectedAsset;
        private PromptTemplate selectedPromptTemplate;
        private PromptRecord currentPromptRecord;
        private HeroineProfile selectedProfile;
        private ExportReport lastExportReport;
        private string selectedAssetImagePath;
        private string selectedAssetImageMessage;
        private string statusMessage;

        public ObservableCollection<HeroineProfile> Profiles { get; }

        public ObservableCollection<AssetUsage> AssetUsages { get; }

        public ObservableCollection<AssetStatus> AssetStatuses { get; }

        public ObservableCollection<PromptTemplate> AvailablePromptTemplates { get; }

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
                SelectedAsset = null;
                if (selectedProfile != null && selectedProfile.Assets.Count > 0)
                {
                    SelectedAsset = selectedProfile.Assets[0];
                }
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

        public ICommand SavePromptRecordCommand { get; }

        public ICommand ApplyPromptTemplateCommand { get; }

        public ICommand ExportSelectedProfileCommand { get; }

        public MainWindowModel()
        {
            characterProjectService = new CharacterProjectService();
            promptRecordService = new PromptRecordService(characterProjectService);
            promptTemplateService = new PromptTemplateService();
            exportService = new ExportService(characterProjectService);
            Profiles = new ObservableCollection<HeroineProfile>();
            AvailablePromptTemplates = new ObservableCollection<PromptTemplate>();
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
            heroineIdInput = "TestHeroine";
            displayNameInput = "テストヒロイン";
            assetIdInput = "Heroine_Normal";
            imageSourcePathInput = string.Empty;
            selectedAssetUsage = AssetUsage.Sprites;
            selectedAssetStatus = AssetStatus.Accepted;
            lastExportReport = new ExportReport();
            selectedAssetImagePath = string.Empty;
            selectedAssetImageMessage = "画像を選択してください。";
            statusMessage = string.Empty;

            CreateCharacterCommand = new RelayCommand(CreateCharacter);
            SaveSelectedProfileCommand = new RelayCommand(SaveSelectedProfile, () => SelectedProfile != null);
            RefreshProfilesCommand = new RelayCommand(LoadProfiles);
            BrowseImageCommand = new RelayCommand(BrowseImage);
            AddImageAssetCommand = new RelayCommand(AddImageAsset, () => SelectedProfile != null);
            SavePromptRecordCommand = new RelayCommand(
                SavePromptRecord,
                () => SelectedProfile != null && SelectedAsset != null && CurrentPromptRecord != null);
            ApplyPromptTemplateCommand = new RelayCommand(
                ApplyPromptTemplate,
                () => SelectedProfile != null && SelectedPromptTemplate != null && CurrentPromptRecord != null);
            ExportSelectedProfileCommand = new RelayCommand(ExportSelectedProfile, () => SelectedProfile != null);

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
    }
}
