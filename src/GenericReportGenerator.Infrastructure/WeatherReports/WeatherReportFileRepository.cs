using Microsoft.Extensions.Options;

namespace GenericReportGenerator.Infrastructure.WeatherReports;

public class WeatherReportFileRepository : IWeatherReportFileRepository
{
    private readonly ReportFilesOptions _config;

    private const string FolderName = "weather_reports";
    private const string ReportFileNameFormat = "{0}.xlsx";

    public WeatherReportFileRepository(IOptions<ReportFilesOptions> config)
    {
        _config = config.Value;
    }

    public async Task<string> Save(Guid reportId, Stream reportFile)
    {
        string fileName = string.Format(ReportFileNameFormat, reportId);
        string fileDirectory = Path.Combine(_config.BasePath, FolderName);
        if (!string.IsNullOrEmpty(fileDirectory) && !Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }
        string filePath = Path.Combine(fileDirectory, fileName);

        reportFile.Position = 0;
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

        await reportFile.CopyToAsync(fileStream);

        return filePath;
    }
}
