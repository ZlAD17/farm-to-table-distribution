namespace FarmToTable.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns a consistent JSON error envelope
/// so the UI developer always receives structured error responses.
/// </summary>
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
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode  = ex switch
            {
                KeyNotFoundException    => StatusCodes.Status404NotFound,
                ArgumentException       => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _                       => StatusCodes.Status500InternalServerError
            };

            var body = new
            {
                error   = ex.GetType().Name,
                message = ex.Message
            };

            await ctx.Response.WriteAsJsonAsync(body);
        }
    }
}
