using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Abstractions;
using SelfBudget.API.Database;
using SelfBudget.API.Dtos;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        AppDbContext dbContext,
        ILogger<UserRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Создан пользователь с идентификатором: '{UserId}'", user.Id);

        return user.Id;
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        await _dbContext.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Удалён пользователь с идентификатором: '{UserId}'", id);
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
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Получены счета у пользователся с идентификатором: '{UserId}'", userId);
        return accounts;
    }

    public async Task<ICollection<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Birthdate = u.Birthdate,
                Email = u.Email,
                Name = u.Name,
                PhotoId = u.PhotoId
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Получены пользователи");
        return users;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Birthdate = u.Birthdate,
                Email = u.Email,
                Name = u.Name,
                PhotoId = u.PhotoId
            })
            .SingleOrDefaultAsync(cancellationToken);

        _logger.LogInformation("Получен пользователь с идентификатором: '{UserId}'", id);
        return user;
    }
}
