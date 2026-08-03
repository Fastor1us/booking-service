using BookingApi.Application.Interfaces;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        var booking = await bookingService.GetByIdAsync(id, ct);
        return Ok(booking.MapToResponseDto());
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id, CancellationToken ct)
    {
        await bookingService.Cancel(id, User.Identity!.Name!, ct);

        return NoContent();
    }
}
