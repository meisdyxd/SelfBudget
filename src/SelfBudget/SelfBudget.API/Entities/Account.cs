using SelfBudget.API.Abstractions;

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
        Guid accountTypeId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        CurrencyCode = currencyCode;
        TypeId = accountTypeId;
        Balance = 0m;
        OverdraftLimit = 0m;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }


    /// <summary>
    /// Идентификатор типа счета
    /// </summary>
    public Guid TypeId { get; set; }

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
    /// Овердрафт лимит
    /// </summary>
    public decimal OverdraftLimit { get; set; }

    /// <summary>
    /// Тип счета
    /// </summary>
    public virtual AccountType Type { get; set; } = null!;

    /// <summary>
    /// Пользователь
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Исходящие транзакции
    /// </summary>
    public virtual ICollection<Transaction> OutTransactions { get; set; } = [];

    /// <summary>
    /// Входящие транзакции
    /// </summary>
    public virtual ICollection<Transaction> InTransactions { get; set; } = [];
}
