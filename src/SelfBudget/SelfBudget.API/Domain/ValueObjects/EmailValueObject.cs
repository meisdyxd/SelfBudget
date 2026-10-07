using CSharpFunctionalExtensions;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Constants;

namespace SelfBudget.API.Domain.ValueObjects;

public sealed class EmailValueObject : ValueObject
{
    private EmailValueObject(string value)
    {
        Value = value;
    }

    public string Value { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        return [Value];
    }

    public static Result<EmailValueObject, Error> Create(string value)
    {
        bool ValidateLength(string value) => value.Length is >= EmailConstants.MIN_LENGTH and <= EmailConstants.MAX_LENGTH;
        bool ValidateRegex(string value) => EmailConstants.Regex.IsMatch(value);

        value = value
            .Trim()
            .ToLower(culture: System.Globalization.CultureInfo.InvariantCulture);

        if (!ValidateLength(value))
            return new Error("Длина почты должна соответствовать от 5 до 255 символов включительно", "error.email.validation");
        if (!ValidateRegex(value))
            return new Error("Почта не соответствуте формату name@domain.name", "error.email.validation");

        return new EmailValueObject(value);
    }
}
