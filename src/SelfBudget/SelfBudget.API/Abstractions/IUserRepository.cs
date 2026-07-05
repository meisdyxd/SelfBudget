using SelfBudget.API.Dtos;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Abstractions;

public interface IUserRepository
{
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<UserDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Guid> CreateUserAsync(User user, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<AccountDto>> GetAccountsByUserAsync(Guid userId, CancellationToken cancellationToken);
}
