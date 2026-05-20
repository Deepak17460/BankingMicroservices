using BankingMicroservices.AccountManagementService.Models;

namespace BankingMicroservices.AccountManagementService.Repositories;

public interface IAccountRepository
{
    Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Account?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Account?> UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task<bool> DeleteByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
