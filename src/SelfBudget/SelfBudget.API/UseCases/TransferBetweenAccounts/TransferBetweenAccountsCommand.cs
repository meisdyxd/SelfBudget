using SelfBudget.API.Dtos.Requests.TransferRequests;

namespace SelfBudget.API.UseCases.TransferBetweenAccounts;

public class TransferBetweenAccountsCommand
{
    public decimal Amount { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public Guid TransactionCategoryId { get; set; }
    public string? Note { get; set; }

    public static TransferBetweenAccountsCommand FromRequest(TransferBetweenAccountsRequest request)
    {
        return new()
        {
            Amount = request.Amount,
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Note = request.Note,
            TransactionCategoryId = request.TransactionCategoryId
        };
    }
}
