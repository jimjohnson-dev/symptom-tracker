using System.Text.Json;

namespace SymptomTracker.Api.Middleware;

// returns structured JSON error response instead of default ASP.NET error page across the app, constrains exception logic to a single place
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
            await WriteErrorResponseAsync(ctx, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";
        
        var body = JsonSerializer.Serialize(new
        {
            error = "An unexpected error occurred.",
            details = ex.Message
        });
        
        await ctx.Response.WriteAsync(body);
    }
}