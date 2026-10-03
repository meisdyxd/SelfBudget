namespace SelfBudget.API.Application.UseCases.UserUseCases.GetUserById;

public class GetUserByIdQuery
{
    public Guid Id { get; set; }

    public GetUserByIdQuery(Guid id)
    {
        Id = id;
    }
}
