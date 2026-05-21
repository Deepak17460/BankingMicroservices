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
        try
        {
            var baseUrl = await _discoveryClient.DiscoverAsync(_settings.AccountServiceName, cancellationToken);
            var client = _httpClientFactory.CreateClient("InterService");
            
            var requestData = new CreateAccountRequest(customerId);
            Console.WriteLine($"Sending account creation request: CustomerId={customerId}, URL={baseUrl}/api/accounts");
            
            var response = await client.PostAsJsonAsync(
                $"{baseUrl}/api/accounts",
                requestData,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Account creation failed. Status: {response.StatusCode}, Content: {errorContent}");
                throw new HttpRequestException(
                    $"Account creation failed. Status: {response.StatusCode}, Content: {errorContent}");
            }
            
            Console.WriteLine($"Account creation succeeded for customer: {customerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during account creation: {ex}");
            throw new InvalidOperationException(
                $"Failed to create account for customer {customerId}. " +
                $"Ensure Account Management Service is running and registered with Service Discovery. " +
                $"Error: {ex.Message}", ex);
        }
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
