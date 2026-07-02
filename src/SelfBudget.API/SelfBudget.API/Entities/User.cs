using SelfBudget.API.Abstractions;

namespace SelfBudget.API.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class User : AuditableEntity, IBaseEntity<Guid>
{
    public User(
        string name,
        string email,
        string? photo,
        string passwordHash,
        DateTime birthdate)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PhotoId = photo;
        PasswordHash = passwordHash;
        Birthdate = birthdate;
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Идентификатор фотографии пользователя
    /// </summary>
    public string? PhotoId { get; set; }

    /// <summary>
    /// Хэш пароля пользователя
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Дата рождения пользователя
    /// </summary>
    public DateTime Birthdate { get; set; }

    /// <summary>
    /// Навигационная сущность фото
    /// </summary>
    public virtual Photo? Photo { get; set; }

    /// <summary>
    /// Счета пользователя
    /// </summary>
    public virtual ICollection<Account> Accounts { get; set; } = [];
}
