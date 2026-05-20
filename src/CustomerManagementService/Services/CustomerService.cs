using BankingMicroservices.CustomerManagementService.Clients;
using BankingMicroservices.CustomerManagementService.Models;
using BankingMicroservices.CustomerManagementService.Repositories;
using BankingMicroservices.Shared.DTOs;
using BankingMicroservices.Shared.Exceptions;

namespace BankingMicroservices.CustomerManagementService.Services;

public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly AccountServiceClient _accountClient;

    public CustomerService(ICustomerRepository repository, AccountServiceClient accountClient)
    {
        _repository = repository;
        _accountClient = accountClient;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(customer, cancellationToken);
        await _accountClient.CreateAccountAsync(customer.Id, cancellationToken);

        return MapToDto(customer);
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _repository.GetAllAsync(cancellationToken);
        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            throw new CustomerNotFoundException(id);
        }

        return MapToDto(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new CustomerNotFoundException(id);
        }

        existing.Name = request.Name;
        existing.Email = request.Email;
        existing.Phone = request.Phone;
        existing.Address = request.Address;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new CustomerNotFoundException(id);
        }

        await _accountClient.DeleteAccountByCustomerIdAsync(id, cancellationToken);
        await _repository.DeleteAsync(id, cancellationToken);
    }

    private static CustomerDto MapToDto(Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, customer.Phone, customer.Address, customer.CreatedAt);
}
