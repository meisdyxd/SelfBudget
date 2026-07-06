using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SelfBudget.API.Abstractions;
using SelfBudget.API.Database;
using System.Data;
using System.Data.Common;

namespace SelfBudget.API.Services;

public class TransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public TransactionManager(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public DbConnection Connection => _dbContext.Database.GetDbConnection();

    public IDbTransaction? CurrentTransaction => _transaction?.GetDbTransaction();

    public async Task BeginTransactionAsync(
        IsolationLevel? isolationLevel,
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Транзакция не начата");

        await _dbContext.SaveChangesAsync(cancellationToken);
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
