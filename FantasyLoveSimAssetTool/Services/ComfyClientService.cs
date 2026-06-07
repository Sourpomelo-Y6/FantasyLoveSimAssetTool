using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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
            ComfyPromptQueueResult result = await QueuePromptWithClientAsync(settings, workflowJson).ConfigureAwait(false);
            return result.PromptId;
        }

        public Task<ComfyPromptQueueResult> QueuePromptWithClientAsync(ComfySettings settings, string workflowJson)
        {
            return QueuePromptWithClientAsync(settings, workflowJson, Guid.NewGuid().ToString("N"));
        }

        public async Task<ComfyPromptQueueResult> QueuePromptWithClientAsync(ComfySettings settings, string workflowJson, string clientId)
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
                ClientId = string.IsNullOrWhiteSpace(clientId) ? Guid.NewGuid().ToString("N") : clientId
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

            return new ComfyPromptQueueResult
            {
                PromptId = promptId,
                ClientId = request.ClientId
            };
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

        public async Task<ComfyQueueStatus> GetQueueStatusAsync(ComfySettings settings, string promptId)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                throw new InvalidOperationException("ComfyUI endpoint URL is empty.");
            }

            Uri endpointUri = BuildQueueEndpointUri(settings.EndpointUrl);
            using HttpResponseMessage response = await httpClient.GetAsync(endpointUri).ConfigureAwait(false);
            string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"ComfyUI queue returned {(int)response.StatusCode}: {TrimForMessage(responseJson)}");
            }

            return ParseQueueStatus(responseJson, promptId);
        }

        public async Task InterruptAsync(ComfySettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                throw new InvalidOperationException("ComfyUI endpoint URL is empty.");
            }

            Uri endpointUri = BuildInterruptEndpointUri(settings.EndpointUrl);
            using StringContent content = new StringContent("{}", Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await httpClient.PostAsync(endpointUri, content).ConfigureAwait(false);
            string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"ComfyUI interrupt returned {(int)response.StatusCode}: {TrimForMessage(responseText)}");
            }
        }

        public async Task WatchPromptProgressAsync(
            ComfySettings settings,
            string promptId,
            string clientId,
            Action<ComfyProgressUpdate> progressReceived,
            CancellationToken cancellationToken)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                throw new InvalidOperationException("ComfyUI endpoint URL is empty.");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException("ComfyUI client_id is empty.");
            }

            Uri endpointUri = BuildWebSocketEndpointUri(settings.EndpointUrl, clientId);
            using ClientWebSocket webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(endpointUri, cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[8192];
            while (webSocket.State == WebSocketState.Open)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using MemoryStream messageStream = new MemoryStream();
                WebSocketReceiveResult receiveResult;
                do
                {
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationToken).ConfigureAwait(false);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    messageStream.Write(buffer, 0, receiveResult.Count);
                }
                while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType != WebSocketMessageType.Text)
                {
                    continue;
                }

                string messageJson = Encoding.UTF8.GetString(messageStream.ToArray());
                ComfyProgressUpdate update = ParseProgressUpdate(messageJson, promptId);
                if (update == null)
                {
                    continue;
                }

                progressReceived?.Invoke(update);
                if (update.IsCompleted)
                {
                    return;
                }
            }
        }

        private static Uri BuildPromptEndpointUri(string endpointUrl)
        {
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/') + "/prompt", UriKind.Absolute, out Uri endpointUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            return endpointUri;
        }

        private static Uri BuildInterruptEndpointUri(string endpointUrl)
        {
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/') + "/interrupt", UriKind.Absolute, out Uri endpointUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            return endpointUri;
        }

        private static Uri BuildQueueEndpointUri(string endpointUrl)
        {
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/') + "/queue", UriKind.Absolute, out Uri endpointUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            return endpointUri;
        }

        private static Uri BuildWebSocketEndpointUri(string endpointUrl, string clientId)
        {
            if (!Uri.TryCreate(endpointUrl.TrimEnd('/'), UriKind.Absolute, out Uri baseUri))
            {
                throw new InvalidOperationException($"ComfyUI endpoint URL is invalid: {endpointUrl}");
            }

            string scheme = baseUri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws";
            UriBuilder builder = new UriBuilder(baseUri)
            {
                Scheme = scheme,
                Path = baseUri.AbsolutePath.TrimEnd('/') + "/ws",
                Query = "clientId=" + Uri.EscapeDataString(clientId)
            };

            return builder.Uri;
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

        private static ComfyQueueStatus ParseQueueStatus(string responseJson, string promptId)
        {
            ComfyQueueStatus status = new ComfyQueueStatus();
            using JsonDocument document = JsonDocument.Parse(responseJson);
            JsonElement rootElement = document.RootElement;

            if (rootElement.TryGetProperty("queue_running", out JsonElement runningElement) &&
                runningElement.ValueKind == JsonValueKind.Array)
            {
                status.RunningCount = runningElement.GetArrayLength();
                status.IsTargetRunning = ContainsPromptId(runningElement, promptId);
            }

            if (rootElement.TryGetProperty("queue_pending", out JsonElement pendingElement) &&
                pendingElement.ValueKind == JsonValueKind.Array)
            {
                status.PendingCount = pendingElement.GetArrayLength();
                status.TargetPendingIndex = FindPromptIdIndex(pendingElement, promptId);
            }

            return status;
        }

        private static ComfyProgressUpdate ParseProgressUpdate(string messageJson, string targetPromptId)
        {
            using JsonDocument document = JsonDocument.Parse(messageJson);
            JsonElement rootElement = document.RootElement;
            if (!rootElement.TryGetProperty("type", out JsonElement typeElement) ||
                typeElement.ValueKind != JsonValueKind.String ||
                !rootElement.TryGetProperty("data", out JsonElement dataElement) ||
                dataElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            string eventType = typeElement.GetString() ?? string.Empty;
            string promptId = GetStringProperty(dataElement, "prompt_id");
            if (!string.IsNullOrWhiteSpace(promptId) &&
                !string.IsNullOrWhiteSpace(targetPromptId) &&
                promptId != targetPromptId)
            {
                return null;
            }

            ComfyProgressUpdate update = new ComfyProgressUpdate
            {
                EventType = eventType,
                PromptId = promptId
            };

            if (dataElement.TryGetProperty("node", out JsonElement nodeElement))
            {
                if (nodeElement.ValueKind == JsonValueKind.String)
                {
                    update.NodeId = nodeElement.GetString() ?? string.Empty;
                }
                else if (nodeElement.ValueKind == JsonValueKind.Null)
                {
                    update.IsCompleted = eventType == "executing";
                }
            }

            if (dataElement.TryGetProperty("value", out JsonElement valueElement) &&
                valueElement.TryGetInt32(out int value))
            {
                update.Value = value;
            }

            if (dataElement.TryGetProperty("max", out JsonElement maxElement) &&
                maxElement.TryGetInt32(out int max))
            {
                update.Max = max;
            }

            if (eventType == "progress" || eventType == "executing")
            {
                return update;
            }

            return null;
        }

        private static bool ContainsPromptId(JsonElement queueElement, string promptId)
        {
            return FindPromptIdIndex(queueElement, promptId) > 0;
        }

        private static int FindPromptIdIndex(JsonElement queueElement, string promptId)
        {
            if (string.IsNullOrWhiteSpace(promptId) || queueElement.ValueKind != JsonValueKind.Array)
            {
                return 0;
            }

            int index = 1;
            foreach (JsonElement itemElement in queueElement.EnumerateArray())
            {
                if (QueueItemContainsPromptId(itemElement, promptId))
                {
                    return index;
                }

                index++;
            }

            return 0;
        }

        private static bool QueueItemContainsPromptId(JsonElement itemElement, string promptId)
        {
            switch (itemElement.ValueKind)
            {
                case JsonValueKind.String:
                    return itemElement.GetString() == promptId;
                case JsonValueKind.Array:
                    foreach (JsonElement childElement in itemElement.EnumerateArray())
                    {
                        if (QueueItemContainsPromptId(childElement, promptId))
                        {
                            return true;
                        }
                    }
                    break;
                case JsonValueKind.Object:
                    foreach (JsonProperty property in itemElement.EnumerateObject())
                    {
                        if (QueueItemContainsPromptId(property.Value, promptId))
                        {
                            return true;
                        }
                    }
                    break;
            }

            return false;
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
