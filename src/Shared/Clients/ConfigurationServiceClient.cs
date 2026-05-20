using System.Net.Http.Json;
using BankingMicroservices.Shared.Models;

namespace BankingMicroservices.Shared.Clients;

public class ConfigurationServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _configurationBaseUrl;

    public ConfigurationServiceClient(IHttpClientFactory httpClientFactory, string configurationBaseUrl)
    {
        _httpClientFactory = httpClientFactory;
        _configurationBaseUrl = configurationBaseUrl.TrimEnd('/');
    }

    public async Task<ServiceConfiguration> GetConfigurationAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ConfigurationService");
        var response = await client.GetAsync(
            $"{_configurationBaseUrl}/config/{serviceName}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var config = await response.Content.ReadFromJsonAsync<ServiceConfiguration>(cancellationToken);
        return config ?? new ServiceConfiguration(new Dictionary<string, string>());
    }
}
