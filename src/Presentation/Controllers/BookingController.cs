using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Booking>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        return Ok(await bookingService.GetBookingByIdAsync(id, ct));
    }
}
