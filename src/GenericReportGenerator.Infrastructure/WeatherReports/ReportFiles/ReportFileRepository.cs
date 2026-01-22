using Microsoft.Extensions.Options;

namespace GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;

public class ReportFileRepository : IReportFileRepository
{
    private readonly ReportFilesOptions _config;

    private const string FolderName = "weather_reports";
    private const string ReportFileNameFormat = "{0}.xlsx";

    public ReportFileRepository(IOptions<ReportFilesOptions> config)
    {
        _config = config.Value;
    }

    public async Task<ReportFile> GetByReportId(Guid reportId)
    {
        string filePath = BuildFilePath(reportId);

        FileStream fileContent = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        ReportFile file = new()
        {
            RealFileName = Path.GetFileName(filePath),
            Content = fileContent
        };

        return file;
    }

    public async Task<string> Save(Guid reportId, Stream fileContent)
    {
        string filePath = BuildFilePath(reportId, createMissingDirectories: true);

        fileContent.Position = 0;
        using FileStream fileContentWithPath = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

        await fileContent.CopyToAsync(fileContentWithPath);

        return filePath;
    }

    private string BuildFilePath(Guid reportId, bool createMissingDirectories = false)
    {
        string fileName = string.Format(ReportFileNameFormat, reportId);
        string fileDirectory = Path.Combine(_config.BasePath, FolderName);

        if (createMissingDirectories &&
            !string.IsNullOrEmpty(fileDirectory) &&
            !Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

        string filePath = Path.Combine(fileDirectory, fileName);

        return filePath;
    }
}
