using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;

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
    public Task<Guid> AddAsync(CreateEventDto @event, CancellationToken ct);
    public Task<bool> TryUpdateAsync(
        Guid id,
        UpdateEventDto @event,
        CancellationToken ct);
    public Task<bool> TryRemoveAsync(Guid id, CancellationToken ct);
}
