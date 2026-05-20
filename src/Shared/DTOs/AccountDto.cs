namespace BankingMicroservices.Shared.DTOs;

public record AccountDto(
    Guid Id,
    Guid CustomerId,
    decimal Balance,
    string AccountType,
    DateTime CreatedAt,
    CustomerDto? Customer = null);

public record CreateAccountRequest(Guid CustomerId);

public record DepositRequest(Guid CustomerId, decimal Amount);

public record WithdrawRequest(Guid CustomerId, decimal Amount);

public record AccountWithCustomerDto(
    Guid Id,
    Guid CustomerId,
    decimal Balance,
    string AccountType,
    DateTime CreatedAt,
    CustomerDto Customer);
