using System.Net;
using System.Text.Json;

namespace ECommerceAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            await HandleAsync(context, ex);
        }
    }

    private static async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (code, message) = ex switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)code;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = message, statusCode = (int)code }));
    }
}
