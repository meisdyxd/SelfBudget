using SelfBudget.API.Domain.Entities.TransactionContext;

namespace SelfBudget.API.Application.Abstractions.Repositories;

public interface ITransactionCategoryRepository
{
    Task<Guid?> GetTransferIdAsync(CancellationToken cancellationToken);
    Task<Guid?> GetTransactionCategoryByCode(string code, CancellationToken cancellationToken);
    Task<Guid?> GetTransactionCategoryByCode(TransactionCategoriesCodes code, CancellationToken cancellationToken);
}
