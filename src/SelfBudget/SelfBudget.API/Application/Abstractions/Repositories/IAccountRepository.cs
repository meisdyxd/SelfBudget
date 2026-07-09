using SelfBudget.API.Domain.Entities.AccountContext;

namespace SelfBudget.API.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<Account>> GetAllAsync(CancellationToken cancellationToken);
    Task<Guid> CreateAccountAsync(Account account, CancellationToken cancellationToken);
    Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<Account?>> GetAccountsByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateAsync(Account account, CancellationToken cancellationToken);
}
