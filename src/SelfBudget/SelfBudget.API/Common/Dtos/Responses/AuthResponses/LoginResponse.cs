using SelfBudget.API.Common.Dtos.UserDtos;

namespace SelfBudget.API.Common.Dtos.Responses.AuthResponses;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public LoginResponseUserDto User { get; set; } = null!;
}

public record LoginResponseUserDto(Guid Id, string Name, string Email);