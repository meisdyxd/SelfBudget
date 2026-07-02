namespace SelfBudget.API.Abstractions;

public class AuditableEntity
{
    /// <summary>
    /// Дата и время создания сущности.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Дата и время обновления сущности
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
