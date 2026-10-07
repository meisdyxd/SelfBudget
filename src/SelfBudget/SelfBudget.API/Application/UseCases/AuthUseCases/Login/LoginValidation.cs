using FluentValidation;
using SelfBudget.API.Common.CustomValidators;
using SelfBudget.API.Domain.ValueObjects;

namespace SelfBudget.API.Application.UseCases.AuthUseCases.Login;

public class LoginValidation : AbstractValidator<LoginCommand>
{
    public LoginValidation()
    {
        RuleFor(l => l.Email)
            .ValidateValueObject(EmailValueObject.Create);

        RuleFor(l => l.Password)
            .NotEmpty()
            .WithMessage("Пароль не может быть пустым");
    }
}
