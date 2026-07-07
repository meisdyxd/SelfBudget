using SelfBudget.API.Dtos.Requests.AccountRequests;

namespace SelfBudget.API.UseCases.AccountUseCases.CreateAccount;

public class CreateAccountCommand
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public Guid AccountTypeId { get; set; }

    public static CreateAccountCommand FromRequest(CreateAccountRequest request)
    {
        return new()
        {
            UserId = request.UserId,
            Name = request.Name,
            CurrencyCode = request.CurrencyCode,
            AccountTypeId = request.AccountTypeId
        };
    }
}
