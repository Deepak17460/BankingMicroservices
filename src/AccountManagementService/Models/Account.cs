namespace BankingMicroservices.AccountManagementService.Models;

public class Account
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Balance { get; set; }
    public string AccountType { get; set; } = "Checking";
    public DateTime CreatedAt { get; set; }
}
