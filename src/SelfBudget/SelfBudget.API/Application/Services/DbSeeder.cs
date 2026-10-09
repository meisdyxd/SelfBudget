using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Domain.Entities.AccountContext;
using SelfBudget.API.Domain.Entities.TransactionContext;
using SelfBudget.API.Domain.Entities.UserContext;
using SelfBudget.API.Domain.ValueObjects;
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
        var passwordHasher = new PasswordHasher<User>();
        var user = new User("Admin", EmailValueObject.Create("admin@admin.ru").Value, new DateOnly(1990, 1, 1));
        var password = passwordHasher.HashPassword(user, "admin123");
        user.SetHashPassword(password);

        var user2 = new User("meisdy", EmailValueObject.Create("kararturkar@gmail.com").Value, new DateOnly(2005, 2, 1));
        var password2 = passwordHasher.HashPassword(user2, "01022005");
        user2.SetHashPassword(password2);

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

        var food = new TransactionCategory(expenseType.Id, "Еда", TransactionCategoriesCodes.Food, null);
        var transport = new TransactionCategory(expenseType.Id, "Транспорт", TransactionCategoriesCodes.Transport, null);
        var shopping = new TransactionCategory(expenseType.Id, "Покупки", TransactionCategoriesCodes.Shopping, null);
        var entertainment = new TransactionCategory(expenseType.Id, "Развлечения", TransactionCategoriesCodes.Entertainment, null);
        var utilities = new TransactionCategory(expenseType.Id, "Коммунальные услуги", TransactionCategoriesCodes.Utilities, null);
        var salary = new TransactionCategory(incomeType.Id, "Зарплата", TransactionCategoriesCodes.Salary, null);
        var freelance = new TransactionCategory(incomeType.Id, "Фриланс", TransactionCategoriesCodes.Freelance, null);
        var investments = new TransactionCategory(incomeType.Id, "Инвестиции", TransactionCategoriesCodes.Ivestments, null);
        var transfer = new TransactionCategory(transferType.Id, "Перевод между счетами", TransactionCategoriesCodes.Transfer, null);
        var selfTransfer = new TransactionCategory(transferType.Id, "Перевод между своими счетами", TransactionCategoriesCodes.SelfTransfer, null);

        await _dbContext.TransactionCategories.AddRangeAsync(
            food, transport, shopping, entertainment, utilities,
            salary, freelance, investments, transfer, selfTransfer);
        await _dbContext.SaveChangesAsync();

        var subCategories = new[]
        {
            // еда
            new TransactionCategory(expenseType.Id, "Рестораны", TransactionCategoriesCodes.Restaurants, food.Id),
            new TransactionCategory(expenseType.Id, "Кафе", TransactionCategoriesCodes.Cafes, food.Id),
            new TransactionCategory(expenseType.Id, "Доставка", TransactionCategoriesCodes.Delivery, food.Id),
            new TransactionCategory(expenseType.Id, "Продукты в магазине", TransactionCategoriesCodes.GroceryShopping, food.Id),
            // транспорт
            new TransactionCategory(expenseType.Id, "Общественный транспорт", TransactionCategoriesCodes.PublicTransport, transport.Id),
            new TransactionCategory(expenseType.Id, "Такси", TransactionCategoriesCodes.Taxis, transport.Id),
            new TransactionCategory(expenseType.Id, "Личный автомобиль (топливо)", TransactionCategoriesCodes.PersonalCarFuel, transport.Id),
            new TransactionCategory(expenseType.Id, "Личный автомобиль (ремонт)", TransactionCategoriesCodes.PersonalCarRepairs, transport.Id),
            // покупки
            new TransactionCategory(expenseType.Id, "Одежда", TransactionCategoriesCodes.Clothing, shopping.Id),
            new TransactionCategory(expenseType.Id, "Техника", TransactionCategoriesCodes.Appliances, shopping.Id),
            // развлечения
            new TransactionCategory(expenseType.Id, "Подписки", TransactionCategoriesCodes.Subscriptions, entertainment.Id),
            new TransactionCategory(expenseType.Id, "Концерты", TransactionCategoriesCodes.Concerts, entertainment.Id),
            new TransactionCategory(expenseType.Id, "Кино", TransactionCategoriesCodes.Movies, entertainment.Id),
            // зарплата
            new TransactionCategory(incomeType.Id, "Основная зарплата", TransactionCategoriesCodes.BasicSalary, salary.Id),
            new TransactionCategory(incomeType.Id, "Премия", TransactionCategoriesCodes.Bonus, salary.Id),
        };

        await _dbContext.TransactionCategories.AddRangeAsync(subCategories);
        await _dbContext.SaveChangesAsync();
    }
}
