using FantasyLoveSimAssetTool.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Services
{
    public class ComfyClientService
    {
        private readonly HttpClient httpClient;

        public ComfyClientService()
            : this(new HttpClient())
        {
        }

        public ComfyClientService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> QueuePromptAsync(ComfySettings settings, string workflowJson)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                throw new InvalidOperationException("ComfyUI endpoint URL is empty.");
            }

            if (string.IsNullOrWhiteSpace(workflowJson))
            {
                throw new InvalidOperationException("ComfyUI workflow JSON is empty.");
            }

            Uri endpointUri = BuildPromptEndpointUri(settings.EndpointUrl);
            using JsonDocument workflowDocument = JsonDocument.Parse(workflowJson);
            ComfyPromptRequest request = new ComfyPromptRequest
            {
                Prompt = workflowDocument.RootElement.Clone(),
                ClientId = Guid.NewGuid().ToString("N")
            };

            string requestJson = JsonSerializer.Serialize(request);
            using StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await httpClient.PostAsync(endpointUri, content).ConfigureAwait(false);
            string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"ComfyUI returned {(int)response.StatusCode}: {TrimForMessage(responseJson)}");
            }

            using JsonDocument responseDocument = JsonDocument.Parse(responseJson);
            if (!responseDocument.RootElement.TryGetProperty("prompt_id", out JsonElement promptIdElement))
            {
                throw new InvalidOperationException($"ComfyUI response did not include prompt_id: {TrimForMessage(responseJson)}");
            }

            string promptId = promptIdElement.GetString();
            if (string.IsNullOrWhiteSpace(promptId))
            {
                throw new InvalidOperationException($"ComfyUI response included an empty prompt_id: {TrimForMessage(responseJson)}");
            }

            return promptId;
        }

        private static Uri BuildPromptEndpointUri(string endpointUrl)
        {
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/') + "/prompt", UriKind.Absolute, out Uri endpointUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            return endpointUri;
        }

        private static string TrimForMessage(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= 500 ? value : value.Substring(0, 500) + "...";
        }

        private class ComfyPromptRequest
        {
            [JsonPropertyName("prompt")]
            public JsonElement Prompt { get; set; }

            [JsonPropertyName("client_id")]
            public string ClientId { get; set; }
        }
    }
}
