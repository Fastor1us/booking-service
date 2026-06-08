using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventRepository
{
    public Task<Event?> TryGetByIdAsync(Guid id, CancellationToken ct);
    public Task<PagedEvents> GetPagedAsync(
       IQueryable<Event> query,
       int pageIndex,
       int pageSize,
       CancellationToken ct);
    public Task<IQueryable<Event>> GetQueryableAsync(CancellationToken ct);
    public Task<Guid> AddAsync(Event @event, CancellationToken ct);
    public Task<bool> TryUpdateAsync(Event @event, CancellationToken ct);
    public Task<bool> TryRemoveAsync(Guid id, CancellationToken ct);
}
