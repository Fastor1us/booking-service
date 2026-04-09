using EventApi.Interfaces;
using EventApi.Models;
using EventApi.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService eventService) : ControllerBase
{
    [HttpGet("events/{id:guid}")]
    public ActionResult<EventResponseDto> GetById([FromRoute] Guid id)
    {
        var @event = eventService.GetById(id);

        return @event != null ? Ok(@event.MapToResponseDto()) : NotFound();
    }

    [HttpGet("events")]
    public ActionResult<IEnumerable<EventResponseDto>> GetAll()
    {
        return Ok(eventService.GetAll().Select(e => e.MapToResponseDto()));
    }

    [HttpPost("events")]
    public ActionResult<EventResponseDto> Add([FromBody] PostEventDto @event)
    {
        var createdEvent = eventService.Add(@event.MapToEvent());
        return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
    }

    [HttpPut("events/{id:guid}")]
    public ActionResult<EventResponseDto> Put([FromBody] PutEventDto @event, [FromRoute] Guid id)
    {
        return eventService.Update(@event.MapToEvent(id)) ? NoContent() : NotFound();
    }

    [HttpDelete("events/{id:guid}")]
    public IActionResult Remove([FromRoute] Guid id)
    {
        return eventService.Remove(id) ? NoContent() : NotFound();
    }
}
