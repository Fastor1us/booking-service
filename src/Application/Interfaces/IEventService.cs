using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventService
{
    public Event GetById(Guid id);
    public IEnumerable<Event> GetAll();
    public Event Add(Event @event);
    public void Update(Event @event);
    public void Remove(Guid id);
}
