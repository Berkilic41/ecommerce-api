using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace ECommerceAPI.Tests.Integration;

public class HealthIntegrationTests : IClassFixture<ECommerceApiFactory>
{
    private readonly HttpClient _client;

    public HealthIntegrationTests(ECommerceApiFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GET_Health_Returns200OrServiceUnavailable()
    {
        var response = await _client.GetAsync("/api/v1/health");

        // Either healthy (200) or degraded (503) — both are valid responses
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GET_Health_ReturnsJsonWithStatusField()
    {
        var response = await _client.GetAsync("/api/v1/health");
        var body     = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var statusProp).Should().BeTrue();
        statusProp.GetString().Should().BeOneOf("healthy", "degraded");
    }

    [Fact]
    public async Task GET_Health_ReturnsJsonWithTimestamp()
    {
        var response = await _client.GetAsync("/api/v1/health");
        var body     = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GET_Health_ReturnsJsonWithDatabaseField()
    {
        var response = await _client.GetAsync("/api/v1/health");
        var body     = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("database", out var dbProp).Should().BeTrue();
        dbProp.GetString().Should().BeOneOf("ok", "error");
    }
}
