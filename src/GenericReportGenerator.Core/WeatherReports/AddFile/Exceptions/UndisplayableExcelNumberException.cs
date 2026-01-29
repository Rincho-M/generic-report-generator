namespace GenericReportGenerator.Core.WeatherReports.AddFile.Exceptions;

public class UndisplayableExcelNumberException(double number) : Exception(
    $"Attempt to write number: '{number}' that cannot be displayed in Excel cell.");
