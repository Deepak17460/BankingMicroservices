using BankingMicroservices.ConfigurationService.Services;
using BankingMicroservices.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingMicroservices.ConfigurationService.Controllers;

[ApiController]
[Route("")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationStore _configurationStore;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(IConfigurationStore configurationStore, ILogger<ConfigurationController> logger)
    {
        _configurationStore = configurationStore;
        _logger = logger;
    }

    [HttpGet("config/{serviceName}")]
    public ActionResult<ServiceConfiguration> GetConfiguration(string serviceName)
    {
        _logger.LogInformation("Retrieving configuration for service: {ServiceName}", serviceName);
        
        var config = _configurationStore.Get(serviceName);
        if (config is null)
        {
            _logger.LogWarning("Configuration not found for service: {ServiceName}", serviceName);
            return NotFound(new { message = $"Configuration for '{serviceName}' not found." });
        }

        _logger.LogInformation("Configuration retrieved successfully for service: {ServiceName}", serviceName);
        
        return Ok(config);
    }
}