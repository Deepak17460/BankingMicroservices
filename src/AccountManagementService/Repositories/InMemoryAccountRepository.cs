using System.Collections.Concurrent;
using BankingMicroservices.AccountManagementService.Models;

namespace BankingMicroservices.AccountManagementService.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<Guid, Account> _accountsById = new();
    private readonly ConcurrentDictionary<Guid, Guid> _accountIdByCustomerId = new();

    public Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        _accountsById[account.Id] = account;
        _accountIdByCustomerId[account.CustomerId] = account.Id;
        return Task.FromResult(account);
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _accountsById.TryGetValue(id, out var account);
        return Task.FromResult(account);
    }

    public Task<Account?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (_accountIdByCustomerId.TryGetValue(customerId, out var accountId) &&
            _accountsById.TryGetValue(accountId, out var account))
        {
            return Task.FromResult<Account?>(account);
        }

        return Task.FromResult<Account?>(null);
    }

    public Task<Account?> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (!_accountsById.ContainsKey(account.Id))
        {
            return Task.FromResult<Account?>(null);
        }

        _accountsById[account.Id] = account;
        return Task.FromResult<Account?>(account);
    }

    public Task<bool> DeleteByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (!_accountIdByCustomerId.TryRemove(customerId, out var accountId))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_accountsById.TryRemove(accountId, out _));
    }
}
