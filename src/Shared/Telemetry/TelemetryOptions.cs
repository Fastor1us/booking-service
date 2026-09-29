namespace Telemetry;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public required string ServiceName { get; init; }
    public string ServiceVersion { get; init; } = "1.0.0";
    public required string OtlpEndpoint { get; init; }
}
