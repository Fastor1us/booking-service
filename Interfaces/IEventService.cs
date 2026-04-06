using EventApi.Models;

namespace EventApi.Interfaces;

public interface IEventService
{
    public EventResponseDto? GetById(Guid id);
    public IEnumerable<EventResponseDto> GetAll();
    public EventResponseDto Add(EventRequestDto item);
    public bool Update(Guid id, EventRequestDto item);
    public bool Remove(Guid id);
}
