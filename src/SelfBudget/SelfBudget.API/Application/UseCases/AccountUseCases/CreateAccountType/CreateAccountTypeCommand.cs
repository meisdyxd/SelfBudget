using SelfBudget.API.Common.Dtos.Requests.AccountRequests;

namespace SelfBudget.API.Application.UseCases.AccountUseCases.CreateAccountType;

public class CreateAccountTypeCommand
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public static CreateAccountTypeCommand FromRequest(CreateAccountTypeRequest request)
    {
        return new()
        {
            Name = request.Name,
            Description = request.Description
        };
    }
}
