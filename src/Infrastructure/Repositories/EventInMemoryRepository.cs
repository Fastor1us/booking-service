using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.Repositories;

public class EventInMemoryRepository : IEventRepository
{
    private readonly Dictionary<Guid, Event> _events = [];
    private readonly Lock locker = new();

    public EventInMemoryRepository()
    {
        // [x] Use to generate base events! UwU
        // UNCOMMENT THIS BLOCK TO POPULATE THE REPOSITORY WITH TEST DATA
        // (20 events with sequential dates)

        // int length = 20;

        // var events = Enumerable.Range(1, length).Select((index) =>
        // {
        //     Guid guid = Guid.NewGuid();
        //     DateTime date = DateTime.Now;
        //     return new Event
        //     {
        //         Id = guid,
        //         Title = "Title #" + index.ToString(),
        //         StartAt = date.AddDays(-1 * (length - index)),
        //         EndAt = date.AddDays(-1 * (length - index - 1))
        //     };
        // });
        // foreach (var @event in events)
        // {
        //     _events.TryAdd(@event.Id, @event);
        // }
    }

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
