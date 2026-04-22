using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventRepository
{
    public Event? GetById(Guid id);
    public IEnumerable<Event> GetAll();
    public void Add(Event @event);
    public bool Update(Event @event);
    public bool Remove(Guid id);
}
