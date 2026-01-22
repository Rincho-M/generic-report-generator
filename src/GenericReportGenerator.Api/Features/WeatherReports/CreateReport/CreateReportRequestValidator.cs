using FluentValidation;

namespace GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

public class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
{
    public CreateReportRequestValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate);

        RuleFor(x => x.ToDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
