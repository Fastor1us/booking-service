using BookingApi.Domain.Models;
using BookingApi.Presentation.Filters;

namespace BookingApi.Application.Interfaces;

public interface IEventService
{
    public Task<Event> GetById(Guid id);
    public Task<PagedEvents> GetAll(
        EventFilter filter,
        PaginationParams paginationParams);
    public Task<Event> Add(Event @event);
    public Task Update(Event @event);
    public Task Remove(Guid id);
}
