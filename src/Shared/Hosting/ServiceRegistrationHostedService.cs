using BankingMicroservices.Shared.Clients;
using BankingMicroservices.Shared.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BankingMicroservices.Shared.Hosting;

public class ServiceRegistrationHostedService : BackgroundService
{
    private readonly ServiceDiscoveryClient _discoveryClient;
    private readonly ILogger<ServiceRegistrationHostedService> _logger;
    private readonly string _serviceName;
    private readonly string _serviceUrl;

    public ServiceRegistrationHostedService(
        ServiceDiscoveryClient discoveryClient,
        ILogger<ServiceRegistrationHostedService> logger,
        string serviceName,
        string serviceUrl)
    {
        _discoveryClient = discoveryClient;
        _logger = logger;
        _serviceName = serviceName;
        _serviceUrl = serviceUrl;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RegisterAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RegisterAsync(stoppingToken);
                _logger.LogDebug("Heartbeat sent for service {ServiceName}", _serviceName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send heartbeat for {ServiceName}", _serviceName);
            }
        }
    }

    private async Task RegisterAsync(CancellationToken cancellationToken)
    {
        var request = new ServiceRegistrationRequest(
            _serviceName,
            _serviceUrl,
            DateTime.UtcNow);

        await _discoveryClient.RegisterAsync(request, cancellationToken);
        _logger.LogInformation("Registered service {ServiceName} at {ServiceUrl}", _serviceName, _serviceUrl);
    }
}
