using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.Repositories;

// базовые проверки на null (без проверок бизнес правил)
public class EventInMemoryRepository : IEventRepository
{
    private readonly List<Event> _events = [];
    private readonly Lock _locker = new();

    public Event? GetById(Guid id)
    {
        using (_locker.EnterScope())
            return _events.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<Event> GetAll()
    {
        using (_locker.EnterScope())
            return _events.ToList();
    }

    public void Add(Event @event)
    {
        using (_locker.EnterScope())
        {
            _events.Add(@event);
        }
    }

    public bool Update(Event @event)
    {
        using (_locker.EnterScope())
        {
            var index = _events.FindIndex(e => e.Id == @event.Id);
            if (index == -1) return false;

            _events[index] = @event;
            return true;
        }
    }

    public bool Remove(Guid id)
    {
        using (_locker.EnterScope())
        {
            var index = _events.FindIndex(e => e.Id == id);
            if (index == -1) return false;

            _events.RemoveAt(index);
            return true;
        }
    }
}
