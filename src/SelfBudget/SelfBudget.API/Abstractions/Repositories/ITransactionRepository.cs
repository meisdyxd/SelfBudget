using SelfBudget.API.Dtos.Responses;
using SelfBudget.API.Entities.TransactionContext;

namespace SelfBudget.API.Abstractions.Repositories;

public interface ITransactionRepository
{
    Task<Guid> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<TransferResponse> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken);
}
