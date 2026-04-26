using BookingApi.Domain.Models;
using BookingApi.Presentation.Filters;

namespace BookingApi.Application.Interfaces;

public interface IEventService
{
    public Event GetById(Guid id);
    public PagedEvents GetAll(
        EventFilter filter,
        PaginationParams paginationParams);
    public Event Add(Event @event);
    public void Update(Event @event);
    public void Remove(Guid id);
}
