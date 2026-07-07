namespace SelfBudget.API.Common.Dtos.Requests.TransferRequests;

public class TransferBetweenAccountsRequest
{
    public decimal Amount { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public Guid TransactionCategoryId { get; set; }
    public string? Note { get; set; }
}
