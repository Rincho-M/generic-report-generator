namespace GenericReportGenerator.Api.ExceptionHandling;

// All possible error responses from this API.
// ------------------------------------------

// Not used by itself.
public abstract record BaseErrorResponse(string Name, string TraceId);

// Used for: 422 (Domain) and 500 (System).
public record DetailErrorResponse(string Name, string TraceId, string Details)
    : BaseErrorResponse(Name, TraceId);

// Used for: 400 (Validation).
public record ValidationErrorResponse(string Name, string TraceId, IDictionary<string, string[]> Errors)
    : BaseErrorResponse(Name, TraceId);
