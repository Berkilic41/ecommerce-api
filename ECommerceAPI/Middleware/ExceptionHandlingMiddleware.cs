using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ECommerceAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
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
        var (statusCode, title) = ex switch
        {
            KeyNotFoundException        => (HttpStatusCode.NotFound,            "Not Found"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,        "Unauthorized"),
            InvalidOperationException   => (HttpStatusCode.BadRequest,          "Bad Request"),
            ArgumentException           => (HttpStatusCode.BadRequest,          "Bad Request"),
            _                           => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        var detail = statusCode == HttpStatusCode.InternalServerError
            ? "An unexpected error occurred. Please try again later."
            : ex.Message;

        var problem = new ProblemDetails
        {
            Type     = $"https://httpstatuses.com/{(int)statusCode}",
            Title    = title,
            Status   = (int)statusCode,
            Detail   = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}
