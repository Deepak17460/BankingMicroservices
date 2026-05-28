using BankingMicroservices.ConfigurationService.Services;
using BankingMicroservices.Shared.Extensions;

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
    ?? "http://localhost:5004";

// Services
builder.Services.AddSingleton<IConfigurationStore, ConfigurationStore>();

builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "configuration-service",
    serviceUrl);

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
app.MapGet("/health", () => Results.Json(new { status = "UP", service = "configuration-service" }));

app.Run();
