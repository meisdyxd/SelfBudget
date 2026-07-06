using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Entities;

namespace SelfBudget.API.UseCases.CreateAccountType;

public class CreateAccountTypeHandler
{
    private readonly IAccountTypeRepository _repository;

    public CreateAccountTypeHandler(IAccountTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handler(
        CreateAccountTypeCommand command,
        CancellationToken cancellationToken)
    {
        var accountType = new AccountType(
            command.Name,
            command.Description);

        var result = await _repository.CreateAccountTypeAsync(accountType, cancellationToken);

        return result;
    }
}
