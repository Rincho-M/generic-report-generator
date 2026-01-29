using GenericReportGenerator.Infrastructure.WeatherReports.WeatherData;

namespace GenericReportGenerator.UnitTests.WeatherReports;

public class ReportFileBuilderTestCases
{
    public static object[] ValidDiverseData =
    {
        new object[]
        {
            Guid.Parse("019c03ca-d526-79f1-be83-dd04259ff333"),
            null!,
            DateOnly.Parse("2023-01-01"),
            DateOnly.Parse("2024-02-29"),
            new List<WeatherDataPoint>
            {
                new() { Date = new DateOnly(2023, 01, 01), MaxTemperature = 15.5 },
            }
        },
        new object[]
        {
            Guid.Parse("019c03ca-d526-79f1-be83-dd04259ff333"),
            "New York San-Francisco München",
            DateOnly.MinValue,
            DateOnly.MaxValue,
            new List<WeatherDataPoint>
            {
                new() { Date = new DateOnly(120, 07, 01), MaxTemperature = -5.0 },
                new() { Date = new DateOnly(1523, 04, 02), MaxTemperature = 7.2 },
                new() { Date = new DateOnly(9023, 01, 03), MaxTemperature = -6.8 },
                new() { Date = new DateOnly(1520, 07, 04), MaxTemperature = 4.5 },
                new() { Date = new DateOnly(9623, 01, 05), MaxTemperature = -3.0 },
                new() { Date = new DateOnly(1523, 01, 06), MaxTemperature = 2.5 },
                new() { Date = new DateOnly(1020, 04, 07), MaxTemperature = -1.0 },
            }
        },
        new object[]
        {
            Guid.Parse("019c03ca-d526-79f1-be83-dd04259ff333"),
            "CityCityCityCityCityCityCityCityCityCityCityCityCityCityCityCityCityCityCityCity",
            DateOnly.MaxValue,
            DateOnly.MinValue,
            new List<WeatherDataPoint>
            {
                new() { Date = new DateOnly(2000, 01, 01), MaxTemperature =  9.99999999999999E+307 },
                new() { Date = new DateOnly(2000, 01, 01), MaxTemperature = -9.99999999999999E+307 },
            }
        },
        new object[]
        {
            default(Guid), string.Empty, default(DateOnly), default(DateOnly), new List<WeatherDataPoint>()
        },
    };

    public static object[] InvalidTemperature =
    {
        new object[]
        {
            default(Guid), string.Empty, default(DateOnly), default(DateOnly),
            new List<WeatherDataPoint>() { new() { Date = default, MaxTemperature = Math.BitIncrement(9.99999999999999E+307) } }
        },
        new object[]
        {
            default(Guid), string.Empty, default(DateOnly), default(DateOnly),
            new List<WeatherDataPoint>() { new() { Date = default, MaxTemperature = Math.BitDecrement(-9.99999999999999E+307) } }
        },
        new object[]
        {
            default(Guid), string.Empty, default(DateOnly), default(DateOnly),
            new List<WeatherDataPoint>() { new() { Date = default, MaxTemperature = double.PositiveInfinity } }
        },
        new object[]
        {
            default(Guid), string.Empty, default(DateOnly), default(DateOnly),
            new List<WeatherDataPoint>() { new() { Date = default, MaxTemperature = double.NegativeInfinity } }
        },
        new object[]
        {
            default(Guid), string.Empty, default(DateOnly), default(DateOnly),
            new List<WeatherDataPoint>() { new() { Date = default, MaxTemperature = double.NaN } }
        },
    };

    public static object[] NullWeatherData =
    {
        new object[]
        {
            default(Guid), string.Empty, default(DateOnly), default(DateOnly), null!
        },
    };
}
