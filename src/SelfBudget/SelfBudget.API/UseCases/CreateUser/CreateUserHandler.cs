using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Entities;
using SelfBudget.API.Services;

namespace SelfBudget.API.UseCases.CreateUser;

public class CreateUserHandler
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var hashPassword = PasswordProvider.HashPassword(command.Password);
        var user = new User(
            command.Name,
            command.Email,
            hashPassword,
            command.Birthdate);

        await _repository.CreateUserAsync(user, cancellationToken);

        return user.Id;
    }
}
