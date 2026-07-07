using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Database;
using SelfBudget.API.Dtos.AccountDtos;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Repositories.AccountRepositories;

public class AccountRepository : IAccountRepository
{
    private readonly ILogger<AccountRepository> _logger;
    private readonly AppDbContext _dbContext;

    public AccountRepository(
        ILogger<AccountRepository> logger,
        AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateAccountAsync(Account account, CancellationToken cancellationToken)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
        _logger.LogInformation("Создан счёт с идентификатором: '{AccountId}'", account.Id);

        return account.Id;
    }

    public async Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        await _dbContext.Accounts
            .Where(a => a.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Удалён счёт с идентификатором: '{AccountId}'", id);
    }

    public async Task<ICollection<AccountDto>> GetAccountsByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var accounts = await _dbContext.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Accounts)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Balance = a.Balance,
                CurrencyCode = a.CurrencyCode,
                Name = a.Name,
                OverdraftLimit = a.OverdraftLimit,
                Type = a.Type.Name,
                TypeId = a.TypeId,
                UserId = a.UserId
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Получены счета у пользователся с идентификатором: '{UserId}'", userId);
        return accounts;
    }

    public async Task<ICollection<AccountDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var accounts = await _dbContext.Accounts
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Balance = a.Balance,
                CurrencyCode = a.CurrencyCode,
                Name = a.Name,
                OverdraftLimit = a.OverdraftLimit,
                Type = a.Type.Name,
                TypeId = a.TypeId,
                UserId = a.UserId
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Получены все счета");
        return accounts;
    }

    public async Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts
            .Where(a => a.Id == id)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Balance = a.Balance,
                CurrencyCode = a.CurrencyCode,
                Name = a.Name,
                OverdraftLimit = a.OverdraftLimit,
                Type = a.Type.Name,
                TypeId = a.TypeId,
                UserId = a.UserId
            })
            .SingleOrDefaultAsync(cancellationToken);

        _logger.LogInformation("Получен счёт с идентификатором: '{AccountId}'", id);
        return account;
    }

    public async Task UpdateAsync(AccountDto account, CancellationToken cancellationToken)
    {
        await _dbContext.Accounts
            .Where(a => a.Id == account.Id)
            .ExecuteUpdateAsync(u =>
            u.SetProperty(a => a.TypeId, account.TypeId)
            .SetProperty(a => a.Name, account.Name)
            .SetProperty(a => a.CurrencyCode, account.CurrencyCode)
            .SetProperty(a => a.Balance, account.Balance)
            .SetProperty(a => a.OverdraftLimit, account.OverdraftLimit),
            cancellationToken);
    }

    public async Task PatchAsync(Account account, CancellationToken cancellationToken)
    {

    }
}
