using CSharpFunctionalExtensions;
using SelfBudget.API.Common;
using SelfBudget.API.Domain.Entities.TransactionContext;
using SelfBudget.API.Domain.Entities.UserContext;
using SelfBudget.API.Infrastructure.Abstractions;

namespace SelfBudget.API.Domain.Entities.AccountContext;

/// <summary>
/// Счет пользователя
/// </summary>
public class Account : AuditableEntity, IBaseEntity<Guid>
{
    protected Account() { }

    public Account(
        Guid userId,
        string name,
        string currencyCode,
        Guid accountTypeId) : base()
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
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Код валюты
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

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

    /// <summary>
    /// Перевод средств на другой счет
    /// </summary>
    /// <param name="targetAccount">Счет получателя</param>
    /// <param name="amount">Сумма перевода</param>
    /// <returns>Результат выполнения операции</returns>
    public Result<bool, Error> TransferTo(Account targetAccount, decimal amount)
    {
        if (amount <= 1)
        {
            return new Error("Сумма перевода должна быть больше единицы.");
        }
        if (Balance - amount < -OverdraftLimit)
        {
            return new Error("Недостаточно средств для перевода.");
        }
        Balance -= amount;
        targetAccount.Balance += amount;

        return true;
    }
}
