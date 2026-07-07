using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities.TransactionContext;

public class TransactionCategory : IBaseEntity<Guid>
{
    protected TransactionCategory() { }

    public TransactionCategory(
        Guid transactionCategoryTypeId,
        string name,
        Guid? baseCategoryId = null)
    {
        TransactionCategoryTypeId = transactionCategoryTypeId;
        Name = name;
        BaseTransactionCategoryId = baseCategoryId;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Родительская категория транзакции
    /// </summary>
    public Guid? BaseTransactionCategoryId { get; set; }

    /// <summary>
    /// Идентификатор типа категории транзакции
    /// </summary>
    public Guid TransactionCategoryTypeId { get; set; }

    /// <summary>
    /// Наименование типа
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Тип
    /// </summary>
    public virtual TransactionCategoryType Type { get; set; } = null!;

    /// <summary>
    /// Родительская категория транзакции
    /// </summary>
    public virtual TransactionCategory? BaseTransactionCategory { get; set; }

    /// <summary>
    /// Дочерние категории транзакций
    /// </summary>
    public virtual ICollection<TransactionCategory> Childrens { get; set; } = [];

    /// <summary>
    /// Транзакции
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = [];
}
