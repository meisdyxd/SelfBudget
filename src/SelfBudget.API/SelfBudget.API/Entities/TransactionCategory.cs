using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

public class TransactionCategory : IBaseEntity<Guid>
{
    public TransactionCategory(
        Guid transactionCategoryTypeId,
        string name)
    {
        TransactionCategoryTypeId = transactionCategoryTypeId;
        Name = name;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

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
}
