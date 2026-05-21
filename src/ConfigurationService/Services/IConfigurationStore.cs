using BankingMicroservices.Shared.Models;

namespace BankingMicroservices.ConfigurationService.Services;

public interface IConfigurationStore
{
    ServiceConfiguration? Get(string serviceName);
}