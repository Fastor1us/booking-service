using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;

namespace BookingApi.Application.Services;

public class EventService : IEventService
{
    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedEvents> GetAllAsync(
        EventFilter filter,
        PaginationParams paginationParams,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Event> AddAsync(CreateEventDto @event, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Guid id, UpdateEventDto @event, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
