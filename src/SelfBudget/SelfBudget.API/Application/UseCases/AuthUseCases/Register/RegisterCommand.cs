using SelfBudget.API.Common.Dtos.Requests.AuthRequests;

namespace SelfBudget.API.Application.UseCases.AuthUseCases.Register;

public class RegisterCommand
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateOnly Birthdate { get; set; }

    public static RegisterCommand FromRequest(RegisterRequest request)
    {
        return new RegisterCommand
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            Birthdate = request.Birthdate
        };
    }
}
