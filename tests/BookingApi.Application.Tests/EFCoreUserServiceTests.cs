using BookingApi.Application.Tests.Helpers;
using BookingApi.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookingApi.Application.Tests;

public class EFCoreUserServiceTests : EFCoreServiceTestsBase
{
    [Fact]
    public async Task RegisterAsync_ValidCreateUserDto_ReturnsCorrectUser()
    {
        // Arrange
        var createUserDto = UserFactory.GenerateCreateDto();

        // Act
        var user = await _userService.RegisterAsync(createUserDto, _ct);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(createUserDto.Login, user.Login);
        Assert.Equal(createUserDto.Role, user.Role);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var createUserDto = UserFactory.GenerateCreateDto();
        var user = await _userService.RegisterAsync(createUserDto, _ct);
        var loginUserDto = UserFactory.GenerateLoginDto();

        // Act
        var token = await _userService.LoginAsync(loginUserDto, _ct);
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        Assert.NotNull(token);
        Assert.Equal(createUserDto.Login, jwtToken.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value);
        Assert.Equal(createUserDto.Role.ToString(), jwtToken.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)!.Value);
    }

    [Fact]
    public async Task LoginAsync_NonExistentLogin_ReturnsToken()
    {
        // Arrange
        var loginUserDto = UserFactory.GenerateLoginDto();
        var expectedException = new UserNotFoundException(loginUserDto.Login);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _userService.LoginAsync(loginUserDto, _ct));

        // Assert
        Assert.IsType<UserNotFoundException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task LoginAsync_IncorrectPassword_ReturnsToken()
    {
        // Arrange
        var createUserDto = UserFactory.GenerateCreateDto();
        var user = await _userService.RegisterAsync(createUserDto, _ct);
        var loginUserDto = UserFactory.GenerateLoginDto(
            password: "IncorrectPassword");
        var expectedException = new UserIncorrectPasswordException();

        // Act
        var exception = await Record.ExceptionAsync(
            () => _userService.LoginAsync(loginUserDto, _ct));

        // Assert
        Assert.IsType<UserIncorrectPasswordException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }
}
