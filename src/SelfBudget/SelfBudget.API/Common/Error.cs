namespace SelfBudget.API.Common;

public sealed record Error
{
    public Error(string message, string? code = null)
    {
        Message = message;
        Code = code;
    }
    public string Message { get; set; }
    public string? Code { get; set; }
}
