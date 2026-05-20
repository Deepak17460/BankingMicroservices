using BankingMicroservices.Shared.Clients;
using BankingMicroservices.Shared.Hosting;
using BankingMicroservices.Shared.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BankingMicroservices.Shared.Extensions;

public static class BankingServiceCollectionExtensions
{
    public static IServiceCollection AddBankingInfrastructure(
        this IServiceCollection services,
        string serviceDiscoveryUrl,
        string configurationServiceUrl,
        string serviceName,
        string serviceUrl)
    {
        services.AddSingleton(new ServiceDiscoveryOptions(serviceDiscoveryUrl, configurationServiceUrl, serviceName, serviceUrl));
        services.AddSingleton<ServiceDiscoveryClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var options = sp.GetRequiredService<ServiceDiscoveryOptions>();
            return new ServiceDiscoveryClient(factory, options.ServiceDiscoveryUrl);
        });
        services.AddSingleton<ConfigurationServiceClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var options = sp.GetRequiredService<ServiceDiscoveryOptions>();
            return new ConfigurationServiceClient(factory, options.ConfigurationServiceUrl);
        });
        services.AddHostedService<ServiceRegistrationHostedService>(sp =>
        {
            var discovery = sp.GetRequiredService<ServiceDiscoveryClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ServiceRegistrationHostedService>>();
            var options = sp.GetRequiredService<ServiceDiscoveryOptions>();
            return new ServiceRegistrationHostedService(discovery, logger, options.ServiceName, options.ServiceUrl);
        });

        return services;
    }

    public static IServiceCollection AddBankingHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("ServiceDiscovery")
            .AddBankingResiliencePolicies();
        services.AddHttpClient("ConfigurationService")
            .AddBankingResiliencePolicies();
        services.AddHttpClient("InterService")
            .AddBankingResiliencePolicies();
        return services;
    }

    public static WebApplicationBuilder UseBankingSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console());
        return builder;
    }

    public static WebApplication UseBankingPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseGlobalExceptionHandling();
        return app;
    }
}

public record ServiceDiscoveryOptions(
    string ServiceDiscoveryUrl,
    string ConfigurationServiceUrl,
    string ServiceName,
    string ServiceUrl);
