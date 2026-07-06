using SelfBudget.API.Entities;

namespace SelfBudget.API.Abstractions.Repositories;

public interface ITransactionRepository
{
    Task<Guid> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken);
}
