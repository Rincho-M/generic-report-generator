using ClosedXML.Excel;
using GenericReportGenerator.Core.WeatherReports.AddFile.Exceptions;
using GenericReportGenerator.Infrastructure.WeatherReports.WeatherData;

namespace GenericReportGenerator.Core.WeatherReports.AddFile;

/// <summary>
/// Builder for weather report files from provided data.
/// </summary>
public class ReportFileBuilder
{
    // Excel has a maximum number limit for numeric values, positive or negative.
    // It will fail to display numbers beyond this limit.
    private const double _excelNumberLimit = 9.99999999999999E+307;

    public Stream Build(Guid id, string city, DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<WeatherDataPoint> weatherData)
    {
        using XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Weather History");

        // Add a title.
        IXLCell titleCell = worksheet.Cell("A1");
        titleCell.Value = $"Weather Report for {city}";
        titleCell.Style.Font.FontSize = 16;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontColor = XLColor.DarkBlue;

        // Insert weather data.
        var tableData = weatherData.Select(dataPoint => new
        {
            Date = dataPoint.Date.ToDateTime(TimeOnly.MinValue),
            Temperature = CheckBounds(dataPoint.MaxTemperature)
        });
        IXLTable table = worksheet.Cell("A3").InsertTable(tableData, createTable: true);

        // Style the table.
        table.Theme = XLTableTheme.TableStyleMedium9;
        worksheet.Columns().AdjustToContents();

        MemoryStream stream = new();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }

    private double CheckBounds(double value)
    {
        if (Math.Abs(value) > _excelNumberLimit ||
            double.IsInfinity(value) ||
            double.IsNaN(value))
        {
            throw new UndisplayableExcelNumberException(value);
        }
        return value;
    }
}
