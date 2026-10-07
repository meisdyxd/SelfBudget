using SelfBudget.API.Common.Dtos.Requests.AuthRequests;

namespace SelfBudget.API.Application.UseCases.AuthUseCases.Login;

public class LoginCommand
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public static LoginCommand FromRequest(LoginRequest request) => new() { Email = request.Email, Password = request.Password };
}
