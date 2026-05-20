using BankingMicroservices.AccountManagementService.Clients;
using BankingMicroservices.AccountManagementService.Models;
using BankingMicroservices.AccountManagementService.Repositories;
using BankingMicroservices.Shared.DTOs;
using BankingMicroservices.Shared.Exceptions;

namespace BankingMicroservices.AccountManagementService.Services;

public class AccountService
{
    private readonly IAccountRepository _repository;
    private readonly CustomerServiceClient _customerClient;

    public AccountService(IAccountRepository repository, CustomerServiceClient customerClient)
    {
        _repository = repository;
        _customerClient = customerClient;
    }

    public async Task<AccountDto> CreateAccountAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await _customerClient.EnsureCustomerExistsAsync(customerId, cancellationToken);

        var existing = await _repository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (existing is not null)
        {
            return MapToDto(existing);
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Balance = 0,
            AccountType = "Checking",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(account, cancellationToken);
        return MapToDto(account);
    }

    public async Task<AccountDto> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than zero.");
        }

        await _customerClient.EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);
        var account = await GetAccountByCustomerIdOrThrow(request.CustomerId, cancellationToken);
        account.Balance += request.Amount;
        await _repository.UpdateAsync(account, cancellationToken);
        return MapToDto(account);
    }

    public async Task<AccountDto> WithdrawAsync(WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be greater than zero.");
        }

        await _customerClient.EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);
        var account = await GetAccountByCustomerIdOrThrow(request.CustomerId, cancellationToken);

        if (account.Balance < request.Amount)
        {
            throw new InsufficientBalanceException(account.Balance, request.Amount);
        }

        account.Balance -= request.Amount;
        await _repository.UpdateAsync(account, cancellationToken);
        return MapToDto(account);
    }

    public async Task<AccountWithCustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(id, cancellationToken);
        if (account is null)
        {
            throw new AccountNotFoundException(id);
        }

        var customer = await _customerClient.GetCustomerAsync(account.CustomerId, cancellationToken);
        return new AccountWithCustomerDto(
            account.Id,
            account.CustomerId,
            account.Balance,
            account.AccountType,
            account.CreatedAt,
            customer);
    }

    public async Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteByCustomerIdAsync(customerId, cancellationToken);
        if (!deleted)
        {
            throw new AccountNotFoundException(customerId, byCustomerId: true);
        }
    }

    private async Task<Account> GetAccountByCustomerIdOrThrow(Guid customerId, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (account is null)
        {
            throw new AccountNotFoundException(customerId, byCustomerId: true);
        }

        return account;
    }

    private static AccountDto MapToDto(Account account) =>
        new(account.Id, account.CustomerId, account.Balance, account.AccountType, account.CreatedAt);
}
