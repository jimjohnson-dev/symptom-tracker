using System.Text.Json;

namespace SymptomTracker.Api.Middleware;

/// <summary>
/// Central exception handling that returns structured JSON error response instead of default ASP.NET error page.
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        var body = JsonSerializer.Serialize(new
        {
            error = "An unexpected error occurred.",
            details = ex.Message
        });
        
        await context.Response.WriteAsync(body);
    }
}