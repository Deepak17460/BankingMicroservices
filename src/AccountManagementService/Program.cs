using System.Net.Http.Json;
using BankingMicroservices.AccountManagementService.Clients;
using BankingMicroservices.AccountManagementService.Configuration;
using BankingMicroservices.AccountManagementService.Repositories;
using BankingMicroservices.AccountManagementService.Services;
using BankingMicroservices.Shared.DTOs;
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
    ?? "http://localhost:5002";

// Try to load remote configuration, fall back to defaults if not available
ServiceConfiguration remoteConfig;
try
{
    using var bootstrapClient = new HttpClient();
    bootstrapClient.Timeout = TimeSpan.FromSeconds(5);
    var configResponse = await bootstrapClient.GetAsync($"{configurationServiceUrl.TrimEnd('/')}/config/account-management");
    if (configResponse.IsSuccessStatusCode)
    {
        remoteConfig = await configResponse.Content.ReadFromJsonAsync<ServiceConfiguration>()
            ?? new ServiceConfiguration(new Dictionary<string, string>());
    }
    else
    {
        remoteConfig = new ServiceConfiguration(new Dictionary<string, string>());
    }
}
catch
{
    // Configuration service not available, use defaults
    remoteConfig = new ServiceConfiguration(new Dictionary<string, string>());
}

var settings = new AccountServiceSettings
{
    CustomerServiceName = remoteConfig.Settings.GetValueOrDefault("CustomerServiceName", "customer-management")!
};

builder.Services.AddSingleton(settings);
builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "account-management",
    serviceUrl);

// Repository
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();

// Clients
builder.Services.AddSingleton<CustomerServiceClient>();

// Services
builder.Services.AddSingleton<IAccountService, AccountService>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseBankingPipeline();

// Configure Swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
