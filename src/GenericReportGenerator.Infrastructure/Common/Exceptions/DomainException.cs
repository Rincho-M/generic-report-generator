namespace GenericReportGenerator.Infrastructure.Common.Exceptions;

/// <summary>
/// Base class for all custom domain exceptions. 
/// Every domain exception should inherit from this class.
/// </summary>
public abstract class DomainException(string message) : Exception(message);
