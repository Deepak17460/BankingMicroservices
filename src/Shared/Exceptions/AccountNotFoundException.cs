namespace BankingMicroservices.Shared.Exceptions;

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid identifier, bool byCustomerId = false)
        : base(byCustomerId
            ? $"Account for customer '{identifier}' was not found."
            : $"Account with id '{identifier}' was not found.")
    {
        Identifier = identifier;
        ByCustomerId = byCustomerId;
    }

    public Guid Identifier { get; }
    public bool ByCustomerId { get; }
}
