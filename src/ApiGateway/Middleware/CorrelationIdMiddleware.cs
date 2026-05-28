using Serilog.Context;

namespace BankingMicroservices.ApiGateway.Middleware;

/// <summary>
/// Generates or forwards X-Correlation-ID at the gateway boundary.
/// Downstream services receive the same ID so all logs can be correlated end-to-end.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Accept from upstream client or generate a fresh one
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        // Ensure downstream services receive the header
        context.Request.Headers[CorrelationIdHeader] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        context.Items[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("[Gateway] {Method} {Path} → {CorrelationId}",
                context.Request.Method, context.Request.Path, correlationId);
            
            await _next(context);
            
            _logger.LogInformation("[Gateway] {StatusCode} {Path} ← {CorrelationId}",
                context.Response.StatusCode, context.Request.Path, correlationId);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}