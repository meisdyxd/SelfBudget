using SelfBudget.API.Common.Dtos.Requests.UserRequests;

namespace SelfBudget.API.Application.UseCases.UserUseCases.DeleteUser;

public class DeleteUserCommand
{
    public Guid Id { get; set; }

    public static DeleteUserCommand FromRequest(DeleteUserRequest request)
    {
        return new DeleteUserCommand
        {
            Id = request.Id
        };
    }
}
