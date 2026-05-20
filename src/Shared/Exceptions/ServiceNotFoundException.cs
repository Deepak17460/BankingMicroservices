namespace BankingMicroservices.Shared.Exceptions;

public class ServiceNotFoundException : Exception
{
    public ServiceNotFoundException(string serviceName)
        : base($"Service '{serviceName}' was not found in the registry.")
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; }
}
