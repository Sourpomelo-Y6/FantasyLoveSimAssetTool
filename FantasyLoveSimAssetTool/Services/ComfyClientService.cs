using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<ComfyOutputImage>> GetOutputImagesAsync(ComfySettings settings, string promptId)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                throw new InvalidOperationException("ComfyUI endpoint URL is empty.");
            }

            if (string.IsNullOrWhiteSpace(promptId))
            {
                throw new InvalidOperationException("ComfyUI prompt_id is empty.");
            }

            Uri endpointUri = BuildHistoryEndpointUri(settings.EndpointUrl, promptId);
            using HttpResponseMessage response = await httpClient.GetAsync(endpointUri).ConfigureAwait(false);
            string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"ComfyUI history returned {(int)response.StatusCode}: {TrimForMessage(responseJson)}");
            }

            return ParseOutputImages(responseJson, promptId);
        }

        public async Task<byte[]> GetImageAsync(ComfySettings settings, ComfyOutputImage image)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                throw new InvalidOperationException("ComfyUI endpoint URL is empty.");
            }

            if (string.IsNullOrWhiteSpace(image.FileName))
            {
                throw new InvalidOperationException("ComfyUI image filename is empty.");
            }

            Uri endpointUri = BuildViewEndpointUri(settings.EndpointUrl, image);
            using HttpResponseMessage response = await httpClient.GetAsync(endpointUri).ConfigureAwait(false);
            byte[] imageBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                string responseText = Encoding.UTF8.GetString(imageBytes);
                throw new InvalidOperationException($"ComfyUI view returned {(int)response.StatusCode}: {TrimForMessage(responseText)}");
            }

            return imageBytes;
        }

        private static Uri BuildPromptEndpointUri(string endpointUrl)
        {
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/') + "/prompt", UriKind.Absolute, out Uri endpointUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            return endpointUri;
        }

        private static Uri BuildHistoryEndpointUri(string endpointUrl, string promptId)
        {
            string escapedPromptId = Uri.EscapeDataString(promptId);
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/') + "/history/" + escapedPromptId, UriKind.Absolute, out Uri endpointUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            return endpointUri;
        }

        private static Uri BuildViewEndpointUri(string endpointUrl, ComfyOutputImage image)
        {
            string query = "filename=" + Uri.EscapeDataString(image.FileName ?? string.Empty) +
                "&subfolder=" + Uri.EscapeDataString(image.Subfolder ?? string.Empty) +
                "&type=" + Uri.EscapeDataString(image.Type ?? string.Empty);
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/') + "/view?" + query, UriKind.Absolute, out Uri endpointUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            return endpointUri;
        }

        private static IReadOnlyList<ComfyOutputImage> ParseOutputImages(string responseJson, string promptId)
        {
            List<ComfyOutputImage> images = new List<ComfyOutputImage>();
            using JsonDocument document = JsonDocument.Parse(responseJson);
            if (!document.RootElement.TryGetProperty(promptId, out JsonElement promptHistoryElement) ||
                !promptHistoryElement.TryGetProperty("outputs", out JsonElement outputsElement) ||
                outputsElement.ValueKind != JsonValueKind.Object)
            {
                return images;
            }

            foreach (JsonProperty outputProperty in outputsElement.EnumerateObject())
            {
                if (!outputProperty.Value.TryGetProperty("images", out JsonElement imagesElement) ||
                    imagesElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (JsonElement imageElement in imagesElement.EnumerateArray())
                {
                    images.Add(new ComfyOutputImage
                    {
                        FileName = GetStringProperty(imageElement, "filename"),
                        Subfolder = GetStringProperty(imageElement, "subfolder"),
                        Type = GetStringProperty(imageElement, "type")
                    });
                }
            }

            return images;
        }

        private static string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement) &&
                propertyElement.ValueKind == JsonValueKind.String)
            {
                return propertyElement.GetString() ?? string.Empty;
            }

            return string.Empty;
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
