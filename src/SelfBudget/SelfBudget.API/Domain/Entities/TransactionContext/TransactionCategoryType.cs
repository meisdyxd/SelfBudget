using SelfBudget.API.Infrastructure.Abstractions;

namespace SelfBudget.API.Domain.Entities.TransactionContext;

/// <summary>
/// Справочник типов категорий транзакции
/// </summary>
public class TransactionCategoryType : IBaseEntity<Guid>
{
    protected TransactionCategoryType() { }

    public TransactionCategoryType(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Наименование типа
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Категории транзакций
    /// </summary>
    public virtual ICollection<TransactionCategory> Categories { get; set; } = [];
}
