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

var serviceDiscoveryUrl = builder.Configuration["Bootstrap:ServiceDiscoveryUrl"]
    ?? Environment.GetEnvironmentVariable("SERVICE_DISCOVERY_URL")
    ?? "http://localhost:5003";
var configurationServiceUrl = builder.Configuration["Bootstrap:ConfigurationServiceUrl"]
    ?? Environment.GetEnvironmentVariable("CONFIGURATION_SERVICE_URL")
    ?? "http://localhost:5004";
var serviceUrl = builder.Configuration["Bootstrap:ServiceUrl"]
    ?? Environment.GetEnvironmentVariable("SERVICE_URL")
    ?? "http://localhost:5001";

using var bootstrapClient = new HttpClient();
var configResponse = await bootstrapClient.GetAsync($"{configurationServiceUrl.TrimEnd('/')}/config/customer-management");
configResponse.EnsureSuccessStatusCode();
var remoteConfig = await configResponse.Content.ReadFromJsonAsync<ServiceConfiguration>()
    ?? new ServiceConfiguration(new Dictionary<string, string>());

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
builder.Services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
builder.Services.AddSingleton<AccountServiceClient>();
builder.Services.AddSingleton<CustomerService>();

var app = builder.Build();
app.UseBankingPipeline();

app.MapPost("/api/customers", async (CreateCustomerRequest request, CustomerService service, CancellationToken ct) =>
{
    var customer = await service.CreateAsync(request, ct);
    return Results.Created($"/api/customers/{customer.Id}", ApiResponse<CustomerDto>.Ok(customer));
});

app.MapGet("/api/customers", async (CustomerService service, CancellationToken ct) =>
{
    var customers = await service.GetAllAsync(ct);
    return Results.Ok(ApiResponse<IReadOnlyList<CustomerDto>>.Ok(customers));
});

app.MapGet("/api/customers/{id:guid}", async (Guid id, CustomerService service, CancellationToken ct) =>
{
    var customer = await service.GetByIdAsync(id, ct);
    return Results.Ok(ApiResponse<CustomerDto>.Ok(customer));
});

app.MapPut("/api/customers/{id:guid}", async (Guid id, UpdateCustomerRequest request, CustomerService service, CancellationToken ct) =>
{
    var customer = await service.UpdateAsync(id, request, ct);
    return Results.Ok(ApiResponse<CustomerDto>.Ok(customer));
});

app.MapDelete("/api/customers/{id:guid}", async (Guid id, CustomerService service, CancellationToken ct) =>
{
    await service.DeleteAsync(id, ct);
    return Results.Ok(ApiResponse<object>.Ok(null!, "Customer and associated account deleted."));
});

app.Run();
