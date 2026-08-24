using FantasyLoveSimAssetTool.Common;
using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.ViewModels
{
    public sealed class LocalAiSettingsViewModel : ObservableObject, IDisposable
    {
        private readonly LocalAiSettingsService settingsService;
        private readonly ILocalLlmClient llmClient;
        private readonly IDisposable disposableClient;
        private CancellationTokenSource operationCancellation;
        private string serverUrl;
        private string selectedModelId;
        private string timeoutSecondsText;
        private string testPrompt;
        private string testResult;
        private string rawResponse;
        private string connectionStatus;
        private string connectionStatusColor;
        private string statusMessage;
        private bool isBusy;

        public LocalAiSettingsViewModel(string workspaceRoot)
            : this(new LocalAiSettingsService(workspaceRoot), new LocalLlmClient())
        {
        }

        public LocalAiSettingsViewModel(LocalAiSettingsService settingsService, ILocalLlmClient llmClient)
        {
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            disposableClient = llmClient as IDisposable;
            Models = new ObservableCollection<string>();
            RefreshModelsCommand = new AsyncRelayCommand(RefreshModelsAsync, () => !IsBusy);
            SendTestCommand = new AsyncRelayCommand(SendTestAsync, () => !IsBusy);
            SaveCommand = new RelayCommand(Save, () => !IsBusy);
            CancelCommand = new RelayCommand(Cancel, () => IsBusy);
            testPrompt = "「通信成功」と簡潔に答えてください。";
            testResult = "ここにローカルLLMからの応答が表示されます。";
            rawResponse = string.Empty;
            connectionStatus = "未接続";
            connectionStatusColor = "#747B8E";
            statusMessage = "llama-serverを起動し、モデル一覧を更新してください。";
            Load();
        }

        public ObservableCollection<string> Models { get; }

        public ICommand RefreshModelsCommand { get; }

        public ICommand SendTestCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public string ServerUrl
        {
            get => serverUrl;
            set { if (serverUrl != value) { serverUrl = value; OnPropertyChanged(); } }
        }

        public string SelectedModelId
        {
            get => selectedModelId;
            set { if (selectedModelId != value) { selectedModelId = value; OnPropertyChanged(); } }
        }

        public string TimeoutSecondsText
        {
            get => timeoutSecondsText;
            set { if (timeoutSecondsText != value) { timeoutSecondsText = value; OnPropertyChanged(); } }
        }

        public string TestPrompt
        {
            get => testPrompt;
            set { if (testPrompt != value) { testPrompt = value; OnPropertyChanged(); } }
        }

        public string TestResult
        {
            get => testResult;
            private set { if (testResult != value) { testResult = value; OnPropertyChanged(); } }
        }

        public string RawResponse
        {
            get => rawResponse;
            private set { if (rawResponse != value) { rawResponse = value; OnPropertyChanged(); } }
        }

        public string ConnectionStatus
        {
            get => connectionStatus;
            private set { if (connectionStatus != value) { connectionStatus = value; OnPropertyChanged(); } }
        }

        public string ConnectionStatusColor
        {
            get => connectionStatusColor;
            private set { if (connectionStatusColor != value) { connectionStatusColor = value; OnPropertyChanged(); } }
        }

        public string StatusMessage
        {
            get => statusMessage;
            private set { if (statusMessage != value) { statusMessage = value; OnPropertyChanged(); } }
        }

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (isBusy == value) return;
                isBusy = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void Load()
        {
            LocalAiSettings settings = settingsService.Load();
            ServerUrl = settings.ServerUrl;
            SelectedModelId = settings.ModelId;
            TimeoutSecondsText = settings.TimeoutSeconds.ToString();
        }

        private void Save()
        {
            try
            {
                settingsService.Save(CreateSettings());
                StatusMessage = $"接続設定を保存しました: {settingsService.SettingsPath}";
            }
            catch (Exception ex)
            {
                ConnectionStatus = "設定エラー";
                ConnectionStatusColor = "#E07A7A";
                StatusMessage = ex.Message;
            }
        }

        private async Task RefreshModelsAsync()
        {
            await RunOperationAsync("モデル一覧を取得中...", async cancellationToken =>
            {
                IReadOnlyList<string> ids = await llmClient.GetModelIdsAsync(ServerUrl, ParseTimeout(), cancellationToken);
                Models.Clear();
                foreach (string id in ids) Models.Add(id);
                if (Models.Count == 0) throw new InvalidOperationException("利用可能なモデルが見つかりませんでした。");
                if (string.IsNullOrWhiteSpace(SelectedModelId) || !Models.Contains(SelectedModelId))
                    SelectedModelId = Models[0];
                ConnectionStatus = $"接続済み（{Models.Count}モデル）";
                ConnectionStatusColor = "#72C9A5";
                StatusMessage = $"{Models.Count}件のモデルを検出しました。";
            });
        }

        private async Task SendTestAsync()
        {
            await RunOperationAsync("テスト応答を待っています...", async cancellationToken =>
            {
                LocalLlmTestResult result = await llmClient.SendTestAsync(
                    ServerUrl, SelectedModelId, TestPrompt, ParseTimeout(), cancellationToken);
                SelectedModelId = result.ModelId;
                TestResult = result.Content;
                RawResponse = result.RawJson;
                ConnectionStatus = "通信成功";
                ConnectionStatusColor = "#72C9A5";
                StatusMessage = $"ローカルLLMから応答を受信しました（Model: {result.ModelId}）。";
            });
        }

        private async Task RunOperationAsync(string progressMessage, Func<CancellationToken, Task> operation)
        {
            operationCancellation?.Dispose();
            operationCancellation = new CancellationTokenSource();
            IsBusy = true;
            ConnectionStatus = "通信中...";
            ConnectionStatusColor = "#E8B86D";
            StatusMessage = progressMessage;
            try
            {
                await operation(operationCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                ConnectionStatus = "キャンセル";
                ConnectionStatusColor = "#A8ADBB";
                StatusMessage = "通信をキャンセルしました。";
            }
            catch (Exception ex)
            {
                ConnectionStatus = "接続失敗";
                ConnectionStatusColor = "#E07A7A";
                StatusMessage = ex.Message;
            }
            finally
            {
                operationCancellation.Dispose();
                operationCancellation = null;
                IsBusy = false;
            }
        }

        private LocalAiSettings CreateSettings()
        {
            return new LocalAiSettings
            {
                ServerUrl = ServerUrl,
                ModelId = SelectedModelId,
                TimeoutSeconds = ParseTimeout()
            };
        }

        private int ParseTimeout()
        {
            if (!int.TryParse(TimeoutSecondsText, out int value) || value < 1 || value > 3600)
                throw new InvalidOperationException("Timeoutは1～3600秒で入力してください。");
            return value;
        }

        private void Cancel()
        {
            operationCancellation?.Cancel();
        }

        public void Dispose()
        {
            operationCancellation?.Cancel();
            operationCancellation?.Dispose();
            disposableClient?.Dispose();
        }
    }
}
