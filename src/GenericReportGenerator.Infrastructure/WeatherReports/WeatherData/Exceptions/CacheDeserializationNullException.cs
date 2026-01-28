namespace GenericReportGenerator.Infrastructure.WeatherReports.WeatherData.Exceptions;

public class CacheDeserializationNullException(string cacheKey) : Exception(
    $"Cached data with key '{cacheKey}' is null after deserialization.");
