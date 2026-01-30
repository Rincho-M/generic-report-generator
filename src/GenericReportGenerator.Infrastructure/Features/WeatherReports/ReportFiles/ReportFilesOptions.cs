namespace GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;

public record ReportFilesOptions
{
    public const string SectionName = "ReportFiles";

    public required string BasePath { get; init; }

    public required string WeatherReportsPath { get; init; }
}