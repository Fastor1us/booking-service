using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class EventService(IEventRepository _eventRepository) : IEventService
{
    public Event GetById(Guid id)
    {
        return _eventRepository.GetById(id);
    }

    public IEnumerable<Event> GetAll()
    {
        return _eventRepository.GetAll();
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
