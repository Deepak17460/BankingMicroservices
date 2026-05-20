using System.Collections.Concurrent;
using BankingMicroservices.CustomerManagementService.Models;

namespace BankingMicroservices.CustomerManagementService.Repositories;

public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly ConcurrentDictionary<Guid, Customer> _customers = new();

    public Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _customers[customer.Id] = customer;
        return Task.FromResult(customer);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _customers.TryGetValue(id, out var customer);
        return Task.FromResult(customer);
    }

    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Customer> customers = _customers.Values.OrderBy(c => c.CreatedAt).ToList();
        return Task.FromResult(customers);
    }

    public Task<Customer?> UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (!_customers.ContainsKey(customer.Id))
        {
            return Task.FromResult<Customer?>(null);
        }

        _customers[customer.Id] = customer;
        return Task.FromResult<Customer?>(customer);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_customers.TryRemove(id, out _));
    }
}
