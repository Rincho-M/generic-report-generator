using ClosedXML.Excel;
using FluentAssertions;
using FluentAssertions.Execution;
using GenericReportGenerator.Core.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Core.Features.WeatherReports.CreateReport.Exceptions;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.WeatherData;
using static GenericReportGenerator.UnitTests.WeatherReports.ReportFileBuilderTestCases;

namespace GenericReportGenerator.UnitTests.WeatherReports;

public class ReportFileBuilderTests
{
    private ReportFileBuilder _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new ReportFileBuilder();
    }

    [TestCaseSource(typeof(ReportFileBuilderTestCases), nameof(ValidDiverseData))]
    public void Build_ShouldReturn_CorrectStream(
        Guid id, string city, DateOnly from, DateOnly to, List<WeatherDataPoint> weatherData)
    {
        // Act
        Stream stream = _sut.Build(id, city, from, to, weatherData);

        // Assert
        using (new AssertionScope())
        {
            stream.Should().NotBeNull();
            stream.CanRead.Should().BeTrue();
            stream.Position.Should().Be(0);
            stream.Length.Should().BeGreaterThan(0);
        }
    }

    [TestCaseSource(typeof(ReportFileBuilderTestCases), nameof(ValidDiverseData))]
    public void Build_ShouldCreate_WorksheetWithCorrectText(
        Guid id, string city, DateOnly from, DateOnly to, List<WeatherDataPoint> weatherData)
    {
        // Act
        using Stream stream = _sut.Build(id, city, from, to, weatherData);
        using XLWorkbook workbook = new(stream);

        // Assert
        using (new AssertionScope())
        {
            workbook.Worksheets.Should().ContainSingle();
            IXLWorksheet sheet = workbook.Worksheets.Single();
            sheet.Name.Should().Be("Weather History");

            sheet.Cell("A1").Value.ToString().Should().Be(string.Format("Weather Report for {0}", city));

            IXLTable? table = sheet.Tables.FirstOrDefault();
            table.Should().NotBeNull();
            table!.Field(0).Name.Should().Be("Date");
            table.Field(1).Name.Should().Be("Temperature");
        }
    }

    [TestCaseSource(typeof(ReportFileBuilderTestCases), nameof(ValidDiverseData))]
    public void Build_ShouldInsert_CorrectNumberOfRowsAndDataValues(
        Guid id, string city, DateOnly from, DateOnly to, List<WeatherDataPoint> weatherData)
    {
        // Act
        using Stream stream = _sut.Build(id, city, from, to, weatherData);
        using XLWorkbook workbook = new(stream);

        // Assert
        using (new AssertionScope())
        {
            IXLTable table = workbook.Worksheets.Single().Table(0);

            // Empty tables have row count of 1 and tables with one row also have row count of 1.
            int rowsCount = weatherData.Count == 0 ? 1 : table.DataRange.Rows().Count();
            table.DataRange.Rows().Count().Should().Be(rowsCount);

            int rowIndex = 1;
            foreach (WeatherDataPoint point in weatherData)
            {
                IXLTableRow row = table.DataRange.Row(rowIndex);

                XLCellValue dateCell = row.Cell(1).Value;
                ((DateTime)dateCell).Should().Be(point.Date.ToDateTime(TimeOnly.MinValue));

                XLCellValue tempCell = row.Cell(2).Value;
                ((double)tempCell).Should().Be(point.MaxTemperature);

                rowIndex++;
            }
        }
    }

    [TestCaseSource(typeof(ReportFileBuilderTestCases), nameof(NullWeatherData))]
    public void Build_ShouldThrow_WhenWeatherDataIsNull(
        Guid id, string city, DateOnly from, DateOnly to, List<WeatherDataPoint> weatherData)
    {
        // Act
        Action act = () => _sut.Build(id, city, from, to, weatherData);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestCaseSource(typeof(ReportFileBuilderTestCases), nameof(InvalidTemperature))]
    public void Build_ShouldThrow_WhenTemperatureIsInvalid(
        Guid id, string city, DateOnly from, DateOnly to, List<WeatherDataPoint> weatherData)
    {
        // Act
        Action act = () => _sut.Build(id, city, from, to, weatherData);

        // Assert
        act.Should().Throw<UndisplayableExcelNumberException>();
    }
}
