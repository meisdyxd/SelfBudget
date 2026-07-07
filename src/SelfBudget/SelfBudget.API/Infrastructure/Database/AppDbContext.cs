using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Domain.Entities.AccountContext;
using SelfBudget.API.Domain.Entities.TransactionContext;
using SelfBudget.API.Domain.Entities.UserContext;

namespace SelfBudget.API.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<AccountType> AccountTypes { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<TransactionCategory> TransactionCategories { get; set; } = null!;
    public DbSet<TransactionCategoryType> TransactionCategoryTypes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
