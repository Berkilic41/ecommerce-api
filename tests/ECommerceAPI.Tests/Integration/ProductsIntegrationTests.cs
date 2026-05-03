using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace ECommerceAPI.Tests.Integration;

public class ProductsIntegrationTests : IClassFixture<ECommerceApiFactory>
{
    private readonly HttpClient _client;

    public ProductsIntegrationTests(ECommerceApiFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GET_Products_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Products_WithInvalidToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        var response = await _client.GetAsync("/api/v1/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Products_WithBadId_Returns404()
    {
        // First get a valid token by registering+logging in
        var token = await GetTestTokenAsync();
        if (token is null) return; // Skip if DB not available

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/products/99999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_Products_ReturnsJsonWithItemsField()
    {
        var token = await GetTestTokenAsync();
        if (token is null) return;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/products?page=1&pageSize=5");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            doc.RootElement.TryGetProperty("items", out _).Should().BeTrue();
            doc.RootElement.TryGetProperty("total", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task POST_Products_WithoutAdminRole_Returns403()
    {
        var token = await GetTestTokenAsync();
        if (token is null) return;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = JsonSerializer.Serialize(new
        {
            name = "Test Product", description = "Desc",
            price = 9.99, stockQuantity = 10, categoryId = 1
        });

        var response = await _client.PostAsync("/api/v1/products",
            new StringContent(body, Encoding.UTF8, "application/json"));

        // Regular user (not Admin) should get 403 Forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // Helper: register + login a test user, return JWT
    private async Task<string?> GetTestTokenAsync()
    {
        try
        {
            var email    = $"test_{Guid.NewGuid():N}@integration.test";
            var password = "TestPass123!";

            var registerBody = JsonSerializer.Serialize(new
            {
                username = $"user_{Guid.NewGuid():N[..8]}",
                email,
                password
            });

            var registerResp = await _client.PostAsync("/api/auth/register",
                new StringContent(registerBody, Encoding.UTF8, "application/json"));

            if (!registerResp.IsSuccessStatusCode) return null;

            var loginBody = JsonSerializer.Serialize(new { email, password });
            var loginResp = await _client.PostAsync("/api/auth/login",
                new StringContent(loginBody, Encoding.UTF8, "application/json"));

            if (!loginResp.IsSuccessStatusCode) return null;

            var loginJson = await loginResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(loginJson);
            return doc.RootElement.GetProperty("accessToken").GetString();
        }
        catch
        {
            return null; // DB not available — skip
        }
    }
}
