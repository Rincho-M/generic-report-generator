namespace GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

internal record CreateReportRequest
{
    /// <summary>
    /// City name to request a weather report from.
    /// </summary>
    public required string City { get; init; }

    /// <summary>
    /// Inclusive lower limit of the requested weather report period. 
    /// </summary>
    public required DateOnly FromDate { get; init; }

    /// <summary>
    /// Inclusive higher limit of the requested weather report period.
    /// </summary>
    public required DateOnly ToDate { get; init; }
}
