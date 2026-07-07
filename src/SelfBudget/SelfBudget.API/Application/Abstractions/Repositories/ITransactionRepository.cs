using SelfBudget.API.Common.Dtos.Responses;
using SelfBudget.API.Domain.Entities.TransactionContext;

namespace SelfBudget.API.Application.Abstractions.Repositories;

public interface ITransactionRepository
{
    Task<Guid> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken);
    Task<TransferResponse?> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken);
}
