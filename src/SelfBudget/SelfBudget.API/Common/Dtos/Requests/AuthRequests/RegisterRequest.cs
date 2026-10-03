namespace SelfBudget.API.Common.Dtos.Requests.AuthRequests;

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime Birthdate { get; set; }
}
