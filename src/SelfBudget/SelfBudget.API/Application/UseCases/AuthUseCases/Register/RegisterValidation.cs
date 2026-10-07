using FluentValidation;
using SelfBudget.API.Common.CustomValidators;
using SelfBudget.API.Domain.ValueObjects;

namespace SelfBudget.API.Application.UseCases.AuthUseCases.Register;

public class RegisterValidation : AbstractValidator<RegisterCommand>
{
    public RegisterValidation()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Имя не может быть пустым")
            .MaximumLength(255)
            .WithMessage("Максимальная длина имени 255 символов")
            .Matches(@"^[А-Яа-яA-Za-zЁё\s-]{1,255}$")
            .WithMessage("Имя должно состоять только из символов кириллического или латинского алфавита, а также пробела и дефиса");

        RuleFor(c => c.Email)
            .ValidateValueObject(EmailValueObject.Create);

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Пароль не должен быть пустым")
            .MinimumLength(12)
            .WithMessage("Минимальная длина пароля 12 символов")
            .MaximumLength(256)
            .WithMessage("Максимальная длина пароля 256 символов");

        RuleFor(c => c.Birthdate)
            .NotEmpty()
            .WithMessage("Дата рождения не может быть пустой")
            .Must(b => b <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Дата рождения не может быть больше текущей");
    }
}
