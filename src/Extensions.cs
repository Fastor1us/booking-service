using System.Text.Json.Serialization;
using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Exceptions;
using BookingApi.Infrastructure.BackgroundServices;
using BookingApi.Infrastructure.Repositories;

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
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IBookingService, BookingService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventRepository, EventInMemoryRepository>();
        services.AddSingleton<IBookingRepository, BookingInMemoryRepository>();

        services.AddHostedService<PendingBookingProcessor>();

        return services;
    }

    public static void LogInfo(this ILogger logger, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(message, args);
    }

    public static void LogInfo(this ILogger logger, Exception? exception, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(exception, message, args);
    }
}
