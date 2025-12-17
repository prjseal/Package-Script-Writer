using Microsoft.Extensions.Logging;
using PackageCliTool.Exceptions;
using Spectre.Console;

namespace PackageCliTool.Logging;

/// <summary>
/// Handles errors and displays user-friendly messages
/// </summary>
public static class ErrorHandler
{
    /// <summary>
    /// Handles an exception and displays appropriate user message
    /// </summary>
    public static void Handle(Exception ex, ILogger? logger = null, bool showStackTrace = false)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // Log the exception
        logger?.LogError(ex, "Error occurred. Correlation ID: {CorrelationId}", correlationId);

        // Display user-friendly message based on exception type
        if (ex is PswException pswEx)
        {
            HandlePswException(pswEx, correlationId, showStackTrace);
        }
        else if (ex is HttpRequestException httpEx)
        {
            HandleHttpException(httpEx, correlationId, showStackTrace);
        }
        else if (ex is TimeoutException)
        {
            HandleTimeoutException(ex, correlationId, showStackTrace);
        }
        else if (ex is UnauthorizedAccessException)
        {
            HandleUnauthorizedAccessException(ex, correlationId, showStackTrace);
        }
        else if (ex is IOException ioEx)
        {
            HandleIOException(ioEx, correlationId, showStackTrace);
        }
        else
        {
            HandleUnexpectedException(ex, correlationId, showStackTrace);
        }
    }

    private static void HandlePswException(PswException ex, string correlationId, bool showStackTrace)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[red]✗ {ex.UserMessage}[/]");

        if (!string.IsNullOrWhiteSpace(ex.Suggestion))
        {
            AnsiConsole.MarkupLine($"[yellow]💡 Suggestion:[/] {ex.Suggestion}");
        }

        AnsiConsole.MarkupLine($"[dim]Error Code: {ex.ErrorCode} | Correlation ID: {correlationId}[/]");

        if (showStackTrace && ex.InnerException != null)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
        }
    }

    private static void HandleHttpException(HttpRequestException ex, string correlationId, bool showStackTrace)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]✗ Failed to connect to the API[/]");
        AnsiConsole.MarkupLine($"[yellow]Reason:[/] {ex.Message}");
        AnsiConsole.MarkupLine("[yellow]💡 Suggestion:[/] Check your internet connection and verify the API is accessible");
        AnsiConsole.MarkupLine($"[dim]Error Code: PSW-NET-001 | Correlation ID: {correlationId}[/]");

        if (showStackTrace)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
        }
    }

    private static void HandleTimeoutException(Exception ex, string correlationId, bool showStackTrace)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]✗ Operation timed out[/]");
        AnsiConsole.MarkupLine("[yellow]💡 Suggestion:[/] The operation took too long. Try again or check your network connection");
        AnsiConsole.MarkupLine($"[dim]Error Code: PSW-TIMEOUT-001 | Correlation ID: {correlationId}[/]");

        if (showStackTrace)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
        }
    }

    private static void HandleUnauthorizedAccessException(Exception ex, string correlationId, bool showStackTrace)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]✗ Permission denied[/]");
        AnsiConsole.MarkupLine($"[yellow]Reason:[/] {ex.Message}");
        AnsiConsole.MarkupLine("[yellow]💡 Suggestion:[/] Check file/directory permissions or try running with appropriate access rights");
        AnsiConsole.MarkupLine($"[dim]Error Code: PSW-PERM-001 | Correlation ID: {correlationId}[/]");

        if (showStackTrace)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
        }
    }

    private static void HandleIOException(IOException ex, string correlationId, bool showStackTrace)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]✗ File system error[/]");
        AnsiConsole.MarkupLine($"[yellow]Reason:[/] {ex.Message}");
        AnsiConsole.MarkupLine("[yellow]💡 Suggestion:[/] Verify the file path exists and you have appropriate permissions");
        AnsiConsole.MarkupLine($"[dim]Error Code: PSW-IO-001 | Correlation ID: {correlationId}[/]");

        if (showStackTrace)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
        }
    }

    private static void HandleUnexpectedException(Exception ex, string correlationId, bool showStackTrace)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[red]✗ An unexpected error occurred[/]");
        AnsiConsole.MarkupLine($"[yellow]Reason:[/] {ex.Message}");
        AnsiConsole.MarkupLine("[yellow]💡 Suggestion:[/] This is an unexpected error. Please report this issue with the correlation ID");
        AnsiConsole.MarkupLine($"[dim]Error Code: PSW-UNK-001 | Correlation ID: {correlationId}[/]");

        if (showStackTrace)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(ex);
        }
    }

    /// <summary>
    /// Displays a warning message
    /// </summary>
    public static void Warning(string message, ILogger? logger = null)
    {
        logger?.LogWarning(message);
        AnsiConsole.MarkupLine($"[yellow]⚠ {message.EscapeMarkup()}[/]");
    }
}
