using System.Net.Http.Json;
using BankingMicroservices.AccountManagementService.Configuration;
using BankingMicroservices.Shared.Clients;
using BankingMicroservices.Shared.DTOs;
using BankingMicroservices.Shared.Exceptions;

namespace BankingMicroservices.AccountManagementService.Clients;

public class CustomerServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceDiscoveryClient _discoveryClient;
    private readonly AccountServiceSettings _settings;

    public CustomerServiceClient(
        IHttpClientFactory httpClientFactory,
        ServiceDiscoveryClient discoveryClient,
        AccountServiceSettings settings)
    {
        _httpClientFactory = httpClientFactory;
        _discoveryClient = discoveryClient;
        _settings = settings;
    }

    public async Task<CustomerDto> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var baseUrl = await _discoveryClient.DiscoverAsync(_settings.CustomerServiceName, cancellationToken);
        var client = _httpClientFactory.CreateClient("InterService");
        var response = await client.GetAsync($"{baseUrl}/api/customers/{customerId}", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new CustomerNotFoundException(customerId);
        }

        response.EnsureSuccessStatusCode();
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>(cancellationToken);
        if (apiResponse?.Data is null)
        {
            throw new CustomerNotFoundException(customerId);
        }

        return apiResponse.Data;
    }

    public async Task EnsureCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await GetCustomerAsync(customerId, cancellationToken);
    }
}
