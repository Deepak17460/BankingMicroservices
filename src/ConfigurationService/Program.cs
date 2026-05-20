using BankingMicroservices.ConfigurationService.Services;
using BankingMicroservices.Shared.Extensions;

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
    ?? "http://localhost:5004";

builder.Services.AddSingleton<ConfigurationStore>();
builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "configuration-service",
    serviceUrl);

var app = builder.Build();
app.UseBankingPipeline();

app.MapGet("/config/{serviceName}", (string serviceName, ConfigurationStore store) =>
{
    var config = store.Get(serviceName);
    if (config is null)
    {
        return Results.NotFound(new { message = $"Configuration for '{serviceName}' not found." });
    }

    return Results.Ok(config);
});

app.Run();
