namespace BankingMicroservices.Shared.DTOs;

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string Address,
    DateTime CreatedAt);

public record CreateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string Address);

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string Address);
