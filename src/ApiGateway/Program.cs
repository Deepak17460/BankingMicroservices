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
    ?? "http://localhost:5000";

builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "api-gateway",
    serviceUrl);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.UseBankingPipeline();
app.MapReverseProxy();
app.Run();
