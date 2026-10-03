using CSharpFunctionalExtensions;
using SelfBudget.API.Common;

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
        throw new NotImplementedException();
    }

    public static Result<EmailValueObject, Error> Create(string value)
    {
        if (value.Length < 5 
            || value.Length > 1024 
            || !value.Contains('@') 
            || value.Split('@').Length != 2 
            || value.Split('@')[1].Length < 3)
            return new Error("Ошибка валидации почты", "error.email.validation");

        return new EmailValueObject(value);
    }
}
