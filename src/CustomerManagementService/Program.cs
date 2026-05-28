using System.Net.Http.Json;
using BankingMicroservices.CustomerManagementService.Clients;
using BankingMicroservices.CustomerManagementService.Configuration;
using BankingMicroservices.CustomerManagementService.Repositories;
using BankingMicroservices.CustomerManagementService.Services;
using BankingMicroservices.Shared.DTOs;
using BankingMicroservices.Shared.Extensions;
using BankingMicroservices.Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.UseBankingSerilog();

var serviceDiscoveryUrl = Environment.GetEnvironmentVariable("SERVICE_DISCOVERY_URL")
    ?? builder.Configuration["Bootstrap:ServiceDiscoveryUrl"]
    ?? "http://localhost:5003";
var configurationServiceUrl = Environment.GetEnvironmentVariable("CONFIGURATION_SERVICE_URL")
    ?? builder.Configuration["Bootstrap:ConfigurationServiceUrl"]
    ?? "http://localhost:5004";
var serviceUrl = Environment.GetEnvironmentVariable("SERVICE_URL")
    ?? builder.Configuration["Bootstrap:ServiceUrl"]
    ?? "http://localhost:5001";

// Try to load remote configuration, fall back to defaults if not available
ServiceConfiguration remoteConfig;
try
{
    using var bootstrapClient = new HttpClient();
    bootstrapClient.Timeout = TimeSpan.FromSeconds(5);
    var configResponse = await bootstrapClient.GetAsync($"{configurationServiceUrl.TrimEnd('/')}/config/customer-management");
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

var settings = new CustomerServiceSettings
{
    AccountServiceName = remoteConfig.Settings.GetValueOrDefault("AccountServiceName", "account-management")!
};

builder.Services.AddSingleton(settings);
builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "customer-management",
    serviceUrl);

// Repository
builder.Services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();

// Clients
builder.Services.AddSingleton<AccountServiceClient>();

// Services
builder.Services.AddSingleton<ICustomerService, CustomerService>();

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

// Health check endpoint
app.MapGet("/health", () => Results.Json(new { status = "UP", service = "customer-management" }));

app.Run();
