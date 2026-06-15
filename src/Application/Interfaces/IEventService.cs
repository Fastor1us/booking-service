using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;

namespace BookingApi.Application.Interfaces;

public interface IEventService
{
    public Task<Event> GetByIdAsync(Guid id, CancellationToken ct);
    public Task<PagedEvents> GetAllAsync(
        EventFilter filter,
        PaginationParams paginationParams,
        CancellationToken ct);
    public Task<Event> AddAsync(
        CreateEventDto @event,
        CancellationToken ct);
    public Task UpdateAsync(Guid id,
        UpdateEventDto @event,
        CancellationToken ct);
    public Task RemoveAsync(Guid id, CancellationToken ct);
}
