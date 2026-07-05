using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

public class Tag : AuditableEntity, IBaseEntity<Guid>
{
    public Tag(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Наименование тега
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Транзакции
    /// </summary>
    public virtual ICollection<TransactionTag> TransactionTags { get; set; } = [];
}
