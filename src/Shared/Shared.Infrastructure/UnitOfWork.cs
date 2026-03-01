using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Shared.Infrastructure;

public class UnitOfWork(params DbContext[] contexts) : IUnitOfWork
{
    private readonly IEnumerable<DbContext> _contexts = contexts;
    private readonly List<IDbContextTransaction> _transactions = [];
    private bool _disposed;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var context in _contexts)
        {
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            _transactions.Add(transaction);
        }

        var totalChanges = 0;
        foreach (var context in _contexts)
        {
            totalChanges += await context.SaveChangesAsync(cancellationToken);
        }

        return totalChanges;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        foreach (var transaction in _transactions)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        foreach (var transaction in _transactions)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        _transactions.Clear();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var transaction in _transactions)
                {
                    transaction.Dispose();
                }
                _transactions.Clear();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
