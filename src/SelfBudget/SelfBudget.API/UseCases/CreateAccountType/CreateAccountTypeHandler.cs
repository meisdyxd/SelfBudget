using SelfBudget.API.Abstractions;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Entities;

namespace SelfBudget.API.UseCases.CreateAccountType;

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

    public async Task<Guid> Handle(
        CreateAccountTypeCommand command,
        CancellationToken cancellationToken)
    {
        var accountType = new AccountType(
            command.Name,
            command.Description);

        var result = await _repository.CreateAccountTypeAsync(accountType, cancellationToken);
        await _transactionManager.SaveChangesAsync(cancellationToken);
        return result;
    }
}
