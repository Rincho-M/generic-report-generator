using GenericReportGenerator.Infrastructure.Common.Exceptions;
using MassTransit;

namespace GenericReportGenerator.Worker.ExceptionHandling;

public class GlobalExceptionFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<GlobalExceptionFilter<T>> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter<T>> logger)
    {
        _logger = logger;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("ExceptionLogger");
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception exception)
        {
            using (_logger.BeginScope(new Dictionary<string, object?> { { "MessageId", context.MessageId } }))
            {

                if (exception is DomainException domainException)
                {
                    _logger.LogWarning(domainException, "Domain error: {ErrorDetails}", domainException.Message);
                }
                else
                {
                    _logger.LogError(exception, "System error: {ErrorDetails}", exception.Message);
                }
            }

            throw;
        }
    }
}
