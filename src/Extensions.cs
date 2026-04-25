using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Presentation.Dtos;

namespace BookingApi;

public static class Extensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var errorMessage = "One or more validation errors occurred.";
                    throw new ModelValidationException(errorMessage, errors);
                };
            });
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEventService, EventService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IEventRepository, EventInMemoryRepository>();

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
