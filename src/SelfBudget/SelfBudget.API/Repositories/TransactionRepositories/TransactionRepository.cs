using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Database;
using SelfBudget.API.Dtos.Responses;
using SelfBudget.API.Entities.TransactionContext;

namespace SelfBudget.API.Repositories.TransactionRepositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TransactionRepository> _logger;

    public TransactionRepository(
        AppDbContext dbContext,
        ILogger<TransactionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> CreateTransactionAsync(
        Transaction transaction,
        CancellationToken cancellationToken)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);

        _logger.LogInformation("Создана новая транзакция с ID: '{TransactionId}'", transaction.Id);
        return transaction.Id;
    }

    public async Task<TransferResponse?> GetTransactionByIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Transactions
            .Where(t => t.Id == transactionId)
            .Select(t => new TransferResponse
            {
                Amount = t.Amount,
                Id = t.Id,
                CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
                FromAccountId = t.FromAccountId,
                ToAccountId = t.ToAccountId,
                Status = "success",
                Note = t.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}
