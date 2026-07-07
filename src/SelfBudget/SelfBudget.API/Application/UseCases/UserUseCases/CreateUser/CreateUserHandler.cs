using CSharpFunctionalExtensions;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.Services;
using SelfBudget.API.Common;
using SelfBudget.API.Domain.Entities.UserContext;

namespace SelfBudget.API.Application.UseCases.UserUseCases.CreateUser;

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

    public async Task<Result<Guid, Error>> Handle(
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
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (!saveResult.IsSuccess)
        {
            return saveResult.Error;
        }

        return user.Id;
    }
}
