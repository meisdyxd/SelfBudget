using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Database;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Services;

public class DbSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        AppDbContext dbContext,
        ILogger<DbSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("START: seed data");
        await SeedAccountTypesAsync();
        await SeedUsersAsync();
        await SeedAccountsAsync();
        _logger.LogInformation("STOP: seed data");
    }

    private async Task SeedAccountTypesAsync()
    {
        if (await _dbContext.AccountTypes.AnyAsync())
        {
            return;
        }

        await _dbContext.AccountTypes.AddRangeAsync(
            new AccountType("Debit", "Дебетовый счет"),
            new AccountType("Credit", "Кредитный счет"),
            new AccountType("Savings", "Накопительный счет")
        );
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        if (await _dbContext.Users.AnyAsync())
        {
            return;
        }

        var user = new User("Admin", "admin@admin.ru", "admin123", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var user2 = new User("meisdy", "kararturkar@gmail.com", "meisdy", new DateTime(2005, 2, 1, 20, 35, 0, DateTimeKind.Utc));
        await _dbContext.Users.AddRangeAsync(user, user2);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedAccountsAsync()
    {
        if (await _dbContext.Accounts.AnyAsync())
        {
            return;
        }
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == "Admin");
        var accountType = await _dbContext.AccountTypes.FirstOrDefaultAsync();
        var user2 = await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == "meisdy");

        if (user != null && accountType != null && user2 != null)
        {
            var account = new Account(user.Id, "Веселый счет", "RUB", accountType.Id);
            var account2 = new Account(user2.Id, "Веселый счет", "RUB", accountType.Id);
            await _dbContext.Accounts.AddRangeAsync(account, account2);
            await _dbContext.SaveChangesAsync();
        }
    }
}
