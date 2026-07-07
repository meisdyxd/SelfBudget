using CSharpFunctionalExtensions;
using SelfBudget.API.Abstractions;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Common;

namespace SelfBudget.API.UseCases.UserUseCases.DeleteUser;

public class DeleteUserHandler
{
    private readonly ILogger<DeleteUserHandler> _logger;
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(
        ILogger<DeleteUserHandler> logger,
        IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<Result<bool, Error>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _userRepository.DeleteUserAsync(userId, cancellationToken);
        
        return true;
    }
}
