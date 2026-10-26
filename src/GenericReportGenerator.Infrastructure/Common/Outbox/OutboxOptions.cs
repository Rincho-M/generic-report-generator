namespace GenericReportGenerator.Infrastructure.Common.Outbox;

/// <summary>
/// Container for config section of the outbox dispatcher. Part of Options pattern.
/// </summary>
public record OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>
    /// Delay between dispatcher poll cycles.
    /// </summary>
    public required TimeSpan PollingInterval { get; init; }

    /// <summary>
    /// Maximum number of pending outbox rows to load per poll.
    /// </summary>
    public required int BatchSize { get; init; }
}
