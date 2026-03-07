using Microsoft.EntityFrameworkCore;
using Shared.Domain.Repositories;
using System.Linq.Expressions;

namespace Shared.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _set;

    public Repository(DbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _set.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _set.ToListAsync(cancellationToken);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _set.Where(predicate).ToListAsync(cancellationToken);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate == null ? await _set.CountAsync(cancellationToken) : await _set.CountAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
