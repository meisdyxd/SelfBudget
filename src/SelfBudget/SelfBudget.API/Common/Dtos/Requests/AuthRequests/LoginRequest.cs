namespace SelfBudget.API.Common.Dtos.Requests.AuthRequests;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
