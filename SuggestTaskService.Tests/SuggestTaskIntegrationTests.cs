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

        // test for valid ResetPasswordTask utterance
        [Fact]
        public async Task PostSuggestTask_ReturnsResetPasswordTask()
        {
            var payload = new
            {
                utterance = "please reset password",
                userId = "u1",
                sessionId = "s1",
                timestamp = "2025-08-21T12:00:00Z"
            };

            var response = await _client.PostAsJsonAsync("/suggestTask", payload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<ResponseDto>();
            Assert.NotNull(json);
            Assert.Equal("ResetPasswordTask", json!.task);
        }

        // test for missing utterance
        [Fact]
        public async Task PostSuggestTask_MissingUtterance_ReturnsBadRequest()
        {
            var payload = new
            {
                userId = "u2",
                sessionId = "s2",
                timestamp = "2025-08-21T12:00:00Z"
            };

            var response = await _client.PostAsJsonAsync("/suggestTask", payload);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // test for no matching utterance
        [Fact]
        public async Task PostSuggestTask_NoMatchingUtterance_ReturnsNoTaskFound()
        {
            var payload = new
            {
                utterance = "hello world",
                userId = "u3",
                sessionId = "s3",
                timestamp = "2025-08-21T12:00:00Z"
            };

            var response = await _client.PostAsJsonAsync("/suggestTask", payload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<ResponseDto>();
            Assert.NotNull(json);
            Assert.Equal("NoTaskFound", json!.task);
        }



        private sealed class ResponseDto
        {
            public string? task { get; set; }
            public string? timestamp { get; set; }
        }
    }
}
