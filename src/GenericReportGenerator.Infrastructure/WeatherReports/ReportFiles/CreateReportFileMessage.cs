namespace GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;

/// <summary>
/// Message for mass transit that requests the creation of a report file for a specified weather report.
/// </summary>
public record CreateReportFileMessage
{
    public required Guid ReportId { get; init; }
}
