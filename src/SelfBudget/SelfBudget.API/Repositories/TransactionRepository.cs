using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Database;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Repositories;

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

    public async Task<Guid> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);

        _logger.LogInformation("Создана новая транзакция с ID: '{TransactionId}'", transaction.Id);
        return transaction.Id;
    }
}
