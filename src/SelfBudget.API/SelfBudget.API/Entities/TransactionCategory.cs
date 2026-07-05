using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

public class TransactionCategory : IBaseEntity<Guid>
{
    public TransactionCategory(
        Guid transactionCategoryTypeId,
        string name,
        Guid? baseCategoryId)
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
    public string Name { get; set; }

    /// <summary>
    /// Тип
    /// </summary>
    public virtual TransactionCategoryType Type { get; set; }

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
