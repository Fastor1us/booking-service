using BookingApi.Application.Interfaces;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;
using BookingApi.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[ApiController]
[Route("api/events")]
public class EventController(IEventService eventService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        var res = await eventService.GetByIdAsync(id, ct);
        return Ok(res.MapToResponseDto());
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

        var res = await eventService
            .GetAllAsync(new(title, from, to), paginationParams, ct);

        return Ok(res.MapToPaginatedResponseDto(paginationParams));
    }

    [HttpPost]
    public async Task<ActionResult<EventResponseDto>> Add(
        [FromBody] PostEventDto @event, CancellationToken ct)
    {
        var createdEvent = await eventService
            .AddAsync(@event.MapToEvent(), ct);

        return CreatedAtAction(
            nameof(GetById), new { id = createdEvent.Id }, createdEvent);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventResponseDto>> Put(
        [FromBody] PutEventDto @event,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        await eventService.UpdateAsync(@event.MapToEvent(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid id, CancellationToken ct)
    {
        await eventService.RemoveAsync(id, ct);
        return NoContent();
    }
}
