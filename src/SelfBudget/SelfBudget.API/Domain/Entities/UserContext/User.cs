using SelfBudget.API.Domain.Entities.AccountContext;
using SelfBudget.API.Domain.ValueObjects;
using SelfBudget.API.Infrastructure.Abstractions;

namespace SelfBudget.API.Domain.Entities.UserContext;

/// <summary>
/// Пользователь
/// </summary>
public class User : AuditableEntity, IBaseEntity<Guid>
{
    protected User() { }

    public User(
        string name,
        EmailValueObject email,
        DateTime birthdate,
        Guid? photo = null) : base()
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PhotoId = photo;
        Birthdate = birthdate.ToUniversalTime();
    }

    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    public EmailValueObject Email { get; set; }

    /// <summary>
    /// Идентификатор фотографии пользователя
    /// </summary>
    public Guid? PhotoId { get; set; }

    /// <summary>
    /// Хэш пароля пользователя
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

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

    public void SetHashPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
