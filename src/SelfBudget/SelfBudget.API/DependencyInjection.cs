using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Abstractions;
using SelfBudget.API.Database;
using SelfBudget.API.Repositories;
using Wolverine.EntityFrameworkCore;

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

        return services;
    }
}
