using EventApi.Interfaces;
using EventApi.Models;

namespace EventApi.Services;

public class EventService : IEventService
{
    private readonly List<Event> _events = [];
    private readonly Lock locker = new();

    public EventResponseDto? GetById(Guid id)
    {
        using (locker.EnterScope())
        {
            return _events
                .FirstOrDefault(e => e.Id == id)
                ?.MapToResponseDto();
        }
    }

    public IEnumerable<EventResponseDto> GetAll()
    {
        using (locker.EnterScope())
        {
            return [.. _events.Select(i => i.MapToResponseDto())];
        }
    }

    public EventResponseDto Add(EventRequestDto item)
    {
        using (locker.EnterScope())
        {
            string lowerItemTitle = item.Title.ToLower();
            if (_events.Any(e => e.Title.ToLower() == lowerItemTitle))
            {
                throw new ArgumentException($"Event with Title '{item.Title}' already exist");
            }

            var newId = Guid.NewGuid();
            while (_events.Any(e => e.Id == newId))
            {
                newId = Guid.NewGuid();
            }

            EventResponseDto newEvent = new()
            {
                Id = newId,
                Title = item.Title,
                Description = item.Description,
                StartAt = item.StartAt,
                EndAt = item.EndAt
            }; 

            _events.Add(newEvent);

            return newEvent;
        }
    }

    public bool Update(Guid id, EventRequestDto item)
    {
        using (locker.EnterScope())
        {
            bool updated = false;

            var index = _events.FindIndex(e => e.Id == id);
            if (index != -1)
            {
                _events[index] = new()
                {
                    Id = id,
                    Title = item.Title,
                    Description = item.Description,
                    StartAt = item.StartAt,
                    EndAt = item.EndAt
                };
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
