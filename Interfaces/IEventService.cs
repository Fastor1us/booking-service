using EventApi.Models;

namespace EventApi.Interfaces;

public interface IEventService
{
    public Event? GetById(Guid id);
    public IEnumerable<Event> GetAll();
    public Event Add(Event @event);
    public bool Update(Event @event);
    public bool Remove(Guid id);
}
