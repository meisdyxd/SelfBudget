using CSharpFunctionalExtensions;
using SelfBudget.API.Abstractions;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Common;
using SelfBudget.API.Entities;

namespace SelfBudget.API.UseCases.AccountUseCases.CreateAccount;

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

    public async Task<Result<Guid, Error>> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var account = new Account(
            command.UserId,
            command.Name,
            command.CurrencyCode,
            command.AccountTypeId);

        var result = await _repository.CreateAccountAsync(account, cancellationToken);
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        return result;
    }
}