namespace BookingApi.Presentation.Application.Interfaces;

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
    /// <summary>
    ///     Builds a queryable collection of entity with configurable tracking behavior.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns an <see cref="IQueryable{Event}"/> that serves as a query builder.
    ///         The actual data source execution occurs when the query is enumerated.
    ///     </para>
    ///     <para>
    ///         <b>Example:</b>
    ///         <code>
    ///             var entities = repository.GetQuery(QueryTrackerBehavior.NoTrack)
    ///                 .Where(e => e.StartAt > DateTime.UtcNow)
    ///                 .OrderByDescending(e => e.StartAt)
    ///                 .Take(10)
    ///                 .ToList();
    ///         </code>
    ///     </para>
    ///     <para>
    ///         <b>Note:</b> This method is the most flexible way to query entities.
    ///     </para>
    /// </remarks>
    /// <returns>A composable <see cref="IQueryable{Event}"/> for building dynamic queries.</returns>
    IQueryable<T> GetQuery(QueryTrackerBehavior behavior = default);

    /// <summary>
    ///     Full search for first entry with QueryTrackerBehavior.Track as default.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(
        QueryTrackerBehavior behavior,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(
        IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task<int> CountAsync(IQueryable<T> query, CancellationToken ct = default);

    Task<List<T>> ToListAsync(IQueryable<T> query, CancellationToken ct = default);

    public void Add(T entity);
}
