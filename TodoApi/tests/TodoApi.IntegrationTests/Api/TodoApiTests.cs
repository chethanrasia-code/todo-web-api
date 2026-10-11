using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using TodoApi.Api;
using TodoApi.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TodoApi.IntegrationTests.Api
{
    public class TodoApiTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private HttpClient _client = default!;

        public TodoApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            _client = _factory.CreateClient();

            // Obtain JWT token from the auth endpoint and set Authorization header for subsequent requests
            var login = new { Username = "admin", Password = "P@ssw0rd" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login);
            loginResponse.EnsureSuccessStatusCode();

            var payload = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("token", out var tokenProp))
            {
                var token = tokenProp.GetString();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        public Task DisposeAsync()
        {
            _client?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetTodos_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/todos");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateTodo_ReturnsCreated()
        {
            var todoDto = new TodoDto { Title = "Test Todo", Description = "Test Description", IsCompleted = false };
            var response = await _client.PostAsJsonAsync("/api/todos", todoDto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateTodo_ReturnsOk()
        {
            var todoDto = new TodoDto { Title = "Updated Todo", Description = "Updated Description", IsCompleted = true };
            var response = await _client.PutAsJsonAsync("/api/todos/1", todoDto);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DeleteTodo_ReturnsNoContent()
        {
            var response = await _client.DeleteAsync("/api/todos/1");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
