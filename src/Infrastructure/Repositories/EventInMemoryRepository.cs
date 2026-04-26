using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.Repositories;

public class EventInMemoryRepository : IEventRepository
{
    private readonly Dictionary<Guid, Event> _events = [];
    private readonly Lock locker = new();

    public Event GetById(Guid id)
    {
        using (locker.EnterScope())
        {
            return _events.GetValueOrDefault(id) ??
                throw new EventNotFoundException(id);
        }
    }

    public PagedEvents GetPaged(
        IQueryable<Event> query,
        int pageIndex,
        int pageSize)
    {
        using (locker.EnterScope())
        {
            var totalCount = query.Count();

            var items = query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new(items, totalCount);
        }
    }

    public IQueryable<Event> GetQueryable()
    {
        using (locker.EnterScope())
        {
            return _events.Values.AsQueryable();
        }
    }

    public Guid Add(Event @event)
    {
        using (locker.EnterScope())
        {
            var newId = Guid.NewGuid();

            Event newEvent = new()
            {
                Id = newId,
                Title = @event.Title,
                Description = @event.Description,
                StartAt = @event.StartAt,
                EndAt = @event.EndAt
            };

            _events.TryAdd(newId, newEvent);
            return newId;
        }
    }

    public void Update(Event @event)
    {
        using (locker.EnterScope())
        {
            if (_events.TryGetValue(@event.Id, out Event? existedEvent))
                _events[@event.Id] = @event;
            else
                throw new EventNotFoundException(@event.Id);
        }
    }

    public void Remove(Guid id)
    {
        using (locker.EnterScope())
        {
            if (!_events.Remove(id))
                throw new EventNotFoundException(id);
        }
    }
}
