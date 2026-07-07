using SelfBudget.API.Dtos.Requests.UserRequests;

namespace SelfBudget.API.UseCases.UserUseCases.CreateUser;

public class CreateUserCommand
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime Birthdate { get; set; }

    public static CreateUserCommand FromRequest(CreateUserRequest request)
    {
        return new CreateUserCommand
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            Birthdate = request.Birthdate
        };
    }
}
