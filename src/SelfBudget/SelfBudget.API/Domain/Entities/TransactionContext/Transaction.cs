using SelfBudget.API.Domain.Entities.AccountContext;
using SelfBudget.API.Infrastructure.Abstractions;

namespace SelfBudget.API.Domain.Entities.TransactionContext;

public class Transaction : AuditableEntity, IBaseEntity<Guid>, IEquatable<Transaction>
{
    protected Transaction() { }

    public Transaction(
        decimal amount,
        Guid fromAccountId,
        Guid toAccountId,
        Guid transactionCategoryId,
        string? note = null) : base()
    {
        Id = Guid.NewGuid();
        Amount = amount;
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        TransactionCategoryId = transactionCategoryId;
        Note = note;
        CreatedAt = DateTime.UtcNow;
    }

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
    public virtual Account FromAccount { get; set; } = null!;

    /// <summary>
    /// На счет
    /// </summary>
    public virtual Account ToAccount { get; set; } = null!;

    /// <summary>
    /// Категория
    /// </summary>
    public virtual TransactionCategory Category { get; set; } = null!;

    public override bool Equals(object? obj)
    {
        return obj is Transaction transaction && Equals(transaction);
    }

    public bool Equals(Transaction? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id == other.Id 
            && Amount == other.Amount 
            && FromAccountId == other.FromAccountId 
            && ToAccountId == other.ToAccountId 
            && TransactionCategoryId == other.TransactionCategoryId 
            && Note == other.Note;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FromAccountId, ToAccountId, CreatedAt);
    }

    public static bool operator ==(Transaction transaction1, Transaction transaction2)
    {
        if (transaction1 is null)
            return transaction2 is null;

        return transaction1.Equals(transaction2);
    }

    public static bool operator !=(Transaction transaction1, Transaction transaction2)
    {
        if (transaction1 is null)
            return transaction2 is not null;

        return !transaction1.Equals(transaction2);
    }
}
