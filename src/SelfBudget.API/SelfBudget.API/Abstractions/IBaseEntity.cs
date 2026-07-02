namespace SelfBudget.API.Abstractions;

public interface IBaseEntity<TId>
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public TId Id { get; set; }
}
