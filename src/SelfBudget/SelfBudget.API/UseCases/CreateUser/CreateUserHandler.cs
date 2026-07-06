using SelfBudget.API.Abstractions;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Entities;
using SelfBudget.API.Services;

namespace SelfBudget.API.UseCases.CreateUser;

public class CreateUserHandler
{
    private readonly IUserRepository _repository;
    private readonly ITransactionManager _transactionManager;

    public CreateUserHandler(
        IUserRepository repository,
        ITransactionManager transactionManager)
    {
        _repository = repository;
        _transactionManager = transactionManager;
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
        await _transactionManager.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
