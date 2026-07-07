namespace SelfBudget.API.Application.Abstractions.Repositories;

public interface ITransactionCategoryRepository
{
    Task<Guid?> GetTransferIdAsync(CancellationToken cancellationToken);
}
