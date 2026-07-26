using System.Text.Json.Serialization;
using BookingApi.Presentation.Application.Interfaces;
using BookingApi.Presentation.Application.Services;
using BookingApi.Presentation.Domain.Exceptions;
using BookingApi.Presentation.Infrastructure.BackgroundServices;
using BookingApi.Presentation.Infrastructure.Data;
using BookingApi.Presentation.Infrastructure.Repositories;
using BookingApi.Presentation.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Presentation;

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
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is required");

            options.UseNpgsql(connectionString);
            options.UseLazyLoadingProxies();
        });

        services.AddScoped<IEventRepository, EFCoreEventRepository>();
        services.AddScoped<IBookingRepository, EFCoreBookingRepository>();
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

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
