using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

// TODO: 
// Валидируем, выкидываем ошибки, прокидываем пагинацию в репозиторий
public class EventService : IEventService
{
    private readonly List<Event> _events = [];
    private readonly Lock locker = new();

    public Event? GetById(Guid id)
    {
        using (locker.EnterScope())
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }
    }

    public IEnumerable<Event> GetAll()
    {
        using (locker.EnterScope())
        {
            return _events;
        }
    }

    public Event Add(Event @event)
    {
        using (locker.EnterScope())
        {
            var newId = Guid.NewGuid();
            while (_events.Any(e => e.Id == newId))
            {
                newId = Guid.NewGuid();
            }

            Event newEvent = new()
            {
                Id = newId,
                Title = @event.Title,
                Description = @event.Description,
                StartAt = @event.StartAt,
                EndAt = @event.EndAt
            };

            _events.Add(newEvent);

            return newEvent;
        }
    }

    public bool Update(Event @event)
    {
        using (locker.EnterScope())
        {
            bool updated = false;

            var index = _events.FindIndex(e => e.Id == @event.Id);
            if (index != -1)
            {
                _events[index] = @event;
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
