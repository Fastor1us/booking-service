using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IEventRepository
{
    public Task<Event> GetById(Guid id);
    public Task<PagedEvents> GetPaged(
       IQueryable<Event> query,
       int pageIndex,
       int pageSize);
    public Task<IQueryable<Event>> GetQueryable();
    public Task<Guid> Add(Event @event);
    public Task Update(Event @event);
    public Task Remove(Guid id);
}
