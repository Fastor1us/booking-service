using BookingApi.Application.Dtos;
using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Tests.Base;
using BookingApi.Presentation.Tests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingApi.Presentation.Tests;

public class AuthorizationTests : PostgreSqlBase
{
    private string? _adminToken;
    private string? _userToken;
    private string? _anotherUserToken;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _adminToken = await GetTokenAsync(UserRole.Admin, 0);
        _userToken = await GetTokenAsync(UserRole.User, 1);
        _anotherUserToken = await GetTokenAsync(UserRole.User, 2);
    }

    #region AllowAnonymousEndpointsTests
    [Fact]
    public async Task PublicEndpoints_AllowAnonymousAccess()
    {
        // Arrange && Act
        var getEventResponseDto = await Client.GetAsync("/api/events/" + Guid.NewGuid());
        var getEventsResponse = await Client.GetAsync("/api/events");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getEventResponseDto.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getEventsResponse.StatusCode);
    }
    #endregion

    #region ProtectedEndpointsWithoutTokenTests
    [Fact]
    public async Task ProtectedEndpoints_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var updateDto = EventFactory.Generate<UpdateEventDto>();

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/events", createEventDto);
        var updateResponse = await Client.PutAsJsonAsync("/api/events/" + Guid.NewGuid(), updateDto);
        var deleteResponse = await Client.DeleteAsync("/api/events/" + Guid.NewGuid());
        var bookResponse = await Client.PostAsync("/api/events/" + Guid.NewGuid() + "/book", null);
        var cancelResponse = await Client.DeleteAsync($"/api/bookings/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, bookResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, cancelResponse.StatusCode);
    }
    #endregion

    #region AdminEndpointsTests
    [Fact]
    public async Task AdminEndpoints_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var updateDto = EventFactory.Generate<UpdateEventDto>();

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/events", createEventDto);
        var updateResponse = await Client.PutAsJsonAsync("/api/events/" + Guid.NewGuid(), updateDto);
        var deleteResponse = await Client.DeleteAsync("/api/events/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoints_WithAdminToken_AllowsAccess()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var updateDto = EventFactory.Generate<UpdateEventDto>();

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/events", createEventDto);
        var createdEvent = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>(JsonOptions);
        var updateResponse = await Client.PutAsJsonAsync($"/api/events/{createdEvent!.Id}", updateDto);
        var deleteResponse = await Client.DeleteAsync($"/api/events/{createdEvent.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdEvent);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
    #endregion

    #region BookingEndpointsTests
    [Fact]
    public async Task BookingEndpoint_WithUserToken_AllowsBooking()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var createEventDto = EventFactory.Generate<CreateEventDto>();

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/events", createEventDto);
        var createdEvent = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        var bookResponse = await Client.PostAsync($"/api/events/{createdEvent!.Id}/book", null);
        var createdBooking = await bookResponse.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);

        // Assert
        Assert.NotNull(createdEvent);
        Assert.Equal(HttpStatusCode.Accepted, bookResponse.StatusCode);
        Assert.NotNull(createdBooking);
        Assert.NotEqual(Guid.Empty, createdBooking.Id);
    }
    #endregion

    #region CancelBookingRulesTests
    [Fact]
    public async Task CancelBooking_WithOwnBooking_ReturnsNoContent()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var createEventDto = EventFactory.Generate<CreateEventDto>();

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/events", createEventDto);
        var createdEvent = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        var bookResponse = await Client.PostAsync($"/api/events/{createdEvent!.Id}/book", null);
        var createdBooking = await bookResponse.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);
        var deleteResponse = await Client.DeleteAsync($"/api/bookings/{createdBooking!.Id}");

        // Assert
        Assert.NotNull(createdEvent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, bookResponse.StatusCode);
        Assert.NotNull(createdBooking);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CancelBooking_WithAnotherUserToken_ReturnsForbidden()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var createEventDto = EventFactory.Generate<CreateEventDto>();

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/events", createEventDto);
        var createdEvent = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        var bookResponse = await Client.PostAsync($"/api/events/{createdEvent!.Id}/book", null);
        var createdBooking = await bookResponse.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _anotherUserToken);
        var deleteResponse = await Client.DeleteAsync($"/api/bookings/{createdBooking!.Id}");

        // Assert
        Assert.NotNull(createdEvent);
        Assert.Equal(HttpStatusCode.Accepted, bookResponse.StatusCode);
        Assert.NotNull(createdBooking);
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CancelBooking_WithAdminToken_AllowsCancellingAnyBooking()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var createEventDto = EventFactory.Generate<CreateEventDto>();

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/events", createEventDto);
        var createdEvent = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        var bookResponse = await Client.PostAsync($"/api/events/{createdEvent!.Id}/book", null);
        var createdBooking = await bookResponse.Content.ReadFromJsonAsync<BookingResponseDto>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        var deleteResponse = await Client.DeleteAsync($"/api/bookings/{createdBooking!.Id}");

        // Assert
        Assert.NotNull(createdEvent);
        Assert.Equal(HttpStatusCode.Accepted, bookResponse.StatusCode);
        Assert.NotNull(createdBooking);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CancelBooking_NonExistentBooking_ReturnsNotFound()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        var nonExistentBookingId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/bookings/{nonExistentBookingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    #endregion

    #region Helper Classes
    private async Task<string> GetTokenAsync(UserRole role, int userIndex)
    {
        // Регистрируем пользователя
        var login = $"test_{role}_{userIndex}";
        var createUserDto = new { Login = login, Password = "password123", Role = role.ToString() };
        var response = await Client.PostAsJsonAsync("/api/auth/register", createUserDto);
        response.EnsureSuccessStatusCode();

        // Логинимся и получаем токен
        var loginUserDto = new { Login = login, Password = "password123" };
        response = await Client.PostAsJsonAsync("/api/auth/login", loginUserDto);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        return tokenResponse!.Token;
    }
    #endregion
}
