using SelfBudget.API.Abstractions;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Entities;

namespace SelfBudget.API.UseCases.CreateAccount;

public class CreateAccountHandler
{
    private readonly IAccountRepository _repository;
    private readonly ITransactionManager _transactionManager;

    public CreateAccountHandler(
        IAccountRepository repository,
        ITransactionManager transactionManager)
    {
        _repository = repository;
        _transactionManager = transactionManager;
    }

    public async Task<Guid> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var account = new Account(
            command.UserId,
            command.Name,
            command.CurrencyCode,
            command.AccountTypeId);

        var result = await _repository.CreateAccountAsync(account, cancellationToken);
        await _transactionManager.SaveChangesAsync(cancellationToken);
        return result;
    }
}