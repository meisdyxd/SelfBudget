using CSharpFunctionalExtensions;
using FluentValidation;

namespace SelfBudget.API.Common.CustomValidators;

public static class CustomFluentValidators
{
    public static IRuleBuilderOptionsConditions<T, TElement> ValidateValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder, 
        Func<TElement, Result<TValueObject, Error>> factory)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Error> result = factory(value);
            if (result.IsFailure)
            {
                var error = result.Error;
                context.AddFailure(new FluentValidation.Results.ValidationFailure
                { 
                    ErrorCode = error.Code, 
                    ErrorMessage = error.Message, 
                    PropertyName = context.PropertyPath
                });
            }
        });
    }
}