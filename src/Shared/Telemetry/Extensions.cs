using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Telemetry;

public static class Extensions
{
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(TelemetryOptions.SectionName);
        var options = section.Get<TelemetryOptions>()
            ?? throw new InvalidOperationException(
                $"Section '{TelemetryOptions.SectionName}' is missing");

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: options.ServiceName,
                    serviceVersion: options.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    // Automaticly collect all incoming HTTP-request
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Exclude system requests from tracking
                        options.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path;

                            // To prevent creating spans at /metrics requests
                            return !path.StartsWithSegments("/health") &&
                                   !path.StartsWithSegments("/metrics");
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(o => o.Endpoint =
                        new Uri(options.OtlpEndpoint));
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()    // HTTP-request metrics
                    .AddRuntimeInstrumentation()       // CPU, RAM and GC metrics
                    .AddPrometheusExporter();
            });

        services.AddHealthChecks();

        return services;
    }

    public static IEndpointRouteBuilder MapTelemetry(
        this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapHealthChecks("/health");

        endpointRouteBuilder.MapPrometheusScrapingEndpoint();

        return endpointRouteBuilder;
    }
}
