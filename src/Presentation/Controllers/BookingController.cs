using BookingApi.Application.Interfaces;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        var booking = await bookingService.GetBookingByIdAsync(id, ct);
        return Ok(booking.MapToResponseDto());
    }
}
