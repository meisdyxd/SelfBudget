using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

public class AccountType : AuditableEntity, IBaseEntity<Guid>
{
    protected AccountType() { }

    public AccountType(
        string name,
        string? description) : base()
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Наименование типа счета
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание типа счета
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Счета, относящиеся к данному типу
    /// </summary>
    public virtual ICollection<Account> Accounts { get; set; } = [];
}
