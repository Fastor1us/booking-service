using BookingApi.Application.Dtos;
using BookingApi.Application.Interfaces;
using BookingApi.Presentation.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] CreateUserDto dto, CancellationToken ct)
    {
        await userService.RegisterAsync(dto, ct);

        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginUserDto dto, CancellationToken ct)
    {
        return Ok(new LoginResponseDto
        {
            Token = await userService.LoginAsync(dto, ct)
        });
    }
}
