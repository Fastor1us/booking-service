using EventService.Application.Dtos;
using EventService.Application.Interfaces;
using EventService.Presentation.Dtos;
using EventService.Presentation.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/events")]
public class EventController(
    IEventService eventService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        var @event = await eventService.GetByIdAsync(id, ct);
        return Ok(@event.MapToResponseDto());
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedEventsResponseDto>> GetAll(
        [FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        PaginationParamsDto paginationParams = new(page, pageSize);
        EventFilterDto eventFilter = new(title, from, to);

        var @event = await eventService.GetAllAsync(eventFilter, paginationParams, ct);
        return Ok(@event.MapToPaginatedResponseDto(paginationParams));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> Add(
        [FromBody] CreateEventDto dto, CancellationToken ct)
    {
        var @event = await eventService.AddAsync(dto, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = @event.Id },
            @event.MapToResponseDto());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> Put(
        [FromRoute] Guid id,
        [FromBody] UpdateEventDto dto,
        CancellationToken ct)
    {
        await eventService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid id, CancellationToken ct)
    {
        await eventService.RemoveAsync(id, ct);
        return NoContent();
    }
}
