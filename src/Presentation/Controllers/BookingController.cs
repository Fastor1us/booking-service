using BookingApi.Infrastructure.Data;
using BookingApi.Presentation.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingResponseDto>> GetById(
        [FromRoute] Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
