using Microsoft.Extensions.Logging;
using Spectre.Console;
using TextCopy;

namespace PackageCliTool.Services;

/// <summary>
/// Helper class for clipboard operations
/// </summary>
public static class ClipboardHelper
{
    /// <summary>
    /// Copies text to clipboard and displays a success message
    /// </summary>
    public static async Task CopyToClipboardAsync(string text, ILogger? logger = null)
    {
        try
        {
            await ClipboardService.SetTextAsync(text);
            AnsiConsole.MarkupLine("[green]✓ Script copied to clipboard successfully![/]");
            logger?.LogInformation("Script copied to clipboard");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to copy script to clipboard");
            AnsiConsole.MarkupLine("[yellow]⚠ Failed to copy to clipboard: {0}[/]", ex.Message);
            AnsiConsole.MarkupLine("[dim]You can manually copy the script from the output above.[/]");
        }
    }
}
