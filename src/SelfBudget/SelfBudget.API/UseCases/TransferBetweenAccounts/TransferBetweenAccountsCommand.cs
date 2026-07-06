namespace SelfBudget.API.UseCases.TransferBetweenAccounts;

public class TransferBetweenAccountsCommand
{
    public decimal Amount { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public Guid TransactionCategoryId { get; set; }
    public string? Note { get; set; }
}
