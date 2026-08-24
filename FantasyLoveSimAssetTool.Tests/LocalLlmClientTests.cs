using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class LocalLlmClientTests
    {
        [TestMethod]
        public async Task GetModelIdsAsync_ReturnsDistinctModelIds()
        {
            using var httpClient = new HttpClient(new StubHandler(request =>
            {
                Assert.AreEqual("http://localhost:8080/v1/models", request.RequestUri.ToString());
                return Json(HttpStatusCode.OK, "{\"data\":[{\"id\":\"model-a\"},{\"id\":\"model-a\"},{\"id\":\"model-b\"}]}");
            }));
            using var client = new LocalLlmClient(httpClient);

            var models = await client.GetModelIdsAsync("http://localhost:8080/", 10);

            CollectionAssert.AreEqual(new[] { "model-a", "model-b" }, new System.Collections.Generic.List<string>(models));
        }

        [TestMethod]
        public async Task SendTestAsync_ParsesJapaneseContent()
        {
            string requestBody = null;
            using var httpClient = new HttpClient(new AsyncStubHandler(async (request, _) =>
            {
                Assert.AreEqual(HttpMethod.Post, request.Method);
                Assert.AreEqual("http://localhost:8080/v1/chat/completions", request.RequestUri.ToString());
                requestBody = await request.Content.ReadAsStringAsync();
                return Json(HttpStatusCode.OK, "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"通信成功です。\"}}]}");
            }));
            using var client = new LocalLlmClient(httpClient);

            LocalLlmTestResult result = await client.SendTestAsync(
                "http://localhost:8080", "model-a", "テスト", 10);

            Assert.AreEqual("model-a", result.ModelId);
            Assert.AreEqual("通信成功です。", result.Content);
            Assert.IsFalse(requestBody.Contains("reasoning_content"));
            Assert.IsFalse(requestBody.Contains(":null"));
        }

        [TestMethod]
        public async Task GetModelIdsAsync_WhenServerReturnsError_ThrowsReadableMessage()
        {
            using var httpClient = new HttpClient(new StubHandler(_ => Json(HttpStatusCode.NotFound, "missing")));
            using var client = new LocalLlmClient(httpClient);

            InvalidOperationException error = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => client.GetModelIdsAsync("http://localhost:8080", 10));

            StringAssert.Contains(error.Message, "HTTP 404");
        }

        [TestMethod]
        public async Task GetModelIdsAsync_WhenUrlIsInvalid_ThrowsReadableMessage()
        {
            using var client = new LocalLlmClient(new HttpClient(new StubHandler(_ => Json(HttpStatusCode.OK, "{}"))));

            InvalidOperationException error = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => client.GetModelIdsAsync("invalid", 10));

            StringAssert.Contains(error.Message, "httpまたはhttps");
        }

        [TestMethod]
        public async Task GetModelIdsAsync_WhenRequestTimesOut_ThrowsTimeoutMessage()
        {
            using var httpClient = new HttpClient(new AsyncStubHandler(async (_, cancellationToken) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                return Json(HttpStatusCode.OK, "{\"data\":[]}");
            }));
            using var client = new LocalLlmClient(httpClient);

            InvalidOperationException error = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => client.GetModelIdsAsync("http://localhost:8080", 1));

            StringAssert.Contains(error.Message, "タイムアウト");
        }

        [TestMethod]
        public async Task GenerateAsync_SendsSpecifiedSystemPromptAndGenerationSettings()
        {
            string requestBody = null;
            using var httpClient = new HttpClient(new AsyncStubHandler(async (request, _) =>
            {
                requestBody = await request.Content.ReadAsStringAsync();
                return Json(HttpStatusCode.OK,
                    "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"{\\\"candidates\\\":[]}\"}}]}");
            }));
            using var client = new LocalLlmClient(httpClient);

            await client.GenerateAsync("http://localhost:8080", "model-a",
                "共通指示", "生成指示", 0.35, 777, 10);

            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            JsonElement root = requestJson.RootElement;
            Assert.AreEqual("共通指示", root.GetProperty("messages")[0].GetProperty("content").GetString());
            Assert.AreEqual("生成指示", root.GetProperty("messages")[1].GetProperty("content").GetString());
            Assert.AreEqual(0.35, root.GetProperty("temperature").GetDouble());
            Assert.AreEqual(777, root.GetProperty("max_tokens").GetInt32());
            Assert.IsFalse(requestBody.Contains(":null"));
        }

        private static HttpResponseMessage Json(HttpStatusCode statusCode, string body)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            {
                this.responseFactory = responseFactory;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(responseFactory(request));
            }
        }

        private sealed class AsyncStubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory;

            public AsyncStubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
            {
                this.responseFactory = responseFactory;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return responseFactory(request, cancellationToken);
            }
        }
    }
}
