using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Database;
using SelfBudget.API.Dtos;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Repositories;

public class AccountTypeRepository : IAccountTypeRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AccountTypeRepository> _logger;

    public AccountTypeRepository(
        AppDbContext dbContext,
        ILogger<AccountTypeRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> CreateAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken)
    {
        await _dbContext.AccountTypes.AddAsync(accountType, cancellationToken);
        _logger.LogInformation("Создан тип счета с именем: '{AccountTypeName}'", accountType.Name);

        return accountType.Id;
    }

    public async Task DeleteAccountTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        await _dbContext.AccountTypes
            .Where(at => at.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        _logger.LogInformation("Удален тип счета с идентификатором: '{AccountTypeId}'", id);
    }

    public async Task<ICollection<AccountTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _dbContext.AccountTypes
            .Select(at => new AccountTypeDto
            {
                Id = at.Id,
                Name = at.Name,
                Description = at.Description
            })
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<AccountTypeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _dbContext.AccountTypes
            .Where(at => at.Id == id)
            .Select(at => new AccountTypeDto
            {
                Id = at.Id,
                Name = at.Name,
                Description = at.Description
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}
