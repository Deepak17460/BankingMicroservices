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

using var bootstrapClient = new HttpClient();
var configResponse = await bootstrapClient.GetAsync($"{configurationServiceUrl.TrimEnd('/')}/config/account-management");
configResponse.EnsureSuccessStatusCode();
var remoteConfig = await configResponse.Content.ReadFromJsonAsync<ServiceConfiguration>()
    ?? new ServiceConfiguration(new Dictionary<string, string>());

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
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<CustomerServiceClient>();
builder.Services.AddSingleton<AccountService>();

var app = builder.Build();
app.UseBankingPipeline();

app.MapPost("/api/accounts", async (CreateAccountRequest request, AccountService service, CancellationToken ct) =>
{
    var account = await service.CreateAccountAsync(request.CustomerId, ct);
    return Results.Created($"/api/accounts/{account.Id}", ApiResponse<AccountDto>.Ok(account));
});

app.MapPost("/api/accounts/deposit", async (DepositRequest request, AccountService service, CancellationToken ct) =>
{
    var account = await service.DepositAsync(request, ct);
    return Results.Ok(ApiResponse<AccountDto>.Ok(account, "Deposit successful."));
});

app.MapPost("/api/accounts/withdraw", async (WithdrawRequest request, AccountService service, CancellationToken ct) =>
{
    var account = await service.WithdrawAsync(request, ct);
    return Results.Ok(ApiResponse<AccountDto>.Ok(account, "Withdrawal successful."));
});

app.MapGet("/api/accounts/{id:guid}", async (Guid id, AccountService service, CancellationToken ct) =>
{
    var account = await service.GetByIdAsync(id, ct);
    return Results.Ok(ApiResponse<AccountWithCustomerDto>.Ok(account));
});

app.MapDelete("/api/accounts/customer/{customerId:guid}", async (Guid customerId, AccountService service, CancellationToken ct) =>
{
    await service.DeleteByCustomerIdAsync(customerId, ct);
    return Results.Ok(ApiResponse<object>.Ok(null!, "Account deleted."));
});

app.Run();
