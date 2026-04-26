using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventRepository
{
    public Event GetById(Guid id);
    PagedEvents GetPaged(
       IQueryable<Event> query,
       int pageIndex,
       int pageSize);
    IQueryable<Event> GetQueryable();
    public Guid Add(Event @event);
    public void Update(Event @event);
    public void Remove(Guid id);
}
