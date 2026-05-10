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
    public async Task<ActionResult<EventResponseDto>> GetById([FromRoute] Guid id)
    {
        return Ok((await eventService.GetById(id)).MapToResponseDto());
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedEventsResponseDto>> GetAll(
        [FromQuery] string? title, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        PaginationParams paginationParams = new(page, pageSize);

        PaginatedEventsResponseDto pagedEvents = (await eventService
            .GetAll(new(title, from, to), paginationParams))
            .MapToPaginatedResponseDto(paginationParams);

        return Ok(pagedEvents);
    }

    [HttpPost]
    public async Task<ActionResult<EventResponseDto>> Add([FromBody] PostEventDto @event)
    {
        var createdEvent = await eventService.Add(@event.MapToEvent());
        return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventResponseDto>> Put([FromBody] PutEventDto @event, [FromRoute] Guid id)
    {
        await eventService.Update(@event.MapToEvent(id));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove([FromRoute] Guid id)
    {
        await eventService.Remove(id);
        return NoContent();
    }
}
