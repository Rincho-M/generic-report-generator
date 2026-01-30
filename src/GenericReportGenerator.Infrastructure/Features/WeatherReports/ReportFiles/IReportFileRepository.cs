namespace GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;

/// <summary>
/// Repository to manage weather report files.
/// </summary>
public interface IReportFileRepository
{
    Task<string> Save(Guid reportId, Stream reportFile);

    Task<ReportFile> GetByReportId(Guid reportId);
}
