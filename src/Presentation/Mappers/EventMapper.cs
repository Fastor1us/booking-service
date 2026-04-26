using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;

namespace BookingApi.Presentation.Mappers;

public static class EventMapperExtension
{
    public static Event MapToEvent(this PostEventDto @event)
    {
        return new Event
        {
            Id = Guid.Empty,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }

    public static Event MapToEvent(this PutEventDto @event, Guid id)
    {
        return new Event
        {
            Id = id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }

    public static EventResponseDto MapToResponseDto(this Event @event)
    {
        return new EventResponseDto
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }

    public static PaginatedEventsResponseDto MapToPaginatedResponseDto(
        this PagedEvents pagedEvents, PaginationParams paginationParams)
    {
        return new PaginatedEventsResponseDto
        {
            Items = pagedEvents.Items.MapToResponseDto(),
            TotalCount = pagedEvents.TotalCount,
            PageIndex = paginationParams.PageIndex
        };
    }

    public static IEnumerable<EventResponseDto> MapToResponseDto(
        this IEnumerable<Event> @events)
    {
        return @events.Select(e => e.MapToResponseDto());
    }
}
