using BookingApi.Application.Dtos;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventService
{
    public Task<Event> GetByIdAsync(Guid id, CancellationToken ct);
    public Task<PagedEventsDto> GetAllAsync(
        EventFilterDto filter,
        PaginationParamsDto paginationParams,
        CancellationToken ct);
    public Task<Event> AddAsync(
        CreateEventDto @event,
        CancellationToken ct);
    public Task UpdateAsync(Guid id,
        UpdateEventDto @event,
        CancellationToken ct);
    public Task RemoveAsync(Guid id, CancellationToken ct);
}
