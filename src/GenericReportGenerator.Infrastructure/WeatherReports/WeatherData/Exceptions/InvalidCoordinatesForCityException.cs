namespace GenericReportGenerator.Infrastructure.WeatherReports.WeatherData.Exceptions;

public class InvalidCoordinatesForCityException(string cityName, double? latitude, double? longitude) : Exception(
    $"Invalid coordinates received for city '{cityName}'; Latitude: {latitude}, Longitude: {longitude}.");
