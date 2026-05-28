namespace BankingMicroservices.Shared.Handlers;

/// <summary>
/// HTTP message handler that forwards the X-Correlation-ID header in outgoing requests.
/// This ensures correlation ID is maintained across service-to-service calls.
/// </summary>
public class CorrelationIdHandler : DelegatingHandler
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Get correlation ID from current request context
            var correlationId = httpContext.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                               ?? httpContext.Items[CorrelationIdHeader]?.ToString();

            if (!string.IsNullOrEmpty(correlationId))
            {
                // Add correlation ID to outgoing request
                request.Headers.Add(CorrelationIdHeader, correlationId);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}