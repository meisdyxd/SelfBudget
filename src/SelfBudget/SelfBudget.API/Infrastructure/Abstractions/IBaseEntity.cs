namespace SelfBudget.API.Infrastructure.Abstractions;

public interface IBaseEntity<TId>
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public TId Id { get; set; }
}
