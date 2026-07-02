using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

public class Photo : AuditableEntity, IBaseEntity<Guid>
{
    public Photo(
        Guid userId,
        string uri,
        int size)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Uri = uri;
        Size = size;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// URL фотографии
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    /// Размер изображения в байтах
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public virtual User User { get; set; }
}
