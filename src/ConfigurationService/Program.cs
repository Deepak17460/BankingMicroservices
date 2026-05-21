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
app.Run();
