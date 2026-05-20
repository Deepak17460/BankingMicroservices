using System.Collections.Concurrent;
using BankingMicroservices.Shared.Models;

namespace BankingMicroservices.ServiceDiscovery.Services;

public class ServiceRegistry
{
    private readonly ConcurrentDictionary<string, ServiceRegistrationRequest> _services = new();
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromSeconds(30);

    public void Register(ServiceRegistrationRequest request)
    {
        _services.AddOrUpdate(
            request.Name,
            request with { LastHeartbeat = DateTime.UtcNow },
            (_, _) => request with { LastHeartbeat = DateTime.UtcNow });
    }

    public ServiceRegistrationRequest? Get(string serviceName)
    {
        if (_services.TryGetValue(serviceName, out var service))
        {
            if (DateTime.UtcNow - service.LastHeartbeat <= StaleThreshold)
            {
                return service;
            }
        }

        return null;
    }

    public int RemoveStaleServices()
    {
        var staleKeys = _services
            .Where(kvp => DateTime.UtcNow - kvp.Value.LastHeartbeat > StaleThreshold)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in staleKeys)
        {
            _services.TryRemove(key, out _);
        }

        return staleKeys.Count;
    }
}
