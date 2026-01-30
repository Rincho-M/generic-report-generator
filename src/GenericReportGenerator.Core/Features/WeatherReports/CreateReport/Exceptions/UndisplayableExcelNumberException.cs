namespace GenericReportGenerator.Core.Features.WeatherReports.CreateReport.Exceptions;

public class UndisplayableExcelNumberException(double number) : Exception(
    $"Attempt to write number: '{number}' that cannot be displayed in Excel cell.");
