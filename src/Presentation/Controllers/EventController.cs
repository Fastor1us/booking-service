using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
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
    public ActionResult<EventResponseDto> GetById([FromRoute] Guid id)
    {
        return Ok(eventService.GetById(id).MapToResponseDto());
    }

    [HttpGet]
    public ActionResult<PaginatedEventsResponseDto> GetAll(
        [FromQuery] string? title, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        PaginationParams paginationParams = new(page, pageSize);

        PaginatedEventsResponseDto pagedEvents = eventService
            .GetAll(new(title, from, to), paginationParams)
            .MapToPaginatedResponseDto(paginationParams);

        return Ok(pagedEvents);
    }

    [HttpPost]
    public ActionResult<EventResponseDto> Add([FromBody] PostEventDto @event)
    {
        var createdEvent = eventService.Add(@event.MapToEvent());
        return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<EventResponseDto> Put([FromBody] PutEventDto @event, [FromRoute] Guid id)
    {
        eventService.Update(@event.MapToEvent(id));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Remove([FromRoute] Guid id)
    {
        eventService.Remove(id);
        return NoContent();
    }
}
