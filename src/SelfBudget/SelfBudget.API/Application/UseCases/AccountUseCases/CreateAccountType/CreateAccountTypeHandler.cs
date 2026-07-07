using CSharpFunctionalExtensions;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Common;
using SelfBudget.API.Domain.Entities.AccountContext;

namespace SelfBudget.API.Application.UseCases.AccountUseCases.CreateAccountType;

public class CreateAccountTypeHandler
{
    private readonly IAccountTypeRepository _repository;
    private readonly ITransactionManager _transactionManager;

    public CreateAccountTypeHandler(
        IAccountTypeRepository repository,
        ITransactionManager transactionManager)
    {
        _repository = repository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateAccountTypeCommand command,
        CancellationToken cancellationToken)
    {
        var accountType = new AccountType(
            command.Name,
            command.Description);

        var result = await _repository.CreateAccountTypeAsync(accountType, cancellationToken);
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        return result;
    }
}
