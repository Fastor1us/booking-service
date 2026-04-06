using EventApi.Interfaces;
using EventApi.Models;
using EventApi.Services;

namespace EventApi;

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

    public static EventResponseDto MapToResponseDto(this Event item)
    {
        return new EventResponseDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            StartAt = item.StartAt,
            EndAt = item.EndAt
        };
    }
}
