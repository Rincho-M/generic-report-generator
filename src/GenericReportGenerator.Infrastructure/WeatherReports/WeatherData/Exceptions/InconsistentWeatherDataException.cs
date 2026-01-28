namespace GenericReportGenerator.Infrastructure.WeatherReports.WeatherData.Exceptions;

public class InconsistentWeatherDataException(int timePointsCount, int temperaturePointsCount) : Exception(
    $"Received inconsistent weather data. Time points count: {timePointsCount}, doesn't match temperature points count: {temperaturePointsCount}.");