using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Domain.Entities.AccountContext;
using SelfBudget.API.Domain.Entities.TransactionContext;
using SelfBudget.API.Domain.Entities.UserContext;
using SelfBudget.API.Infrastructure.Database;

namespace SelfBudget.API.Application.Services;

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
        await SeedTransactionCategoryTypes();
        await SeedTransactionCategories();
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

    private async Task SeedTransactionCategoryTypes()
    {
        if (await _dbContext.TransactionCategoryTypes.AnyAsync())
            return;

        var types = new[]
        {
        new TransactionCategoryType("Transfer"),
        new TransactionCategoryType("Expense"),
        new TransactionCategoryType("Income")
    };

        await _dbContext.TransactionCategoryTypes.AddRangeAsync(types);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedTransactionCategories()
    {
        if (await _dbContext.TransactionCategories.AnyAsync())
            return;

        var transferType = await _dbContext.TransactionCategoryTypes.FirstAsync(t => t.Name == "Transfer");
        var expenseType = await _dbContext.TransactionCategoryTypes.FirstAsync(t => t.Name == "Expense");
        var incomeType = await _dbContext.TransactionCategoryTypes.FirstAsync(t => t.Name == "Income");

        var food = new TransactionCategory(expenseType.Id, "Еда", null);
        var transport = new TransactionCategory(expenseType.Id, "Транспорт", null);
        var shopping = new TransactionCategory(expenseType.Id, "Покупки", null);
        var entertainment = new TransactionCategory(expenseType.Id, "Развлечения", null);
        var utilities = new TransactionCategory(expenseType.Id, "Коммунальные услуги", null);
        var salary = new TransactionCategory(incomeType.Id, "Зарплата", null);
        var freelance = new TransactionCategory(incomeType.Id, "Фриланс", null);
        var investments = new TransactionCategory(incomeType.Id, "Инвестиции", null);
        var transfer = new TransactionCategory(transferType.Id, "Перевод между счетами", null);

        await _dbContext.TransactionCategories.AddRangeAsync(
            food, transport, shopping, entertainment, utilities,
            salary, freelance, investments, transfer);
        await _dbContext.SaveChangesAsync();

        var subCategories = new[]
        {
            // еда
            new TransactionCategory(expenseType.Id, "Рестораны", food.Id),
            new TransactionCategory(expenseType.Id, "Кафе", food.Id),
            new TransactionCategory(expenseType.Id, "Доставка", food.Id),
            new TransactionCategory(expenseType.Id, "Продукты в магазине", food.Id),
            // транспорт
            new TransactionCategory(expenseType.Id, "Общественный транспорт", transport.Id),
            new TransactionCategory(expenseType.Id, "Такси", transport.Id),
            new TransactionCategory(expenseType.Id, "Личный автомобиль (топливо)", transport.Id),
            new TransactionCategory(expenseType.Id, "Личный автомобиль (ремонт)", transport.Id),
            // покупки
            new TransactionCategory(expenseType.Id, "Одежда", shopping.Id),
            new TransactionCategory(expenseType.Id, "Обувь", shopping.Id),
            new TransactionCategory(expenseType.Id, "Техника", shopping.Id),
            // развлечения
            new TransactionCategory(expenseType.Id, "Подписки", entertainment.Id),
            new TransactionCategory(expenseType.Id, "Концерты", entertainment.Id),
            new TransactionCategory(expenseType.Id, "Кино", entertainment.Id),
            // зарплата
            new TransactionCategory(incomeType.Id, "Основная зарплата", salary.Id),
            new TransactionCategory(incomeType.Id, "Премия", salary.Id),
        };

        await _dbContext.TransactionCategories.AddRangeAsync(subCategories);
        await _dbContext.SaveChangesAsync();
    }
}
