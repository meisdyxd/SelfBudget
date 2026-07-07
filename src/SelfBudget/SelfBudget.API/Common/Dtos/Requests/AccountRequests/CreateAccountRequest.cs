namespace SelfBudget.API.Common.Dtos.Requests.AccountRequests;

public class CreateAccountRequest
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public Guid AccountTypeId { get; set; }
}
