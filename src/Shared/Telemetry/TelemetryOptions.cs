namespace Telemetry;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; init; } = string.Empty;
    public string ServiceVersion { get; init; } = "1.0.0";
}
