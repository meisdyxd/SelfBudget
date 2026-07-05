using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

public class Transaction : AuditableEntity, IBaseEntity<Guid>
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Сумма транзакции
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Со счета
    /// </summary>
    public Guid FromAccountId { get; set; }

    /// <summary>
    /// На счет
    /// </summary>
    public Guid ToAccountId { get; set; }

    /// <summary>
    /// Идентификатор категории транзакции
    /// </summary>
    public Guid TransactionCategoryId { get; set; }

    /// <summary>
    /// Комментарий транзакции
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Теги транзакции
    /// </summary>
    public virtual ICollection<TransactionTag> TransactionTags { get; set; } = [];

    /// <summary>
    /// Со счета
    /// </summary>
    public virtual Account FromAccount { get; set; }

    /// <summary>
    /// На счет
    /// </summary>
    public virtual Account ToAccount { get; set; }

    /// <summary>
    /// Категория
    /// </summary>
    public virtual TransactionCategory Category { get; set; }
}
