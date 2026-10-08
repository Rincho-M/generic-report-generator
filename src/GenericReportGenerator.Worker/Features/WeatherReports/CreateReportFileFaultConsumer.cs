using GenericReportGenerator.Core.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using MassTransit;

namespace GenericReportGenerator.Worker.Features.WeatherReports;

/// <summary>
/// Marks a report as Failed after MassTransit retries for creating the report file are exhausted.
/// </summary>
public class CreateReportFileFaultConsumer : IConsumer<Fault<CreateReportFileMessage>>
{
    private readonly ReportProcessingService _processingService;
    private readonly ILogger<CreateReportFileFaultConsumer> _logger;

    public CreateReportFileFaultConsumer(
        ReportProcessingService processingService,
        ILogger<CreateReportFileFaultConsumer> logger)
    {
        _processingService = processingService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Fault<CreateReportFileMessage>> context)
    {
        Guid reportId = context.Message.Message.ReportId;

        _logger.LogError("Retries exhausted for {MessageType}, ReportId: {ReportId}", nameof(CreateReportFileMessage), reportId);

        await _processingService.MarkFailed(reportId, context.CancellationToken);
    }
}
