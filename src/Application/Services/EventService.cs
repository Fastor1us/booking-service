using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Presentation.Filters;

namespace BookingApi.Application.Services;

public class EventService(IEventRepository _eventRepository)
    : IEventService
{
    public Task<Event> GetById(Guid id)
    {
        return _eventRepository.GetById(id);
    }

    public async Task<PagedEvents> GetAll(
        EventFilter filter, PaginationParams paginationParams)
    {
        var query = await _eventRepository.GetQueryable();

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

        var res = await _eventRepository.GetPaged(
            query,
            paginationParams.PageIndex,
            paginationParams.PageSize);

        return new(res.Items, res.TotalCount);
    }

    public async Task<Event> Add(Event @event)
    {
        var id = await _eventRepository.Add(@event);
        return await _eventRepository.GetById(id);
    }

    public Task Update(Event @event)
    {
        return _eventRepository.Update(@event);
    }

    public Task Remove(Guid id)
    {
        return _eventRepository.Remove(id);
    }
}
