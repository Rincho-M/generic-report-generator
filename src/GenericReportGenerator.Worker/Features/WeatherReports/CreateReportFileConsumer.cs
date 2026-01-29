using GenericReportGenerator.Core.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Infrastructure.Features.WeatherReports.ReportFiles;
using MassTransit;

namespace GenericReportGenerator.Worker.Features.WeatherReports;

/// <summary>
/// MassTransit message consumer for creating report files.
/// </summary>
public class CreateReportFileConsumer : IConsumer<CreateReportFileMessage>
{
    private readonly AddFileService _service;
    private readonly ILogger<CreateReportFileConsumer> _logger;

    public CreateReportFileConsumer(
        AddFileService service,
        ILogger<CreateReportFileConsumer> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateReportFileMessage> context)
    {
        _logger.LogInformation("Received {MessageType} for ReportId: {ReportId}", nameof(CreateReportFileMessage), context.Message.ReportId);

        await _service.AddFileToReport(context.Message.ReportId, context.CancellationToken);

        _logger.LogInformation("Processed {MessageType} for ReportId: {ReportId}", nameof(CreateReportFileMessage), context.Message.ReportId);
    }
}
