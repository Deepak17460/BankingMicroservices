using System.Collections.Concurrent;
using BankingMicroservices.Shared.Models;

namespace BankingMicroservices.ConfigurationService.Services;

public class ConfigurationStore
{
    private readonly ConcurrentDictionary<string, ServiceConfiguration> _configs = new();

    public ConfigurationStore()
    {
        _configs["customer-management"] = new ServiceConfiguration(new Dictionary<string, string>
        {
            ["AccountServiceName"] = "account-management"
        });

        _configs["account-management"] = new ServiceConfiguration(new Dictionary<string, string>
        {
            ["CustomerServiceName"] = "customer-management"
        });

        _configs["api-gateway"] = new ServiceConfiguration(new Dictionary<string, string>
        {
            ["CustomerServiceName"] = "customer-management",
            ["AccountServiceName"] = "account-management"
        });

        _configs["service-discovery"] = new ServiceConfiguration(new Dictionary<string, string>());
        _configs["configuration-service"] = new ServiceConfiguration(new Dictionary<string, string>());
    }

    public ServiceConfiguration? Get(string serviceName) =>
        _configs.TryGetValue(serviceName, out var config) ? config : null;
}
