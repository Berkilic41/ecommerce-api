using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerceAPI.Tests.Integration;

/// <summary>
/// Test factory that spins up a real in-process HTTP server for integration tests.
/// Overrides the connection string to point at a test database and disables rate limiting.
/// </summary>
public class ECommerceApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Override connection string for integration tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\mssqllocaldb;Database=ECommerceDb_Test;Integrated Security=true;TrustServerCertificate=true;",
                // Use fast symmetric key for test JWTs
                ["Jwt:Key"]      = "integration-test-secret-key-min-32-chars!!",
                ["Jwt:Issuer"]   = "ECommerceAPI",
                ["Jwt:Audience"] = "ECommerceAPI"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Disable rate limiting in tests to avoid 429s
            services.Configure<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>(options =>
            {
                options.RejectionStatusCode = 200; // never reject in tests
            });
        });
    }
}
