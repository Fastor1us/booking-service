using EventService.Application.Dtos;
using EventService.Domain.Models;
using EventService.Presentation.Dtos;

namespace EventService.Presentation.Mappers;

public static class EventMapperExtension
{
    public static EventResponseDto MapToResponseDto(this Event @event)
    {
        return new EventResponseDto
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.AvailableSeats,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }

    public static PaginatedEventsResponseDto MapToPaginatedResponseDto(
        this PagedEventsDto pagedEvents, PaginationParamsDto paginationParams)
    {
        return new PaginatedEventsResponseDto
        {
            Items = pagedEvents.Items.MapToResponseDto(),
            TotalCount = pagedEvents.TotalCount,
            PageIndex = paginationParams.PageIndex,
            ItemsCount = pagedEvents.Items.Count()
        };
    }

    public static IEnumerable<EventResponseDto> MapToResponseDto(
        this IEnumerable<Event> @events)
    {
        return @events.Select(e => e.MapToResponseDto());
    }
}
