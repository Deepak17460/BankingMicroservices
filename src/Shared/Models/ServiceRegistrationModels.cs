namespace BankingMicroservices.Shared.Models;

public record ServiceRegistrationRequest(string Name, string Url, DateTime LastHeartbeat);

public record DiscoverResponse(string Name, string Url, DateTime LastHeartbeat);

public record ServiceConfiguration(Dictionary<string, string> Settings);
