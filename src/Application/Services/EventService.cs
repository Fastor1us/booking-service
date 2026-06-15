using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;

namespace BookingApi.Application.Services;

public class EventService(IEventRepository _eventRepository)
    : IEventService
{
    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _eventRepository.TryGetByIdAsync(id, ct)
            ?? throw new EventNotFoundException(id);
    }

    public async Task<PagedEvents> GetAllAsync(
        EventFilter filter,
        PaginationParams paginationParams,
        CancellationToken ct)
    {
        var query = await _eventRepository.GetQueryableAsync(ct);

        if (!string.IsNullOrEmpty(filter.Title))
        {
            query = query.Where(e => e.Title.Contains(
                filter.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.From.HasValue)
        {
            query = query.Where(e => e.StartAt >= filter.From);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(e => e.EndAt <= filter.To);
        }

        var res = await _eventRepository.GetPagedAsync(
            query,
            paginationParams.PageIndex,
            paginationParams.PageSize,
            ct);

        return new(res.Items, res.TotalCount);
    }

    public async Task<Event> AddAsync(CreateEventDto @event, CancellationToken ct)
    {
        var id = await _eventRepository.AddAsync(@event, ct);

        return await _eventRepository.TryGetByIdAsync(id, ct)
            ?? throw new EventNotFoundException(id);
    }

    public async Task UpdateAsync(Guid id, UpdateEventDto @event, CancellationToken ct)
    {
        if (!await _eventRepository.TryUpdateAsync(id, @event, ct))
            throw new EventNotFoundException(id);
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        if (!await _eventRepository.TryRemoveAsync(id, ct))
            throw new EventNotFoundException(id);
    }
}
