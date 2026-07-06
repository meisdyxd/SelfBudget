namespace SelfBudget.API.Dtos.Requests;

public class CreateAccountRequest
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public Guid AccountTypeId { get; set; }
}
