using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Infrastructure.Database;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Domain.Entities.TransactionContext;

namespace SelfBudget.API.Infrastructure.Repositories.TransactionRepositories;

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
            .FirstOrDefaultAsync(tc => tc.Name == "Перевод между счетами", cancellationToken);

        return result?.Id;
    }

    public async Task<Guid?> GetTransactionCategoryByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _dbContext.TransactionCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(tc => tc.Code == code);

        return result?.Id;
    }

    public async Task<Guid?> GetTransactionCategoryByCode(TransactionCategoriesCodes code, CancellationToken cancellationToken)
    {
        return await GetTransactionCategoryByCode(code.ToString(), cancellationToken);
    }
}
