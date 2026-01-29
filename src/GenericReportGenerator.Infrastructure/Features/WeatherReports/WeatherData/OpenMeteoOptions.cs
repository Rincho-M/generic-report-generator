namespace GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData;

/// <summary>
/// Container for config section of OpenMeteo integration. Part of Options pattern.
/// </summary>
public record OpenMeteoOptions
{
    public const string SectionName = "OpenMeteo";

    /// <summary>
    /// Archive data API URL.
    /// Required for getting historical weather data.
    /// </summary>
    public required string ArchiveUrl { get; init; }

    /// <summary>
    /// Geocoding API URL.
    /// Required for converting location names into coordinates for weather lookup.
    /// </summary>
    public required string GeocodingUrl { get; init; }
}
