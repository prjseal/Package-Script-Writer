using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Exceptions;

namespace PackageCliTool.Services;

/// <summary>
/// Service for executing generated installation scripts
/// </summary>
public class ScriptExecutor
{
    private readonly ILogger? _logger;

    public ScriptExecutor(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes the script content in the specified directory
    /// </summary>
    public async Task RunScriptAsync(string scriptContent, string workingDirectory)
    {
        _logger?.LogInformation("Executing script in directory: {Directory}", workingDirectory);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold blue]Running script in:[/] {workingDirectory}");
        AnsiConsole.WriteLine();

        // Determine shell for script execution
        string shell;
        if (OperatingSystem.IsWindows())
        {
            shell = "cmd.exe";
        }
        else
        {
            shell = "/bin/bash";
        }

        _logger?.LogDebug("Using shell: {Shell}", shell);

        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = shell,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger?.LogDebug("Script output: {Output}", e.Data);
                AnsiConsole.MarkupLine($"[dim]{e.Data.EscapeMarkup()}[/]");
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger?.LogWarning("Script error output: {Error}", e.Data);
                AnsiConsole.MarkupLine($"[red]{e.Data.EscapeMarkup()}[/]");
            }
        };

        process.Start();

        // Write the script content to stdin
        await process.StandardInput.WriteAsync(scriptContent);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
        {
            _logger?.LogInformation("Script executed successfully with exit code 0");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]✓ Script executed successfully![/]");
        }
        else
        {
            _logger?.LogWarning("Script exited with non-zero code: {ExitCode}", process.ExitCode);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]⚠ Script exited with code {process.ExitCode}[/]");

            throw new ScriptExecutionException(
                $"Script execution failed with exit code {process.ExitCode}",
                process.ExitCode,
                "Check the script output above for error details"
            );
        }
    }
}
