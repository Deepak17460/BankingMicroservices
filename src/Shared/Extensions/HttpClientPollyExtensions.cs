using Polly;
using Polly.Extensions.Http;

namespace BankingMicroservices.Shared.Extensions;

public static class HttpClientPollyExtensions
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(2));

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy() =>
        Policy.WrapAsync(GetRetryPolicy(), GetCircuitBreakerPolicy());

    public static IHttpClientBuilder AddBankingResiliencePolicies(this IHttpClientBuilder builder) =>
        builder
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
}
