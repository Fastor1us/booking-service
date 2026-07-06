using BookingApi.Infrastructure.Data;
using BookingApi.Presentation.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[ApiController]
[Route("api/events")]
public class EventController(AppDbContext context) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

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
        throw new NotImplementedException();
    }

    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> Add(
        [FromBody] CreateEventDto eventDto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventResponseDto>> Put(
        [FromBody] UpdateEventDto eventDto,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id:guid}/book")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingResponseDto>> Book(
        [FromRoute] Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
