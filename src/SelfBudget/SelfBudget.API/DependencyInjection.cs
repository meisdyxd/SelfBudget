using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.Services;
using SelfBudget.API.Application.UseCases.AuthUseCases.Register;
using SelfBudget.API.Infrastructure.Database;
using SelfBudget.API.Infrastructure.InMemoryStorage;
using SelfBudget.API.Infrastructure.Repositories.AccountRepositories;
using SelfBudget.API.Infrastructure.Repositories.TransactionRepositories;
using SelfBudget.API.Infrastructure.Repositories.UserRepositories;
using System.Text;
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
        services.AddSingleton<ITokenStorage, InMemoryTokenStorage>();

        return services;
    }

    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterValidation>();
        services.AddScoped<ITokenProvider, JwtTokenProvider>();

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

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "SelfBudget.Auth",
                    ValidAudience = "SelfBudget.Backend",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("auth-security-key"))
                };
            });

        return services;
    }
}
