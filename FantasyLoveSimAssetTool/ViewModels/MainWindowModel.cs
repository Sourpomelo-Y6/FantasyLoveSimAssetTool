using FantasyLoveSimAssetTool.Common;
using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.ViewModels
{
    public class MainWindowModel : ObservableObject
    {
        private readonly CharacterProjectService characterProjectService;
        private string heroineIdInput;
        private string displayNameInput;
        private HeroineProfile selectedProfile;
        private string statusMessage;

        public ObservableCollection<HeroineProfile> Profiles { get; }

        public string WorkspacePath
        {
            get { return characterProjectService.WorkspaceRoot; }
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

        public HeroineProfile SelectedProfile
        {
            get { return selectedProfile; }
            set
            {
                if (selectedProfile == value) { return; }
                selectedProfile = value;
                OnPropertyChanged(nameof(SelectedProfile));
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

        public MainWindowModel()
        {
            characterProjectService = new CharacterProjectService();
            Profiles = new ObservableCollection<HeroineProfile>();
            heroineIdInput = "TestHeroine";
            displayNameInput = "テストヒロイン";
            statusMessage = string.Empty;

            CreateCharacterCommand = new RelayCommand(CreateCharacter);
            SaveSelectedProfileCommand = new RelayCommand(SaveSelectedProfile, () => SelectedProfile != null);
            RefreshProfilesCommand = new RelayCommand(LoadProfiles);

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
