using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Telemetry;

public static class Extensions
{
    public static void AddTelemetry(
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
                .AddService(options.ServiceName, options.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    // Задаём имя нашего сервиса в системе мониторинга
                    //.SetResourceBuilder(
                    //    ResourceBuilder
                    //        .CreateDefault()
                    //        .AddService("OtelDemoWebApi"))
                    // Автоматически собираем все входящие HTTP-запросы к нашему API
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Исключаем системные запросы из трейсинга
                        options.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path;

                            // Если запрос идёт на /health или /metrics, спан НЕ создаётся
                            return !path.StartsWithSegments("/health") &&
                                   !path.StartsWithSegments("/metrics");
                        };
                    })
                    // Автоматически отслеживаем все исходящие HTTP-вызовы через HttpClient
                    //.AddHttpClientInstrumentation()
                    // Выводим результаты в консоль приложения для тестирования
                    //.AddConsoleExporter();
                    // Настраиваем экспорт по протоколу OTLP
                    .AddOtlpExporter(options =>
                    {
                        // Указываем адрес, где запущен Jaeger
                        //options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                        // Выбираем протокол (http или gRPC)
                        //options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        // Настройка пакетного процессора
                        // Отправка каждые 2 секунды (5 base)
                        options.BatchExportProcessorOptions
                            .ScheduledDelayMilliseconds = 2000;
                        // Тайм-аут ответа 3 секунды (30 base)
                        options.BatchExportProcessorOptions
                            .ExporterTimeoutMilliseconds = 3000;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()    // Метрики HTTP-запросов
                    .AddRuntimeInstrumentation()       // Метрики среды выполнения (CPU, память, GC)
                    .AddPrometheusExporter();          // Экспорт в формате Prometheus
            })
            .WithLogging();
    }
}
