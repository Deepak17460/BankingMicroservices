using BankingMicroservices.CustomerManagementService.Services;
using BankingMicroservices.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankingMicroservices.CustomerManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateAsync(
        [FromBody] CreateCustomerRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer with email: {Email}", request.Email);
        
        var customer = await _customerService.CreateAsync(request, cancellationToken);
        
        _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);
        
        return CreatedAtAction(
            nameof(GetByIdAsync), 
            new { id = customer.Id }, 
            ApiResponse<CustomerDto>.Ok(customer));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CustomerDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all customers");
        
        var customers = await _customerService.GetAllAsync(cancellationToken);
        
        _logger.LogInformation("Retrieved {Count} customers", customers.Count);
        
        return Ok(ApiResponse<IReadOnlyList<CustomerDto>>.Ok(customers));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
        
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        
        _logger.LogInformation("Customer retrieved successfully: {CustomerId}", customer.Id);
        
        return Ok(ApiResponse<CustomerDto>.Ok(customer));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateAsync(
        Guid id, 
        [FromBody] UpdateCustomerRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", id);
        
        var customer = await _customerService.UpdateAsync(id, request, cancellationToken);
        
        _logger.LogInformation("Customer updated successfully: {CustomerId}", customer.Id);
        
        return Ok(ApiResponse<CustomerDto>.Ok(customer));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
        
        await _customerService.DeleteAsync(id, cancellationToken);
        
        _logger.LogInformation("Customer and associated account deleted successfully: {CustomerId}", id);
        
        return Ok(ApiResponse<object>.Ok(null!, "Customer and associated account deleted."));
    }
}