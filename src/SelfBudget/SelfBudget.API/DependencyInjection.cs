using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.Services;
using SelfBudget.API.Infrastructure.Database;
using SelfBudget.API.Infrastructure.Repositories.AccountRepositories;
using SelfBudget.API.Infrastructure.Repositories.TransactionRepositories;
using SelfBudget.API.Infrastructure.Repositories.UserRepositories;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace SelfBudget.API;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionCategoryRepository, TransactionCategoryRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<DbSeeder>();

        return services;
    }
    
    public static IHostBuilder ConfigureWolverine(
        this IHostBuilder host,
        IConfiguration configuration)
    {
        var sectionName = "Database";
        var connectionString = configuration.GetConnectionString(sectionName)
            ?? throw new ArgumentNullException(sectionName, "Connection string is null");
        
        host.UseWolverine(opts =>
        {
            opts.UseRuntimeCompilation();
            opts.UsePostgresqlPersistenceAndTransport(connectionString);
        });

        return host;
    }
}
