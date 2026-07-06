using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Entities;

namespace SelfBudget.API.UseCases.CreateAccount;

public class CreateAccountHandler
{
    private readonly IAccountRepository _repository;

    public CreateAccountHandler(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handler(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var account = new Account(
            command.UserId,
            command.Name,
            command.CurrencyCode,
            command.AccountTypeId);

        var result = await _repository.CreateAccountAsync(account, cancellationToken);
        return result;
    }
}