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
using System.Security.Claims;

namespace SelfBudget.API.Application.UseCases.AuthUseCases.Login;

public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<LoginCommand> _validator;
    private readonly ITokenStorage _tokenStorage;
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository userRepository,
        ITransactionManager transactionManager,
        IValidator<LoginCommand> validator,
        ITokenStorage tokenStorage,
        ITokenProvider tokenProvider,
        ILogger<LoginHandler> logger)
    {
        _userRepository = userRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _tokenStorage = tokenStorage;
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public async Task<Result<LoginResponse, Error>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var resultValidation = await _validator.ValidateAsync(command, cancellationToken);
        if (!resultValidation.IsValid)
        {
            return resultValidation.ToError("login");
        }

        var email = EmailValueObject.Create(command.Email).Value;
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return new Error("Неверный пароль или почта для входа", "error.login.fail");
        }
        var passwordHasher = new PasswordHasher<User>();
        var resultValidationPassword = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        switch (resultValidationPassword)
        {
            case PasswordVerificationResult.Failed:
                return new Error("Неверный пароль или почта для входа", "error.login.fail");
            case PasswordVerificationResult.SuccessRehashNeeded:
                var newHashedPassword = passwordHasher.HashPassword(user, command.Password);
                user.SetHashPassword(newHashedPassword);
                var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    return new Error("Ошибка входа в аккаунт, попробуйте снова", "error.login.fail");
                }
                break;
        }

        Claim[] claims = [new Claim("sub", user.Id.ToString())];
        var accessAndRefresh = _tokenProvider.GenerateAccessAndRefreshToken(claims);

        await _tokenStorage.SetAccessAndRefreshTokenByUserId(user.Id, accessAndRefresh);

        return await Task.FromResult<LoginResponse>(new()
        {
            AccessToken = accessAndRefresh.AccessToken,
            ExpiresAt = accessAndRefresh.AccessExpiresAt,
            User = new(user.Id, user.Name, user.Email.Value)
        });
    }
}
