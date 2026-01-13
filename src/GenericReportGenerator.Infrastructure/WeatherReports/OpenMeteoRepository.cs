using Flurl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GenericReportGenerator.Infrastructure.WeatherReports;

/// <summary>
/// Repository that retrieves weather data from the OpenMeteo API.
/// </summary>
public class OpenMeteoRepository : IWeatherDataRepository
{
    private readonly HttpClient _httpClient;

    private readonly ILogger<OpenMeteoRepository> _logger;

    private readonly OpenMeteoOptions _config;

    private const string DateFormat = "yyyy-MM-dd";

    public OpenMeteoRepository(
        HttpClient httpClient, 
        ILogger<OpenMeteoRepository> logger,
        IOptions<OpenMeteoOptions> config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Get historic weather data for the specified city and date range.
    /// </summary>
    public async Task<IReadOnlyCollection<WeatherDataPoint>> GetWeatherHistory(
        string city, 
        DateOnly fromDate, 
        DateOnly toDate, 
        CancellationToken ct)
    {
        Coordinates cityCoordinates = await GetCityCoordinates(city, ct);
        IReadOnlyCollection<WeatherDataPoint> data = await GetWeatherHistory(cityCoordinates, fromDate, toDate, ct);

        return data;
    }

    /// <summary>
    /// Internal method to get city coordinates by city name from OpenMeteo API.
    /// </summary>
    private async Task<Coordinates> GetCityCoordinates(string city, CancellationToken ct)
    {
        string requestUrl = _config.GeocodingUrl.SetQueryParam("name", city);
        GeocodingResponse? response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(requestUrl, ct);
        GeocodingResult? result = response?.Results?.FirstOrDefault();

        if (result is null ||
            result.Name?.ToLower() != city.ToLower())
        {
            // TODO: custom excetion.
            throw new Exception("City '{City}' not found.");
        }
        if (result.Latitude is null || result.Longitude is null)
        {
            // TODO: custom excetion.
            throw new Exception($"Invalid coordinates received for city '{city}'; Latitude: {result.Latitude}, Longitude: {result.Longitude}.");
        }

        Coordinates cityCoordinates = new(result.Latitude.Value, result.Longitude.Value);

        return cityCoordinates;
    }

    /// <summary>
    /// Internal method to get historic weather data by coordinates from OpenMeteo API and convert it to usable format.
    /// </summary>
    private async Task<IReadOnlyCollection<WeatherDataPoint>> GetWeatherHistory(
        Coordinates cityCoordinates, 
        DateOnly fromDate, 
        DateOnly toDate, 
        CancellationToken ct)
    {
        string requestUrl = _config.ArchiveUrl
            .SetQueryParams(new
            {
                latitude = cityCoordinates.Latitude,
                longitude = cityCoordinates.Longitude,
                start_date = fromDate.ToString(DateFormat),
                end_date = toDate.ToString(DateFormat),
                daily = "temperature_2m_max",
                timezone = "auto"
            })
            .ToString();
        ArchiveResponse? response = await _httpClient.GetFromJsonAsync<ArchiveResponse>(requestUrl, ct);

        if (response?.Daily is null ||
            response.Daily.Time is null || 
            response.Daily.MaxTemp is null)
        {
            return Array.Empty<WeatherDataPoint>();
        }

        if (response.Daily.Time.Count != response.Daily.MaxTemp.Count)
        {
            // TODO: custom excetion.
            throw new Exception($"Received inconsistent weather data: time points count {response.Daily.Time.Count} does not match max temperature points count {response.Daily.MaxTemp.Count}.");
        }

        // Map weird response json structure to row-based objects.
        // OpenMeteo returns objects with arrays like: { time: [d1, d2], temp: [t1, t2] }.
        List<WeatherDataPoint> mappedData = new();
        for (int i = 0; i < response.Daily.Time.Count; i++)
        {
            if (DateOnly.TryParse(response.Daily.Time[i], out DateOnly date))
            {
                mappedData.Add(new WeatherDataPoint()
                {
                    Date = date,
                    MaxTemperature = response.Daily.MaxTemp[i],
                });
            }
        }

        return mappedData;
    }

    #region OpenMeteo responses
    private record GeocodingResponse
    {
        [JsonPropertyName("results")]
        public IReadOnlyCollection<GeocodingResult>? Results { get; init; }
    }

    private record GeocodingResult
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("latitude")] 
        public double? Latitude { get; init; }

        [JsonPropertyName("longitude")] 
        public double? Longitude { get; init; }
    }

    private record ArchiveResponse
    {
        [JsonPropertyName("daily")]
        public DailyWeatherData? Daily { get; init; }
    }

    private record DailyWeatherData
    {
        [JsonPropertyName("time")] 
        public IReadOnlyList<string>? Time { get; init; }

        [JsonPropertyName("temperature_2m_max")] 
        public IReadOnlyList<double>? MaxTemp { get; init; }
    }
    #endregion
}
