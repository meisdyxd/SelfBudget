using SelfBudget.API.Abstractions;
using SelfBudget.API.Enums;

namespace SelfBudget.API.Entities;

/// <summary>
/// Счет пользователя
/// </summary>
public class Account : AuditableEntity, IBaseEntity<Guid>
{
    public Account(
        Guid userId, 
        string name, 
        string currencyCode,
        AccountType type)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        CurrencyCode = currencyCode;
        Balance = 0m;
        Type = type;
        OverdraftLimit = 0m;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Название счета
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Код валюты
    /// </summary>
    public string CurrencyCode { get; set; }

    /// <summary>
    /// Баланс счета
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Тип счета
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Овердрафт лимит
    /// </summary>
    public decimal OverdraftLimit { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public virtual User User { get; set; }
}
