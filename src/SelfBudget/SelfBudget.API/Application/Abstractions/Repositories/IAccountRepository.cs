using SelfBudget.API.Common.Dtos.AccountDtos;
using SelfBudget.API.Domain.Entities.AccountContext;

namespace SelfBudget.API.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<AccountDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Guid> CreateAccountAsync(Account account, CancellationToken cancellationToken);
    Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<AccountDto>> GetAccountsByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateAsync(AccountDto account, CancellationToken cancellationToken);
}
