using EventApi.Interfaces;
using EventApi.Models;

namespace EventApi.Services;

public class EventService : IEventService
{
    private List<Event> _events = [];
    private readonly Lock locker = new();

    public bool Add(Event item)
    {
        using (locker.EnterScope())
        {

            bool createdNew = false;

            if (!_events.Any(e => e.Id == item.Id))
            {
                _events.Add(item);
                createdNew = true;
            }

            return createdNew;
        }
    }

    public IEnumerable<Event> GetAll()
    {
        using (locker.EnterScope())
        {
            return [.. _events];
        }
    }

    public Event? GetById(Guid id)
    {
        using (locker.EnterScope())
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }
    }

    public bool Update(Event item)
    {
        using (locker.EnterScope())
        {
            bool updated = false;

            var index = _events.FindIndex(e => e.Id == item.Id);
            if (index == -1)
            {
                _events.Add(item);
                updated = true;
            }
            else
            {
                _events[index] = item;
                updated = true;
            }

            return updated;
        }
    }

    public bool Remove(Guid id)
    {
        using (locker.EnterScope())
        {
            bool removed = false;

            var index = _events.FindIndex(e => e.Id == id);
            if (index != -1)
            {
                _events.RemoveAt(index);
                removed = true;
            }

            return removed;
        }
    }
}
