using BookingService.Application.Interfaces;
using BookingService.Presentation.Dtos;
using BookingService.Presentation.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookingResponseDto>> Create(
        [FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var booking = await bookingService.AddAsync(request.EventId, userId, ct);

        return AcceptedAtAction(
            nameof(GetById),
            new { id = booking.Id },
            booking.MapToResponseDto());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        var booking = await bookingService.GetByIdAsync(id, ct);
        return Ok(booking.MapToResponseDto());
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id, CancellationToken ct)
    {
        await bookingService.CancelAsync(id, User.Identity!.Name!, ct);

        return NoContent();
    }
}
