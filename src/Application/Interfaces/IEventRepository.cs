using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventRepository
{
    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct);
    public Task<PagedEvents> GetPagedAsync(
       IQueryable<Event> query,
       int pageIndex,
       int pageSize,
       CancellationToken ct);
    public Task<IQueryable<Event>> GetQueryableAsync(CancellationToken ct);
    public Task<Guid> AddAsync(Event @event, CancellationToken ct);
    public Task<bool> UpdateAsync(Event @event, CancellationToken ct);
    public Task<bool> RemoveAsync(Guid id, CancellationToken ct);
}
