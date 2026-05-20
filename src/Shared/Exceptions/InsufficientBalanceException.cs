namespace BankingMicroservices.Shared.Exceptions;

public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException(decimal balance, decimal requested)
        : base($"Insufficient balance. Available: {balance}, Requested: {requested}.")
    {
        AvailableBalance = balance;
        RequestedAmount = requested;
    }

    public decimal AvailableBalance { get; }
    public decimal RequestedAmount { get; }
}
