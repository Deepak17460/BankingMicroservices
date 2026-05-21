using BankingMicroservices.Shared.DTOs;

namespace BankingMicroservices.AccountManagementService.Services;

public interface IAccountService
{
    Task<AccountDto> CreateAccountAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<AccountDto> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default);
    Task<AccountDto> WithdrawAsync(WithdrawRequest request, CancellationToken cancellationToken = default);
    Task<AccountWithCustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}