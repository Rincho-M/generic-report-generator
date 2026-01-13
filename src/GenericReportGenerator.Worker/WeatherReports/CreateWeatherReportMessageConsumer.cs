using GenericReportGenerator.Core.WeatherReports;
using GenericReportGenerator.Infrastructure.WeatherReports;
using MassTransit;

namespace GenericReportGenerator.Worker.WeatherReports;

internal class CreateWeatherReportMessageConsumer : IConsumer<CreateWeatherReportMessage>
{
    private readonly WeatherReportService _service;
    private readonly ILogger<CreateWeatherReportMessageConsumer> _logger;

    public CreateWeatherReportMessageConsumer(
        WeatherReportService service,
        ILogger<CreateWeatherReportMessageConsumer> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateWeatherReportMessage> context)
    {
        await _service.CreateReport(context.Message.Id, context.CancellationToken);
    }
}
