using EventApi.Models;

namespace EventApi.Interfaces;

public interface IEventService
{
    public Event? GetById(Guid id);
    public IEnumerable<Event> GetAll();
    public bool Add(Event item);
    public bool Update(Event item);
    public bool Remove(Guid id);
}
