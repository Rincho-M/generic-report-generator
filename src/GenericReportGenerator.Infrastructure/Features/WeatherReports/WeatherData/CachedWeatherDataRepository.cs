using System.Text.Json;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData;

/// <summary>
/// Decorator that adds caching functionality for <see cref="IWeatherDataRepository"/> implementations.
/// </summary>
public class CachedWeatherDataRepository : IWeatherDataRepository
{
    private readonly IWeatherDataRepository _innerRepository;

    private readonly IDistributedCache _cache;

    private readonly ILogger<CachedWeatherDataRepository> _logger;

    private const string cacheKeyFormat = "weather:{0}:{1:O}:{2:O}";

    public CachedWeatherDataRepository(
        IWeatherDataRepository innerRepository,
        IDistributedCache cache,
        ILogger<CachedWeatherDataRepository> logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<WeatherDataPoint>> GetWeatherHistory(
        string city, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        string cacheKey = string.Format(cacheKeyFormat, city.ToLowerInvariant(), fromDate, toDate);
        string? cachedDataRaw = await _cache.GetStringAsync(cacheKey, ct);

        // Cache hit.
        if (!string.IsNullOrEmpty(cachedDataRaw))
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);

            WeatherDataPoint[]? cachedData = JsonSerializer.Deserialize<WeatherDataPoint[]>(cachedDataRaw);
            if (cachedData is null)
            {
                throw new CacheDeserializationNullException(cacheKey);
            }

            return cachedData;
        }
        // Cache miss.
        else
        {
            _logger.LogInformation("Cache miss for {CacheKey}", cacheKey);

            IReadOnlyCollection<WeatherDataPoint> data = await _innerRepository.GetWeatherHistory(city, fromDate, toDate, ct);

            if (data.Any())
            {
                DistributedCacheEntryOptions options = new()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };

                string serializedData = JsonSerializer.Serialize(data);
                await _cache.SetStringAsync(cacheKey, serializedData, options, ct);
            }

            return data;
        }
    }
}
