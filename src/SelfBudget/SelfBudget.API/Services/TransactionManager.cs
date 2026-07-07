using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SelfBudget.API.Abstractions;
using SelfBudget.API.Common;
using SelfBudget.API.Database;
using System.Data;
using System.Data.Common;

namespace SelfBudget.API.Services;

public class TransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction? _transaction;
    private readonly ILogger _logger;
    private bool _disposed;

    public TransactionManager(
        AppDbContext dbContext,
        ILogger<TransactionManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public DbConnection Connection => _dbContext.Database.GetDbConnection();

    public IDbTransaction? CurrentTransaction => _transaction?.GetDbTransaction();

    public async Task BeginTransactionAsync(
        IsolationLevel? isolationLevel = null,
        CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Транзакция уже начата");
        }
        var isolationLevelValue = isolationLevel ?? IsolationLevel.ReadCommitted;
        if (Connection.State != ConnectionState.Open)
            await Connection.OpenAsync(cancellationToken);

        _transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevelValue, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Транзакция не начата");
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task<Result<int, Error>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Транзакция не начата");
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении изменений в базе данных");
            return Result.Failure<int, Error>(new Error("Ошибка при сохранении изменений в базе данных", "error.database.save_changes"));
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _disposed = true;
        }
    }
}
