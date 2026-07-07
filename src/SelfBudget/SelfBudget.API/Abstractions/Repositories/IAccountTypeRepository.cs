using SelfBudget.API.Dtos.AccountDtos;
using SelfBudget.API.Entities.AccountContext;

namespace SelfBudget.API.Abstractions.Repositories;

public interface IAccountTypeRepository
{
    Task<AccountTypeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<AccountTypeDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Guid> CreateAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken);
    Task DeleteAccountTypeAsync(Guid id, CancellationToken cancellationToken);
}
