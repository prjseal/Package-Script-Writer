namespace PackageCliTool.Exceptions;

/// <summary>
/// Base exception for all PSW-specific exceptions
/// </summary>
public class PswException : Exception
{
    public string ErrorCode { get; }
    public string? UserMessage { get; }
    public string? Suggestion { get; }

    public PswException(string errorCode, string message, string? userMessage = null, string? suggestion = null)
        : base(message)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage ?? message;
        Suggestion = suggestion;
    }

    public PswException(string errorCode, string message, Exception innerException, string? userMessage = null, string? suggestion = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage ?? message;
        Suggestion = suggestion;
    }
}

/// <summary>
/// Exception thrown when API calls fail
/// </summary>
public class ApiException : PswException
{
    public int? StatusCode { get; }

    public ApiException(string message, int? statusCode = null, string? suggestion = null)
        : base("PSW-API-001", message, message, suggestion)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message, Exception innerException, int? statusCode = null, string? suggestion = null)
        : base("PSW-API-001", message, innerException, message, suggestion)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : PswException
{
    public string? FieldName { get; }

    public ValidationException(string message, string? fieldName = null, string? suggestion = null)
        : base("PSW-VAL-001", message, message, suggestion)
    {
        FieldName = fieldName;
    }
}

/// <summary>
/// Exception thrown when script execution fails
/// </summary>
public class ScriptExecutionException : PswException
{
    public int? ExitCode { get; }

    public ScriptExecutionException(string message, int? exitCode = null, string? suggestion = null)
        : base("PSW-EXEC-001", message, message, suggestion)
    {
        ExitCode = exitCode;
    }

    public ScriptExecutionException(string message, Exception innerException, int? exitCode = null, string? suggestion = null)
        : base("PSW-EXEC-001", message, innerException, message, suggestion)
    {
        ExitCode = exitCode;
    }
}
