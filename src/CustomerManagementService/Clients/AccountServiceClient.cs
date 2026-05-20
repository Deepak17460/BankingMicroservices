using System.Net.Http.Json;
using BankingMicroservices.Shared.Clients;
using BankingMicroservices.Shared.DTOs;
using BankingMicroservices.CustomerManagementService.Configuration;

namespace BankingMicroservices.CustomerManagementService.Clients;

public class AccountServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceDiscoveryClient _discoveryClient;
    private readonly CustomerServiceSettings _settings;

    public AccountServiceClient(
        IHttpClientFactory httpClientFactory,
        ServiceDiscoveryClient discoveryClient,
        CustomerServiceSettings settings)
    {
        _httpClientFactory = httpClientFactory;
        _discoveryClient = discoveryClient;
        _settings = settings;
    }

    public async Task CreateAccountAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var baseUrl = await _discoveryClient.DiscoverAsync(_settings.AccountServiceName, cancellationToken);
        var client = _httpClientFactory.CreateClient("InterService");
        var response = await client.PostAsJsonAsync(
            $"{baseUrl}/api/accounts",
            new CreateAccountRequest(customerId),
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAccountByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var baseUrl = await _discoveryClient.DiscoverAsync(_settings.AccountServiceName, cancellationToken);
        var client = _httpClientFactory.CreateClient("InterService");
        var response = await client.DeleteAsync(
            $"{baseUrl}/api/accounts/customer/{customerId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
