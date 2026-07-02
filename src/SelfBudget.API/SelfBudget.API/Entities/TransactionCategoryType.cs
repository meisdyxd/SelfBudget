using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

/// <summary>
/// Справочник типов категорий транзакции
/// </summary>
public class TransactionCategoryType : IBaseEntity<Guid>
{
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
    public string Name { get; set; }

    /// <summary>
    /// Категории транзакций
    /// </summary>
    public virtual ICollection<TransactionCategory> Categories { get; set; } = [];
}
