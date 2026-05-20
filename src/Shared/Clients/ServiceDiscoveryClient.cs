using System.Net.Http.Json;
using BankingMicroservices.Shared.Exceptions;
using BankingMicroservices.Shared.Models;

namespace BankingMicroservices.Shared.Clients;

public class ServiceDiscoveryClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _discoveryBaseUrl;

    public ServiceDiscoveryClient(IHttpClientFactory httpClientFactory, string discoveryBaseUrl)
    {
        _httpClientFactory = httpClientFactory;
        _discoveryBaseUrl = discoveryBaseUrl.TrimEnd('/');
    }

    public async Task<string> DiscoverAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ServiceDiscovery");
        var response = await client.GetAsync(
            $"{_discoveryBaseUrl}/discover/{serviceName}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceNotFoundException(serviceName);
        }

        var result = await response.Content.ReadFromJsonAsync<DiscoverResponse>(cancellationToken);
        if (result is null || string.IsNullOrWhiteSpace(result.Url))
        {
            throw new ServiceNotFoundException(serviceName);
        }

        return result.Url.TrimEnd('/');
    }

    public async Task RegisterAsync(ServiceRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ServiceDiscovery");
        var response = await client.PostAsJsonAsync($"{_discoveryBaseUrl}/register", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
