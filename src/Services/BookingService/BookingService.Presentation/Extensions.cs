using Domain.Exceptions;
using Messaging.Kafka;
using Security.Jwt;
using System.Text.Json.Serialization;

namespace BookingService.Presentation;

public static class Extensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        var configuration = services
            .BuildServiceProvider()
            .GetRequiredService<IConfiguration>();

        services.AddJwtAuthentication(configuration);

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

        services.AddJwtSwagger();

        return services;
    }
}
