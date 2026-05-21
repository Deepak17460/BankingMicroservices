using BankingMicroservices.ServiceDiscovery.Services;
using BankingMicroservices.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingMicroservices.ServiceDiscovery.Controllers;

[ApiController]
[Route("")]
public class ServiceDiscoveryController : ControllerBase
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceDiscoveryController> _logger;

    public ServiceDiscoveryController(IServiceRegistry serviceRegistry, ILogger<ServiceDiscoveryController> logger)
    {
        _serviceRegistry = serviceRegistry;
        _logger = logger;
    }

    [HttpPost("register")]
    public ActionResult RegisterService([FromBody] ServiceRegistrationRequest request)
    {
        _logger.LogInformation("Registering service: {ServiceName} at {ServiceUrl}", 
            request.Name, request.Url);
        
        _serviceRegistry.Register(request);
        
        _logger.LogInformation("Service registered successfully: {ServiceName}", request.Name);
        
        return Ok(new { message = $"Service '{request.Name}' registered." });
    }

    [HttpGet("discover/{serviceName}")]
    public ActionResult<DiscoverResponse> DiscoverService(string serviceName)
    {
        _logger.LogInformation("Discovering service: {ServiceName}", serviceName);
        
        var service = _serviceRegistry.Get(serviceName);
        if (service is null)
        {
            _logger.LogWarning("Service not found: {ServiceName}", serviceName);
            return NotFound(new { message = $"Service '{serviceName}' not found." });
        }

        _logger.LogInformation("Service discovered: {ServiceName} at {ServiceUrl}", 
            service.Name, service.Url);
        
        return Ok(new DiscoverResponse(service.Name, service.Url, service.LastHeartbeat));
    }
}