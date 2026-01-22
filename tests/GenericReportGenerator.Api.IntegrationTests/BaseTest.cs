using GenericReportGenerator.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GenericReportGenerator.Api.IntegrationTests;

/// <summary>
/// Base class for all integration tests.
/// </summary>
public class BaseTest
{
    protected AppFactory AppFactory;

    protected HttpClient HttpClient;

    protected IServiceScope ServiceScope;

    protected AppDbContext DbContext 
    { 
        get
        { 
            if (!_isDbContextValid)
            {
                _isDbContextValid = true;
                field = ServiceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            }
            return field;
        }
    }
    private bool _isDbContextValid = false;

    [SetUp]
    public void Setup()
    {
        AppFactory = new AppFactory();
        HttpClient = AppFactory.CreateClient();
        ServiceScope = AppFactory.Services.CreateScope();
    }

    [TearDown]
    public async Task TearDown()
    {
        await AppFactory.DisposeAsync();
        HttpClient.Dispose();
        ServiceScope.Dispose();

        if (_isDbContextValid)
        {
            await DbContext.DisposeAsync();
            _isDbContextValid = false;
        }
    }
}
