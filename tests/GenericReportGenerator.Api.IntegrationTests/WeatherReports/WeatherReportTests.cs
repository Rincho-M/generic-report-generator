using FluentAssertions;
using GenericReportGenerator.Api.Features.WeatherReports.CreateReport;
using GenericReportGenerator.Api.Features.WeatherReports.GetReport;
using GenericReportGenerator.Infrastructure.WeatherReports;
using GenericReportGenerator.Infrastructure.WeatherReports.ReportFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace GenericReportGenerator.Api.IntegrationTests.WeatherReports;

public class WeatherReportTests : BaseTest
{
    private Report _exampleReport = new()
    {
        City = "London",
        FromDate = DateOnly.Parse("2023-01-01"),
        ToDate = DateOnly.Parse("2023-01-02"),
        Status = ReportStatus.Pending,
    };

    private Report? _createdReport;

    [Test, Order(0)]
    public async Task CreateReport()
    {
        // Arrange.
        var requestData = new
        {
            _exampleReport.City,
            _exampleReport.FromDate,
            _exampleReport.ToDate,
        };
        string endpointRoute = "/api/weather-report";

        // Act.
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(endpointRoute, requestData);

        // Assert.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        CreateReportResponse? responseContent = await response.Content.ReadFromJsonAsync<CreateReportResponse>();
        responseContent.Should().NotBeNull();
        responseContent.ReportId.Should().NotBe(default(Guid));
        responseContent.ReportStatus.Should().Be(_exampleReport.Status);

        Report? reportInDb = await DbContext.WeatherReports.SingleOrDefaultAsync(report => report.Id == responseContent.ReportId);
        reportInDb.Should().BeEquivalentTo(_exampleReport, options => options
            .Excluding(report => report.Id)
            .Excluding(report => report.CreatedAt));
        reportInDb.Id.Should().Be(responseContent.ReportId);
        reportInDb.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));

        _createdReport = reportInDb;
    }


    [Test, Order(1)]
    public async Task GetReport()
    {
        // Arrange.
        _createdReport.Should().NotBeNull("CreateReport test must run before GetReport test.");

        string endpointRoute = $"/api/weather-report/{_createdReport.Id}";

        // Act.
        HttpResponseMessage response = await HttpClient.GetAsync(endpointRoute);

        // Assert.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        GetReportResponse? responseContent = await response.Content.ReadFromJsonAsync<GetReportResponse>();
        responseContent.Should().NotBeNull();
        responseContent.Id.Should().Be(_createdReport.Id);
        responseContent.Status.Should().Be(_createdReport.Status);
        responseContent.City.Should().Be(_createdReport.City);
        responseContent.FromDate.Should().Be(_createdReport.FromDate);
        responseContent.ToDate.Should().Be(_createdReport.ToDate);
        responseContent.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    // TODO: add worker execution before running this test.
    [Test, Order(2)]
    public async Task GetReportFile()
    {
        //// Arrange.
        //_createdReport.Should().NotBeNull("CreateReport test must run before GetReportFile test.");

        //IReportFileRepository repo = ServiceScope.ServiceProvider.GetRequiredService<IReportFileRepository>();
        //ReportFile expectedFile = await repo.GetByReportId(_createdReport.Id);
        //using MemoryStream memoryStream = new();
        //await expectedFile.Content.CopyToAsync(memoryStream);
        //byte[] expectedFileContent = memoryStream.ToArray();

        //string endpointRoute = $"/api/weather-report/{_createdReport.Id}/file";

        //// Act.
        //HttpResponseMessage response = await HttpClient.GetAsync(endpointRoute);

        //// Assert.
        //response.StatusCode.Should().Be(HttpStatusCode.OK);

        //response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        //byte[] actualContent = await response.Content.ReadAsByteArrayAsync();
        //actualContent.Should().Equal(expectedFileContent);

        //ContentDispositionHeaderValue? disposition = response.Content.Headers.ContentDisposition;
        //disposition.Should().NotBeNull();

        //string expectedFileName = $"WeatherReport_{_createdReport.City}_{_createdReport.FromDate:yyyyMMdd}_{_createdReport.ToDate:yyyyMMdd}.xlsx";
        //disposition.FileName.Should().Be(expectedFileName);
    }
}
