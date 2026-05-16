using BookingApi.Application.Interfaces;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;
using BookingApi.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[ApiController]
[Route("api/events")]
public class EventController(
    IEventService eventService,
    IBookingService bookingService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        var @event = await eventService.GetByIdAsync(id, ct);
        return Ok(@event.MapToResponseDto());
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedEventsResponseDto>> GetAll(
        [FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        PaginationParams paginationParams = new(page, pageSize);

        var @event = await eventService
            .GetAllAsync(new(title, from, to), paginationParams, ct);
        return Ok(@event.MapToPaginatedResponseDto(paginationParams));
    }

    [HttpPost]
    public async Task<ActionResult<EventResponseDto>> Add(
        [FromBody] PostEventDto eventDto, CancellationToken ct)
    {
        var @event = await eventService
            .AddAsync(eventDto.MapToEvent(), ct);
        return CreatedAtAction(
            nameof(GetById), new { id = @event.Id }, @event);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventResponseDto>> Put(
        [FromBody] PutEventDto eventDto,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        await eventService.UpdateAsync(eventDto.MapToEvent(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid id, CancellationToken ct)
    {
        await eventService.RemoveAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/book")]
    public async Task<ActionResult<BookingResponseDto>> Book(
        [FromRoute] Guid id, CancellationToken ct)
    {
        var booking = await bookingService.CreateBookingAsync(id, ct);
        return Accepted(booking.MapToResponseDto());
    }
}
