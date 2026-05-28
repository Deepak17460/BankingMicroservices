using BankingMicroservices.Shared.Extensions;
using BankingMicroservices.ApiGateway.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.UseBankingSerilog();

// Configure Ocelot configuration file
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var serviceDiscoveryUrl = Environment.GetEnvironmentVariable("SERVICE_DISCOVERY_URL")
    ?? builder.Configuration["Bootstrap:ServiceDiscoveryUrl"]
    ?? "http://localhost:5003";
var configurationServiceUrl = Environment.GetEnvironmentVariable("CONFIGURATION_SERVICE_URL")
    ?? builder.Configuration["Bootstrap:ConfigurationServiceUrl"]
    ?? "http://localhost:5004";
var serviceUrl = Environment.GetEnvironmentVariable("SERVICE_URL")
    ?? builder.Configuration["Bootstrap:ServiceUrl"]
    ?? "http://localhost:5010";

builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "api-gateway",
    serviceUrl);

// Add Ocelot
builder.Services.AddOcelot();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Banking API Gateway", 
        Version = "v1",
        Description = "Ocelot API Gateway for Banking Microservices"
    });
});

var app = builder.Build();
app.UseBankingPipeline();

// Configure Swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking API Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

// Add correlation ID middleware before Ocelot
app.UseCorrelationId();

// Health check endpoint - use Map to bypass Ocelot routing
app.Map("/health", healthApp =>
{
    healthApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"status\":\"UP\",\"service\":\"api-gateway\"}");
    });
});

// Use Ocelot middleware (this will handle all other routes)
await app.UseOcelot();

app.Run();
