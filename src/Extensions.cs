using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Models;
using BookingApi.Domain.Models.Dtos;

namespace BookingApi;

public static class Extensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEventService, EventService>();

        return services;
    }

    public static Event MapToEvent(this PostEventDto @event)
    {
        return new Event
        {
            Id = Guid.Empty,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }

    public static Event MapToEvent(this PutEventDto @event, Guid id)
    {
        return new Event
        {
            Id = id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }

    public static EventResponseDto MapToResponseDto(this Event @event)
    {
        return new EventResponseDto
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }
}
