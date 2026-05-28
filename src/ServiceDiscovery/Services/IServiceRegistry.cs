using BankingMicroservices.Shared.Models;

namespace BankingMicroservices.ServiceDiscovery.Services;

public interface IServiceRegistry
{
    void Register(ServiceRegistrationRequest request);
    ServiceRegistrationRequest? Get(string serviceName);
    IEnumerable<ServiceRegistrationRequest> GetAll();
    int RemoveStaleServices();
}