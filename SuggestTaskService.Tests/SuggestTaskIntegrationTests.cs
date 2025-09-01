using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SuggestTaskService.Tests
{
    public class SuggestTaskIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SuggestTaskIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // in-memory server + HttpClient
            _client = factory.CreateClient();
        }

        // Case 1: 0% failure -> should always return 200 OK + ResetPasswordTask
        [Fact]
        public async Task PostSuggestTask_WithZeroFailureProbability_ReturnsOk()
        {
            var payload = new
            {
                utterance = "please reset password",
                userId = "u1",
                sessionId = "s1",
                timestamp = "2025-08-21T12:00:00Z"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/suggestTask")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("X-Failure-Probability", "0.0");

            var response = await _client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<ResponseDto>();
            Assert.NotNull(json);
            Assert.Equal("ResetPasswordTask", json!.task);
        }

        // Case 2: 50% failure -> can be either 200 OK or 503 ServiceUnavailable
        [Fact]
        public async Task PostSuggestTask_WithFiftyPercentFailureProbability_ReturnsOkOr503()
        {
            var payload = new
            {
                utterance = "please reset password",
                userId = "u2",
                sessionId = "s2",
                timestamp = "2025-08-21T12:00:00Z"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/suggestTask")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("X-Failure-Probability", "0.5");

            var response = await _client.SendAsync(request);

            // Accept either success or Service Unavailable due to random simulation
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable
            );

            // If success, assert the task
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadFromJsonAsync<ResponseDto>();
                Assert.NotNull(json);
                Assert.Equal("ResetPasswordTask", json!.task);
            }
        }

        // Case 3: 100% failure -> should always return 503 ServiceUnavailable
        [Fact]
        public async Task PostSuggestTask_WithAlwaysFailingDependency_Returns503()
        {
            var payload = new
            {
                utterance = "please reset password",
                userId = "u3",
                sessionId = "s3",
                timestamp = "2025-08-21T12:00:00Z"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/suggestTask")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("X-Failure-Probability", "1.0");

            var response = await _client.SendAsync(request);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        private sealed class ResponseDto
        {
            public string? task { get; set; }
            public string? timestamp { get; set; }
        }
    }
}
