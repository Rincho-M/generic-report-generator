using GenericReportGenerator.Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection;

namespace GenericReportGenerator.IntegrationTests;

/// <summary>
/// Base class for all integration tests. 
/// Provides common useful fields and setup/teardown logic that executes after each test case.
/// </summary>
public abstract class BaseTest
{
    protected ApiFactory AppFactory;

    protected WorkerFactory WorkerFactory;

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
            return field ?? throw new NullReferenceException();
        }
    }
    private bool _isDbContextValid = false;

    [SetUp]
    public async Task Setup()
    {
        AppFactory = new ApiFactory();
        HttpClient = AppFactory.CreateClient();
        ServiceScope = AppFactory.Services.CreateScope();

        WorkerFactory = new WorkerFactory();
        await WorkerFactory.Start();
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

        await WorkerFactory.DisposeAsync();
    }
}
