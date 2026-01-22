namespace GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;

public record ReportFile
{
    public required string RealFileName { get; set; }

    public string? ReadableFileName { get; set; }

    public required Stream Content { get; init; }
}
