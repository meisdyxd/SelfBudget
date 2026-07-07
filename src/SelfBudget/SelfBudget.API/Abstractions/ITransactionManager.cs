using CSharpFunctionalExtensions;
using SelfBudget.API.Common;
using System.Data;
using System.Data.Common;

namespace SelfBudget.API.Abstractions;

public interface ITransactionManager : IDisposable
{
    /// <summary>
    /// Начинает транзакцию БД
    /// </summary>
    Task BeginTransactionAsync(IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет изменения через EF Core (не коммитит транзакцию)
    /// </summary>
    Task<Result<int, Error>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Коммитит транзакцию
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Откатывает транзакцию
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Соединение для Dapper
    /// </summary>
    DbConnection Connection { get; }

    /// <summary>
    /// Транзакция для Dapper (можно передавать в методы ExecuteAsync)
    /// </summary>
    IDbTransaction? CurrentTransaction { get; }
}
