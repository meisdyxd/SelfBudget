namespace SelfBudget.API.Application.UseCases.AccountUseCases.GetAccountById;

public class GetAccountByIdQuery
{
    public Guid Id { get; set; }

    public GetAccountByIdQuery(Guid id)
    {
        Id = id;
    }
}
