using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventRepository : IRepository<Event>
{
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
    /// </remarks>
    /// <returns>Number of entities updated (0 if event not found, 1 if successful)</returns>
    Task<int> ExecuteUpdateByIdAsync(Event @event, CancellationToken ct = default);

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
    /// </remarks>
    /// <returns>Number of entities deleted (0 if event not found, 1 if successful)</returns>
    Task<int> ExecuteDeleteByIdAsync(Guid id, CancellationToken ct = default);
}
