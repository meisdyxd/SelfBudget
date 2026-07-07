using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Database;
using Microsoft.EntityFrameworkCore;

namespace SelfBudget.API.Repositories.TransactionRepositories;

public class TransactionCategoryRepository : ITransactionCategoryRepository
{
    private readonly AppDbContext _dbContext;

    public TransactionCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> GetTransferIdAsync(CancellationToken cancellationToken)
    {
        var result = await _dbContext.TransactionCategories
            .Select(tc => new { tc.Id, tc.Name })
            .FirstOrDefaultAsync(tc => tc.Name == "Перевод между счетами");

        return result?.Id;
    }
}
