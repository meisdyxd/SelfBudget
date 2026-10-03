using CSharpFunctionalExtensions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.AccountDtos;

namespace SelfBudget.API.Application.UseCases.AccountUseCases.GetAccountById;

public class GetAccountByIdHandler
{
    private readonly IAccountRepository _repository;

    public GetAccountByIdHandler(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AccountDto, Error>> Handle(GetAccountByIdQuery query, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(query.Id, cancellationToken);

        if (account is null)
            return new Error($"Не найден счёт с ID: {query.Id}", "error.account.notfound");

        return new AccountDto
        {
            Id = account.Id,
            Balance = account.Balance,
            CurrencyCode = account.CurrencyCode,
            Name = account.Name,
            OverdraftLimit = account.OverdraftLimit,
            Type = account.Type.Description ?? "Неизвестный",
            TypeId = account.TypeId,
            UserId = account.UserId,
        };
    }
}
