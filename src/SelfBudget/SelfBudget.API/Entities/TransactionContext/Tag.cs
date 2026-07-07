using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities.TransactionContext;

public class Tag : AuditableEntity, IBaseEntity<Guid>
{
    protected Tag() { }

    public Tag(string name) : base()
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Наименование тега
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Транзакции
    /// </summary>
    public virtual ICollection<TransactionTag> TransactionTags { get; set; } = [];
}
