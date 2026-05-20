namespace BankingMicroservices.Shared.Exceptions;

public class CustomerNotFoundException : Exception
{
    public CustomerNotFoundException(Guid customerId)
        : base($"Customer with id '{customerId}' was not found.")
    {
        CustomerId = customerId;
    }

    public Guid CustomerId { get; }
}
