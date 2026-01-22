using GenericReportGenerator.Infrastructure.Common.Exceptions;

namespace GenericReportGenerator.Infrastructure.WeatherReports.WeatherData.Exceptions;

public class InconsistentWeatherDataException(int timePointsCount, int temperaturePointsCount) : DomainException(
    $"Received inconsistent weather data. Time points count: {timePointsCount}, doesn't match temperature points count: {temperaturePointsCount}.");