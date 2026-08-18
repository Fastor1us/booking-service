using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Repositories;

public abstract class RepositoryBase<T>
    : IRepository<T> where T : class
{
    public abstract Task<T?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    public abstract Task<T?> FirstOrDefaultAsync(
        QueryTrackerBehavior behavior,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    public async Task<T?> FirstOrDefaultAsync(
        IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
    {
        return await query.FirstOrDefaultAsync(predicate, ct);
    }

    public Task<int> CountAsync(
        IQueryable<T> query, CancellationToken ct = default)
    {
        return query.CountAsync(ct);
    }

    public Task<List<T>> ToListAsync(
        IQueryable<T> query, CancellationToken ct = default)
    {
        return query.ToListAsync(ct);
    }

    public abstract IQueryable<T> GetQuery(
        QueryTrackerBehavior behavior = QueryTrackerBehavior.Track);

    public abstract void Add(T entity);
}
