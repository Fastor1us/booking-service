using System.Linq.Expressions;

namespace Messaging.Abstractions.Persistence;

/// <summary>
///     Marker interface for all repositories in the system.
///     All repository implementations must inherit from this interface
///     to be recognized by the Unit of Work.
/// </summary>
/// <remarks>
///     This is a contract that ensures all repositories share the same
///     data access patterns and can be managed by the Unit of Work.
/// </remarks>
public interface IRepository<T> where T : class
{
    IQueryable<T> GetQuery(
        QueryTrackerBehavior behavior = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(
        QueryTrackerBehavior behavior,
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(
        IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task<int> CountAsync(
        IQueryable<T> query,
        CancellationToken ct = default);

    Task<List<T>> ToListAsync(
        IQueryable<T> query,
        CancellationToken ct = default);

    void Add(T entity);

    void Remove(T entity);
}
