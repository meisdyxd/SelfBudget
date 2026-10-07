using Microsoft.EntityFrameworkCore;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Common.Dtos.UserDtos;
using SelfBudget.API.Domain.Entities.UserContext;
using SelfBudget.API.Domain.ValueObjects;
using SelfBudget.API.Infrastructure.Database;

namespace SelfBudget.API.Infrastructure.Repositories.UserRepositories;

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
        _logger.LogInformation("Создан пользователь с идентификатором: '{UserId}'", user.Id);

        return user.Id;
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        await _dbContext.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Удалён пользователь с идентификатором: '{UserId}'", id);
    }

    public async Task<ICollection<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Birthdate = u.Birthdate,
                Email = u.Email.Value,
                Name = u.Name,
                PhotoId = u.PhotoId
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Получены пользователи");
        return users;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Birthdate = u.Birthdate,
                Email = u.Email.Value,
                Name = u.Name,
                PhotoId = u.PhotoId
            })
            .SingleOrDefaultAsync(cancellationToken);

        _logger.LogInformation("Получен пользователь с идентификатором: '{UserId}'", id);
        return user;
    }

    public async Task<bool> IsExistsByEmail(EmailValueObject email, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Проверка существования пользователя по почте: {Email}", email.Value);
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user is not null;
    }

    public async Task<User?> GetByEmailAsync(EmailValueObject email, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получение пользователя по почте: {Email}", email.Value);
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
