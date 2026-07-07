using SelfBudget.API.Common.Dtos.AccountDtos;
using SelfBudget.API.Domain.Entities.AccountContext;

namespace SelfBudget.API.Application.Abstractions.Repositories;

public interface IAccountTypeRepository
{
    Task<AccountTypeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ICollection<AccountTypeDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Guid> CreateAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken);
    Task DeleteAccountTypeAsync(Guid id, CancellationToken cancellationToken);
}
