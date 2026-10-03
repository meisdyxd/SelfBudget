using FluentValidation.Results;
using SelfBudget.API.Common;

namespace SelfBudget.API.Application.Extensions;

public static class ValidationResultExtensions
{
    public static Error ToError(this ValidationResult validationResult, string contextName)
    {
        var firstError = validationResult.Errors.First();

        return new Error(firstError.ErrorMessage, $"error.{contextName}.validation")
    }
}
