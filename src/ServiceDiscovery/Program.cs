using BankingMicroservices.ServiceDiscovery.Hosting;
using BankingMicroservices.ServiceDiscovery.Services;
using BankingMicroservices.Shared.Extensions;
using BankingMicroservices.Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.UseBankingSerilog();

var serviceDiscoveryUrl = builder.Configuration["Bootstrap:ServiceDiscoveryUrl"]
    ?? Environment.GetEnvironmentVariable("SERVICE_DISCOVERY_URL")
    ?? "http://localhost:5003";
var configurationServiceUrl = builder.Configuration["Bootstrap:ConfigurationServiceUrl"]
    ?? Environment.GetEnvironmentVariable("CONFIGURATION_SERVICE_URL")
    ?? "http://localhost:5004";
var serviceUrl = builder.Configuration["Bootstrap:ServiceUrl"]
    ?? Environment.GetEnvironmentVariable("SERVICE_URL")
    ?? "http://localhost:5003";

builder.Services.AddSingleton<ServiceRegistry>();
builder.Services.AddHostedService<StaleServiceCleanupHostedService>();
builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "service-discovery",
    serviceUrl);

var app = builder.Build();
app.UseBankingPipeline();

app.MapPost("/register", (ServiceRegistrationRequest request, ServiceRegistry registry) =>
{
    registry.Register(request);
    return Results.Ok(new { message = $"Service '{request.Name}' registered." });
});

app.MapGet("/discover/{serviceName}", (string serviceName, ServiceRegistry registry) =>
{
    var service = registry.Get(serviceName);
    if (service is null)
    {
        return Results.NotFound(new { message = $"Service '{serviceName}' not found." });
    }

    return Results.Ok(new DiscoverResponse(service.Name, service.Url, service.LastHeartbeat));
});

app.Run();
