using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.Extensions;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.Responses.AuthResponses;
using SelfBudget.API.Domain.Entities.UserContext;
using SelfBudget.API.Domain.ValueObjects;

namespace SelfBudget.API.Application.UseCases.AuthUseCases.Register;

public class RegisterHandler
{
    private readonly IValidator<RegisterCommand> _validator;
    private readonly IUserRepository _repository;
    private readonly ITransactionManager _transactionManager;

    public RegisterHandler(
        IValidator<RegisterCommand> validator,
        IUserRepository repository,
        ITransactionManager transactionManager)
    {
        _validator = validator;
        _repository = repository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<RegisterResponse, Error>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        //validation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToError("register");

        var emailResult = EmailValueObject.Create(command.Email);
        if (emailResult.IsFailure)
            return emailResult.Error;

        var exists = await _repository.IsExistsByEmail(emailResult.Value, cancellationToken);
        if (exists)
            return new Error("Уже существует пользователь с указанным email", "error.register.conflict");

        var user = new User(
            command.Name,
            emailResult.Value,
            command.Birthdate);

        var passwordHasher = new PasswordHasher<User>();
        var hashPassword = passwordHasher.HashPassword(user, command.Password);

        user.SetHashPassword(hashPassword);

        await _repository.CreateUserAsync(user, cancellationToken);
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (!saveResult.IsSuccess)
        {
            if (saveResult.Error.Code == "persistence.user.email_duplicate")
                return new Error("Уже существует пользователь с указанным email", "error.register.conflict");

            return saveResult.Error;
        }

        return new RegisterResponse(user.Id, user.Name, user.Email.Value);
    }
}
