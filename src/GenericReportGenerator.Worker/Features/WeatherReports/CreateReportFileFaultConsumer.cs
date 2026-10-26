using GenericReportGenerator.Core.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using MassTransit;

namespace GenericReportGenerator.Worker.Features.WeatherReports;

/// <summary>
/// Marks a report as Failed when report-file creation faults.
/// A domain exception faults immediately. A system exception faults after retries are exhausted.
/// </summary>
public class CreateReportFileFaultConsumer : IConsumer<Fault<CreateReportFileMessage>>
{
    private readonly ReportProcessingService _processingService;

    public CreateReportFileFaultConsumer(ReportProcessingService processingService)
    {
        _processingService = processingService;
    }

    public async Task Consume(ConsumeContext<Fault<CreateReportFileMessage>> context)
    {
        Guid reportId = context.Message.Message.ReportId;

        await _processingService.MarkFailed(reportId, context.CancellationToken);
    }
}
