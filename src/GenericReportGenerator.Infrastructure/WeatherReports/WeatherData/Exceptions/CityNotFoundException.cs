using GenericReportGenerator.Infrastructure.Common.Exceptions;

namespace GenericReportGenerator.Infrastructure.WeatherReports.WeatherData.Exceptions;

public class CityNotFoundException(string cityName) : DomainException(
    $"City '{cityName}' not found.");
