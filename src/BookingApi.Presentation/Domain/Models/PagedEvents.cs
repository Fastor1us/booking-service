namespace BookingApi.Presentation.Domain.Models;

public record PagedEvents(IEnumerable<Event> Items, int TotalCount);
