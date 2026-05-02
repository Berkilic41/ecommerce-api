using ECommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly DbConnectionFactory     _dbFactory;
    private readonly ILogger<HealthController> _logger;

    public HealthController(DbConnectionFactory dbFactory, ILogger<HealthController> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    /// <summary>Health check — verifies API and database connectivity.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Check()
    {
        var timestamp  = DateTime.UtcNow;
        var dbStatus   = "ok";
        var overallStatus = "healthy";

        try
        {
            using var connection = _dbFactory.CreateConnection();
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: database connection failed");
            dbStatus      = "error";
            overallStatus = "degraded";
        }

        var response   = new HealthResponse { Status = overallStatus, Database = dbStatus, Timestamp = timestamp };
        var statusCode = overallStatus == "healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
        return StatusCode(statusCode, response);
    }
}

public record HealthResponse
{
    public string   Status    { get; init; } = string.Empty;
    public string   Database  { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
