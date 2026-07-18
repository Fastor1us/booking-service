using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventRepository : IRepository
{
    /// <summary>
    ///     Builds a queryable collection of events with configurable tracking behavior.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns an <see cref="IQueryable{Event}"/> that serves as a query builder.
    ///         The actual data source execution occurs when the query is enumerated.
    ///     </para>
    ///     <para>
    ///         <b>Example:</b>
    ///         <code>
    ///             var upcomingEvents = repository.GetQuery(QueryTrackerBehavior.NoTrack)
    ///                 .Where(e => e.StartAt > DateTime.UtcNow)
    ///                 .OrderByDescending(e => e.StartAt)
    ///                 .Take(10)
    ///                 .ToList();
    ///         </code>
    ///     </para>
    ///     <para>
    ///         <b>Note:</b> This method is the most flexible way to query events.
    ///         For common scenarios, consider using specialized methods like <see cref="GetByIdAsync"/>
    ///         or <see cref="GetEvents"/>.
    ///     </para>
    /// </remarks>
    /// <returns>A composable <see cref="IQueryable{Event}"/> for building dynamic queries.</returns>
    IQueryable<Event> GetQuery(QueryTrackerBehavior behavior = default);

    void Add(Event @event);

    /// <summary>
    ///     Performs a direct, atomic update of an event by its ID without loading the entity into memory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This operation executes immediately and bypasses the ChangeTracker.
    ///         It does NOT require calling <see cref="IUnitOfWork.SaveChangesAsync"/> afterwards.
    ///     </para>
    ///     <para>
    ///         Use this method when:
    ///         <list type="bullet">
    ///             <item>You need to update specific fields without retrieving the full entity</item>
    ///             <item>Optimizing performance for bulk or partial updates</item>
    ///             <item>Working with high-concurrency scenarios where optimistic concurrency is not required</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         For tracked updates (with change detection), use <see cref="GetByIdAsync"/> followed by 
    ///         <see cref="IUnitOfWork.SaveChangesAsync"/> instead.
    ///     </para>
    /// </remarks>
    /// <returns>Number of entities updated (0 if event not found, 1 if successful)</returns>
    Task<int> ExecuteUpdateByIdAsync(
        Guid id,
        string title,
        string? description,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        CancellationToken ct = default);

    /// <summary>
    ///     Permanently removes an event by its ID without loading the entity into memory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This operation executes immediately and bypasses the ChangeTracker.
    ///         It does NOT require calling <see cref="IUnitOfWork.SaveChangesAsync"/> afterwards.
    ///     </para>
    ///     <para>
    ///         Use this method when:
    ///         <list type="bullet">
    ///              <item>You need to delete an event without retrieving it first</item>
    ///             <item>Optimizing performance for direct deletion operations</item>
    ///             <item>Working with high-concurrency scenarios</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///     <b>Important:</b> This operation is immediate and bypasses all change tracking.
    ///         For tracked deletion, use <see cref="GetByIdAsync"/> followed by <see cref="RemoveAsync"/> 
    ///         and <see cref="IUnitOfWork.SaveChangesAsync"/>.
    ///     </para>
    /// </remarks>
    /// <returns>Number of entities deleted (0 if event not found, 1 if successful)</returns>
    Task<int> ExecuteDeleteByIdAsync(Guid id, CancellationToken ct = default);
}
