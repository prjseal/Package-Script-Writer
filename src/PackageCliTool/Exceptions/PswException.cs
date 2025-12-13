namespace PackageCliTool.Exceptions;

/// <summary>
/// Base exception for all PSW-specific exceptions
/// </summary>
public class PswException : Exception
{
    /// <summary>Gets the error code for this exception</summary>
    public string ErrorCode { get; }

    /// <summary>Gets the user-friendly error message</summary>
    public string? UserMessage { get; }

    /// <summary>Gets the suggestion for resolving this error</summary>
    public string? Suggestion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PswException"/> class
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The exception message</param>
    /// <param name="userMessage">The user-friendly message</param>
    /// <param name="suggestion">The suggestion for resolving the error</param>
    public PswException(string errorCode, string message, string? userMessage = null, string? suggestion = null)
        : base(message)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage ?? message;
        Suggestion = suggestion;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PswException"/> class with an inner exception
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="userMessage">The user-friendly message</param>
    /// <param name="suggestion">The suggestion for resolving the error</param>
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
    /// <summary>Gets the HTTP status code if available</summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="suggestion">The suggestion for resolving the error</param>
    public ApiException(string message, int? statusCode = null, string? suggestion = null)
        : base("PSW-API-001", message, message, suggestion)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class with an inner exception
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="suggestion">The suggestion for resolving the error</param>
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
    /// <summary>Gets the name of the field that failed validation</summary>
    public string? FieldName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="fieldName">The name of the field that failed validation</param>
    /// <param name="suggestion">The suggestion for resolving the error</param>
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
    /// <summary>Gets the script exit code if available</summary>
    public int? ExitCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptExecutionException"/> class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="exitCode">The script exit code</param>
    /// <param name="suggestion">The suggestion for resolving the error</param>
    public ScriptExecutionException(string message, int? exitCode = null, string? suggestion = null)
        : base("PSW-EXEC-001", message, message, suggestion)
    {
        ExitCode = exitCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptExecutionException"/> class with an inner exception
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="exitCode">The script exit code</param>
    /// <param name="suggestion">The suggestion for resolving the error</param>
    public ScriptExecutionException(string message, Exception innerException, int? exitCode = null, string? suggestion = null)
        : base("PSW-EXEC-001", message, innerException, message, suggestion)
    {
        ExitCode = exitCode;
    }
}
