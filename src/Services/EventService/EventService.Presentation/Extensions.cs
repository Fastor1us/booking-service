using Domain.Exceptions;
using Messaging.Kafka;
using Security.Jwt;
using System.Text.Json.Serialization;

namespace EventService.Presentation;

public static class Extensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        var configuration = services
            .BuildServiceProvider()
            .GetRequiredService<IConfiguration>();

        services.AddJwtAuthentication(configuration);

        services.Configure<KafkaOptions>(options =>
        {
            options.BootstrapServers = configuration
                .GetConnectionString("Kafka")
                ?? throw new InvalidOperationException(
                    "Kafka connection string is not configured. " +
                    "Expected ConnectionStrings:Kafka (injected by Aspire via WithReference).");
        });

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
