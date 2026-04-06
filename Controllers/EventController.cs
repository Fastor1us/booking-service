using EventApi.Interfaces;
using EventApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService eventService) : ControllerBase
{
    [HttpGet("/events/{id:guid}")]
    public ActionResult<EventResponseDto> GetById([FromRoute] Guid id)
    {
        try
        {
            var item = eventService.GetById(id);

            return item != null ? Ok(item) : NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("/events")]
    public ActionResult<IEnumerable<EventResponseDto>> GetAll()
    {
        try
        {
            return Ok(eventService.GetAll());
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("/events")]
    public ActionResult<EventResponseDto> Add([FromBody] EventRequestDto item)
    {
        try
        {
            var createdEvent = eventService.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("/events/{id:guid}")]
    public IActionResult Put([FromBody] EventRequestDto item, [FromRoute] Guid id)
    {
        try
        {
            return eventService.Update(id, item) ? NoContent() : NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("/events/{id:guid}")]
    public IActionResult Remove([FromRoute] Guid id)
    {
        try
        {
            return eventService.Remove(id) ? NoContent() : NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
