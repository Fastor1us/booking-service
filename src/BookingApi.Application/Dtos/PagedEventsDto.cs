using BookingApi.Domain.Models;

namespace BookingApi.Application.Dtos;

public record PagedEventsDto(IEnumerable<Event> Items, int TotalCount);
