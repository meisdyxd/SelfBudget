using CSharpFunctionalExtensions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.AccountDtos;

namespace SelfBudget.API.Application.UseCases.AccountUseCases.GetAccountById;

public class GetAccountsHandler
{
    private readonly IAccountRepository _repository;

    public GetAccountsHandler(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ICollection<AccountDto>, Error>> Handle(GetAccountsQuery query, CancellationToken cancellationToken)
    {
        var accounts = await _repository.GetAllAsync(cancellationToken);

        return accounts.Select(account => new AccountDto
        {
            Id = account.Id,
            Balance = account.Balance,
            CurrencyCode = account.CurrencyCode,
            Name = account.Name,
            OverdraftLimit = account.OverdraftLimit,
            Type = account.Type.Description ?? "Неизвестный",
            TypeId = account.TypeId,
            UserId = account.UserId,
        }).ToArray();
    }
}
