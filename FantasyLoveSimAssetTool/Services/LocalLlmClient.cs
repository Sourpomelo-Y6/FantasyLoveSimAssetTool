using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Services
{
    public interface ILocalLlmClient
    {
        Task<IReadOnlyList<string>> GetModelIdsAsync(string serverUrl, int timeoutSeconds, CancellationToken cancellationToken = default);

        Task<LocalLlmTestResult> SendTestAsync(string serverUrl, string modelId, string prompt,
            int timeoutSeconds, CancellationToken cancellationToken = default);
    }

    public sealed class LocalLlmTestResult
    {
        public string ModelId { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string RawJson { get; set; } = string.Empty;
    }

    public sealed class LocalLlmClient : ILocalLlmClient, IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly bool ownsHttpClient;

        public LocalLlmClient()
            : this(new HttpClient(), true)
        {
        }

        public LocalLlmClient(HttpClient httpClient)
            : this(httpClient, false)
        {
        }

        private LocalLlmClient(HttpClient httpClient, bool ownsHttpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.ownsHttpClient = ownsHttpClient;
        }

        public async Task<IReadOnlyList<string>> GetModelIdsAsync(string serverUrl, int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            string url = ValidateAndNormalizeUrl(serverUrl);
            using CancellationTokenSource timeout = CreateTimeout(timeoutSeconds, cancellationToken);
            HttpResponseMessage receivedResponse;
            try
            {
                receivedResponse = await httpClient.GetAsync($"{url}/v1/models", timeout.Token);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException($"モデル一覧の取得が{timeoutSeconds}秒でタイムアウトしました。", ex);
            }
            using HttpResponseMessage response = receivedResponse;
            string rawJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"llama-serverがHTTP {(int)response.StatusCode}を返しました: {Shorten(rawJson)}");

            ModelsResponse models;
            try
            {
                models = JsonSerializer.Deserialize<ModelsResponse>(rawJson);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("モデル一覧の応答JSONを解析できません。", ex);
            }

            return (models?.Data ?? new List<ModelInfo>())
                .Select(model => model.Id?.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        public async Task<LocalLlmTestResult> SendTestAsync(string serverUrl, string modelId, string prompt,
            int timeoutSeconds, CancellationToken cancellationToken = default)
        {
            string url = ValidateAndNormalizeUrl(serverUrl);
            if (string.IsNullOrWhiteSpace(prompt)) throw new InvalidOperationException("テスト送信内容を入力してください。");

            string resolvedModelId = (modelId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(resolvedModelId))
            {
                IReadOnlyList<string> ids = await GetModelIdsAsync(url, timeoutSeconds, cancellationToken);
                resolvedModelId = ids.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(resolvedModelId))
                    throw new InvalidOperationException("llama-serverに利用可能なモデルがありません。");
            }

            ChatCompletionRequest body = new ChatCompletionRequest
            {
                Model = resolvedModelId,
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "system", Content = "あなたは通信テストに簡潔な日本語で応答するアシスタントです。" },
                    new ChatMessage { Role = "user", Content = prompt }
                },
                Temperature = 0.7,
                MaxTokens = 1024,
                Stream = false
            };

            string requestJson = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{url}/v1/chat/completions")
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            using CancellationTokenSource timeout = CreateTimeout(timeoutSeconds, cancellationToken);
            HttpResponseMessage receivedResponse;
            try
            {
                receivedResponse = await httpClient.SendAsync(request, timeout.Token);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException($"テスト送信が{timeoutSeconds}秒でタイムアウトしました。", ex);
            }
            using HttpResponseMessage response = receivedResponse;
            string rawJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"llama-serverがHTTP {(int)response.StatusCode}を返しました: {Shorten(rawJson)}");

            ChatCompletionResponse parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(rawJson);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("llama-serverの応答JSONを解析できません。", ex);
            }

            ChatMessage responseMessage = parsed?.Choices?.FirstOrDefault()?.Message;
            string content = responseMessage?.Content;
            if (string.IsNullOrWhiteSpace(content)) content = responseMessage?.ReasoningContent;
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("llama-serverの応答にcontentがありません。");

            return new LocalLlmTestResult
            {
                ModelId = resolvedModelId,
                Content = content.Trim(),
                RawJson = rawJson
            };
        }

        private static string ValidateAndNormalizeUrl(string serverUrl)
        {
            if (!Uri.TryCreate(serverUrl?.Trim(), UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("Server URLにはhttpまたはhttpsのURLを入力してください。");
            return uri.ToString().TrimEnd('/');
        }

        private static CancellationTokenSource CreateTimeout(int timeoutSeconds, CancellationToken cancellationToken)
        {
            if (timeoutSeconds < 1) throw new InvalidOperationException("Timeoutは1秒以上にしてください。");
            CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            source.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            return source;
        }

        private static string Shorten(string value)
        {
            string text = (value ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ').Trim();
            return text.Length <= 240 ? text : text.Substring(0, 240) + "...";
        }

        public void Dispose()
        {
            if (ownsHttpClient) httpClient.Dispose();
        }

        private sealed class ModelsResponse
        {
            [JsonPropertyName("data")]
            public List<ModelInfo> Data { get; set; }
        }

        private sealed class ModelInfo
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
        }

        private sealed class ChatCompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("messages")]
            public List<ChatMessage> Messages { get; set; }

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }
        }

        private sealed class ChatCompletionResponse
        {
            [JsonPropertyName("choices")]
            public List<ChatChoice> Choices { get; set; }
        }

        private sealed class ChatChoice
        {
            [JsonPropertyName("message")]
            public ChatMessage Message { get; set; }
        }

        private sealed class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("reasoning_content")]
            public string ReasoningContent { get; set; }
        }
    }
}
