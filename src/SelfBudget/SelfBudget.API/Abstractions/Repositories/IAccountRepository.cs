using SelfBudget.API.Dtos.AccountDtos;
using SelfBudget.API.Entities.AccountContext;

namespace SelfBudget.API.Abstractions.Repositories;

public interface IAccountRepository
{
    Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<AccountDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Guid> CreateAccountAsync(Account account, CancellationToken cancellationToken);
    Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<AccountDto>> GetAccountsByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateAsync(AccountDto account, CancellationToken cancellationToken);
}
