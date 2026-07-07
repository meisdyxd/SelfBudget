namespace SelfBudget.API.Dtos.Requests.AccountRequests;

public class CreateAccountTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
