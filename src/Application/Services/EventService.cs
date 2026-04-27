using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Presentation.Filters;

namespace BookingApi.Application.Services;

public class EventService(IEventRepository _eventRepository)
    : IEventService
{
    public Event GetById(Guid id)
    {
        return _eventRepository.GetById(id);
    }

    public PagedEvents GetAll(
        EventFilter filter, PaginationParams paginationParams)
    {
        var query = _eventRepository.GetQueryable();

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

        var res = _eventRepository.GetPaged(
            query,
            paginationParams.PageIndex,
            paginationParams.PageSize);

        return new(res.Items, res.TotalCount);
    }

    public Event Add(Event @event)
    {
        var id = _eventRepository.Add(@event);
        return _eventRepository.GetById(id);
    }

    public void Update(Event @event)
    {
        _eventRepository.Update(@event);
    }

    public void Remove(Guid id)
    {
        _eventRepository.Remove(id);
    }
}
