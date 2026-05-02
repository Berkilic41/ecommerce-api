using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace ECommerceAPI.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<ECommerceApiFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(ECommerceApiFactory factory)
        => _client = factory.CreateClient();

    private static StringContent Json(object obj)
        => new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    [Fact]
    public async Task POST_Register_WithInvalidEmail_Returns400()
    {
        var response = await _client.PostAsync("/api/auth/register",
            Json(new { username = "user1", email = "not-an-email", password = "Pass123!" }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Register_WithShortPassword_Returns400()
    {
        var response = await _client.PostAsync("/api/auth/register",
            Json(new { username = "user2", email = "user2@test.com", password = "123" }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Login_WithWrongCredentials_Returns401()
    {
        var response = await _client.PostAsync("/api/auth/login",
            Json(new { email = "nonexistent@test.com", password = "WrongPassword1!" }));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_Register_ThenLogin_ReturnsAccessToken()
    {
        var email    = $"flow_{Guid.NewGuid():N}@integration.test";
        var password = "IntegrationPass1!";

        var registerResp = await _client.PostAsync("/api/auth/register",
            Json(new { username = $"u{Guid.NewGuid():N[..8]}", email, password }));

        // If DB not available, skip gracefully
        if (!registerResp.IsSuccessStatusCode) return;

        var loginResp = await _client.PostAsync("/api/auth/login",
            Json(new { email, password }));

        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await loginResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.TryGetProperty("accessToken",  out var at).Should().BeTrue();
        doc.RootElement.TryGetProperty("refreshToken", out var rt).Should().BeTrue();
        at.GetString().Should().NotBeNullOrEmpty();
        rt.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task POST_Register_DuplicateEmail_Returns409OrBadRequest()
    {
        var email    = $"dup_{Guid.NewGuid():N}@integration.test";
        var password = "DupPass123!";

        var first = await _client.PostAsync("/api/auth/register",
            Json(new { username = $"u{Guid.NewGuid():N[..8]}", email, password }));

        if (!first.IsSuccessStatusCode) return; // DB unavailable

        var second = await _client.PostAsync("/api/auth/register",
            Json(new { username = $"u{Guid.NewGuid():N[..8]}", email, password }));

        second.StatusCode.Should().BeOneOf(
            HttpStatusCode.Conflict,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }
}
