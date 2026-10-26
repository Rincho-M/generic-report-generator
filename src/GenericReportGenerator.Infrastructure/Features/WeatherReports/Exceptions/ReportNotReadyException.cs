using GenericReportGenerator.Infrastructure.Common.Exceptions;

namespace GenericReportGenerator.Infrastructure.Features.WeatherReports.Exceptions;

public class ReportNotReadyException(Guid reportId, ReportStatus status) : DomainException(
    $"Report '{reportId}' is not ready for download. Current status: {status}.");
