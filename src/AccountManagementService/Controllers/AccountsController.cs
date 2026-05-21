using BankingMicroservices.AccountManagementService.Services;
using BankingMicroservices.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankingMicroservices.AccountManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccountAsync(
        [FromBody] CreateAccountRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating account for customer: {CustomerId}", request.CustomerId);
        
        var account = await _accountService.CreateAccountAsync(request.CustomerId, cancellationToken);
        
        _logger.LogInformation("Account created successfully with ID: {AccountId} for customer: {CustomerId}", 
            account.Id, account.CustomerId);
        
        return CreatedAtAction(
            nameof(GetByIdAsync), 
            new { id = account.Id }, 
            ApiResponse<AccountDto>.Ok(account));
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> DepositAsync(
        [FromBody] DepositRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing deposit of {Amount} for customer: {CustomerId}", 
            request.Amount, request.CustomerId);
        
        var account = await _accountService.DepositAsync(request, cancellationToken);
        
        _logger.LogInformation("Deposit successful. New balance: {Balance} for customer: {CustomerId}", 
            account.Balance, account.CustomerId);
        
        return Ok(ApiResponse<AccountDto>.Ok(account, "Deposit successful."));
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> WithdrawAsync(
        [FromBody] WithdrawRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing withdrawal of {Amount} for customer: {CustomerId}", 
            request.Amount, request.CustomerId);
        
        var account = await _accountService.WithdrawAsync(request, cancellationToken);
        
        _logger.LogInformation("Withdrawal successful. New balance: {Balance} for customer: {CustomerId}", 
            account.Balance, account.CustomerId);
        
        return Ok(ApiResponse<AccountDto>.Ok(account, "Withdrawal successful."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AccountWithCustomerDto>>> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving account with ID: {AccountId}", id);
        
        var account = await _accountService.GetByIdAsync(id, cancellationToken);
        
        _logger.LogInformation("Account retrieved successfully: {AccountId}", account.Id);
        
        return Ok(ApiResponse<AccountWithCustomerDto>.Ok(account));
    }

    [HttpDelete("customer/{customerId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteByCustomerIdAsync(
        Guid customerId, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting account for customer: {CustomerId}", customerId);
        
        await _accountService.DeleteByCustomerIdAsync(customerId, cancellationToken);
        
        _logger.LogInformation("Account deleted successfully for customer: {CustomerId}", customerId);
        
        return Ok(ApiResponse<object>.Ok(null!, "Account deleted."));
    }
}