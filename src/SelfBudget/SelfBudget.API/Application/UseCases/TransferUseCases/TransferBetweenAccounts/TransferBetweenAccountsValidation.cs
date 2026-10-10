using FluentValidation;

namespace SelfBudget.API.Application.UseCases.TransferUseCases.TransferBetweenAccounts;

public class TransferBetweenAccountsValidation : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsValidation()
    {
        RuleFor(c => c.Amount)
            .GreaterThan(1)
            .WithMessage("Сумма должна быть больше 1")
            .PrecisionScale(18, 2, true)
            .WithMessage("Сумма должна иметь не более 2 знаков после запятой");
    }
}