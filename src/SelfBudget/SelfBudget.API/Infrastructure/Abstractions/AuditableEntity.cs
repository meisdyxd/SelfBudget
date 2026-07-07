namespace SelfBudget.API.Infrastructure.Abstractions;

public class AuditableEntity
{
    public AuditableEntity(
        string? createdBy = null,
        string? updatedBy = null)
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Дата и время создания сущности.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Дата и время обновления сущности
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Кем создано
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Кем обновлено
    /// </summary>
    public string? UpdatedBy { get; set; }
}
