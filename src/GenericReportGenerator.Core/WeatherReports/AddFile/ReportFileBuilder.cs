using ClosedXML.Excel;
using GenericReportGenerator.Infrastructure.WeatherReports.WeatherData;

namespace GenericReportGenerator.Core.WeatherReports.AddFile;

/// <summary>
/// Builder for weather report files from provided data.
/// </summary>
public class ReportFileBuilder
{
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
        var tableData = weatherData.Select(x => new
        {
            Date = x.Date.ToDateTime(TimeOnly.MinValue),
            Temperature = x.MaxTemperature
        });
        IXLTable table = worksheet.Cell("A3").InsertTable(tableData, createTable: true);

        // Style the table.
        table.Theme = XLTableTheme.TableStyleMedium9;
        worksheet.Columns().AdjustToContents();

        MemoryStream stream = new();
        workbook.SaveAs(stream);

        return stream;
    }
}
