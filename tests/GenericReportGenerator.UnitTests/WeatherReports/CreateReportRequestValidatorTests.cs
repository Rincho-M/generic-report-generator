using FluentValidation.TestHelper;
using GenericReportGenerator.Api.Features.WeatherReports.CreateReport;

namespace GenericReportGenerator.UnitTests.WeatherReports;

public class CreateReportRequestValidatorTests
{
    private CreateReportRequestValidator _validator;
    private CreateReportRequest _validModel;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateReportRequestValidator();
        _validModel = new CreateReportRequest
        {
            City = "TestCity",
            FromDate = new DateOnly(2020, 1, 1),
            ToDate = new DateOnly(2020, 1, 5)
        };
    }

    [Test]
    public void ShouldFail_WhenCityIsEmpty()
    {
        CreateReportRequest model = _validModel with { City = "" };
        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void ShouldFail_WhenCityIsNull()
    {
        CreateReportRequest model = _validModel with { City = null! };
        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void ShouldSuccess_WhenCityIsValid()
    {
        CreateReportRequest model = _validModel with { City = "London" };
        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void ShouldFail_WhenCityExceedsMaxLength()
    {
        string longCity = new('A', 101);
        CreateReportRequest model = _validModel with { City = longCity };

        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void ShouldSuccess_WhenCityIsExactlyMaxLength()
    {
        string city = new('A', 100);
        CreateReportRequest model = _validModel with { City = city };

        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void ShouldFail_WhenFromDateIsAfterToDate()
    {
        CreateReportRequest model = _validModel with
        {
            FromDate = new DateOnly(2023, 1, 5),
            ToDate = new DateOnly(2023, 1, 1)
        };

        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.FromDate);
    }

    [Test]
    public void ShouldSuccess_WhenDatesAreEqual()
    {
        CreateReportRequest model = _validModel with
        {
            FromDate = new DateOnly(2023, 1, 1),
            ToDate = new DateOnly(2023, 1, 1)
        };

        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.FromDate);
    }

    [Test]
    public void ShouldFail_WhenToDateIsInFuture()
    {
        DateOnly tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        CreateReportRequest model = _validModel with { ToDate = tomorrow };

        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ToDate);
    }

    [Test]
    public void ShouldSuccess_WhenToDateIsToday()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        CreateReportRequest model = _validModel with { ToDate = today };

        TestValidationResult<CreateReportRequest> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ToDate);
    }
}