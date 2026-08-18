using EventService.Domain.Models;

namespace EventService.Application.Dtos;

public record PagedEventsDto(IEnumerable<Event> Items, int TotalCount);
