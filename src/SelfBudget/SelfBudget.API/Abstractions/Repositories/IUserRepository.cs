using SelfBudget.API.Dtos.UserDtos;
using SelfBudget.API.Entities.UserContext;

namespace SelfBudget.API.Abstractions.Repositories;

public interface IUserRepository
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<UserDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Guid> CreateUserAsync(User user, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken);
}
