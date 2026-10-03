using FluentValidation;

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
            .Matches("^[А-Яа-яA-Za-z]$")
            .WithMessage("Имя должно состоять только из символов кириллического или латинского алфавита");

        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("Почта не должна быть пустой")
            .MaximumLength(1024)
            .WithMessage("Максимальная длина почты 1024 символа")
            .MinimumLength(5)
            .WithMessage("Минимальная длина почты 5 символов")
            .Matches("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")
            .WithMessage("Почта не соответствует формату");

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Почта не должна быть пустой")
            .MinimumLength(12)
            .WithMessage("Минимальная длина пароля 12 символов")
            .MaximumLength(256)
            .WithMessage("Максимальная длина пароля 256 символов");

        RuleFor(c => c.Birthdate)
            .NotEmpty()
            .WithMessage("Дата рождения не может быть пустой")
            .LessThan(DateTime.UtcNow)
            .WithMessage("Дата рождения не может быть больше текущей");
    }
}
