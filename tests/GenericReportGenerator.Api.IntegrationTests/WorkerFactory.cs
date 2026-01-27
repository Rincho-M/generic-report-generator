extern alias WorkerAlias;

using System.Reflection;
using Microsoft.Extensions.Hosting;
using WorkerProgram = WorkerAlias::Program;

namespace GenericReportGenerator.Api.IntegrationTests;

/// <summary>
/// Provides an instance of Worker project for integration testing scenarios.
/// </summary>
public class WorkerFactory : IAsyncDisposable
{
    private IHost? _host;

    private Dictionary<string, string?> _configOverrides = new()
    {
        { "ConnectionStrings:Database", Setup.DbContainer.GetConnectionString() },
        { "ConnectionStrings:Redis", Setup.RedisContainer.GetConnectionString() },
        { "RabbitMq:Host", Setup.RabbitContainer.Hostname },
        { "RabbitMq:Port", Setup.RabbitContainer.GetMappedPublicPort(Setup.ApiConfiguration["RabbitMq:Port"]).ToString() },
    };

    public async Task Start()
    {
        // Convert config dictionary to command line arguments.
        // This overrides appsettings.json similar to environment variables.
        List<string> configArgs = _configOverrides
            .Select(kvp => $"--{kvp.Key}={kvp.Value}")
            .ToList();
        configArgs.Add(WorkerProgram.TestModeArg);

        await ExecuteMain(configArgs.ToArray());
        _host = WorkerProgram.TestHost ?? throw new NullReferenceException("Worker host is null after it's Main() was executed.");

        await _host.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
        }
    }

    /// <summary>
    /// Executes the Main method of the Worker Program.cs to get a built host.
    /// </summary>
    private async Task ExecuteMain(string[] args)
    {
        Type programType = typeof(WorkerProgram);
        MethodInfo? entryPoint = programType.Assembly.EntryPoint;

        if (entryPoint is not null)
        {
            object? result = entryPoint.Invoke(null, new object[] { args });

            if (result is Task task)
            {
                await task;
            }
        }
    }
}
