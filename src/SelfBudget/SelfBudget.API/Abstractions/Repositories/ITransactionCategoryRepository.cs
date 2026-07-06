namespace SelfBudget.API.Abstractions.Repositories;

public interface ITransactionCategoryRepository
{
    Task<Guid> GetTransferIdAsync(CancellationToken cancellationToken);
}
