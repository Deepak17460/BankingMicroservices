using BankingMicroservices.ServiceDiscovery.Services;

namespace BankingMicroservices.ServiceDiscovery.Hosting;

public class StaleServiceCleanupHostedService : BackgroundService
{
    private readonly ServiceRegistry _registry;
    private readonly ILogger<StaleServiceCleanupHostedService> _logger;

    public StaleServiceCleanupHostedService(
        ServiceRegistry registry,
        ILogger<StaleServiceCleanupHostedService> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var removed = _registry.RemoveStaleServices();
            if (removed > 0)
            {
                _logger.LogInformation("Removed {Count} stale service(s) from registry", removed);
            }
        }
    }
}
