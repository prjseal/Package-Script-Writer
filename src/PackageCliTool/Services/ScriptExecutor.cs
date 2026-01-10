using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Exceptions;
using PackageCliTool.Validation;
using System.Diagnostics;

namespace PackageCliTool.Services;

/// <summary>
/// Service for executing generated installation scripts
/// </summary>
public class ScriptExecutor
{
    private readonly ILogger? _logger;
    private readonly CommandValidator _commandValidator;
    private readonly bool _skipValidation;
    private static readonly List<Process> _activeProcesses = new();
    private static readonly object _processLock = new();
    private static bool _cleanupHandlersRegistered = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptExecutor"/> class
    /// </summary>
    /// <param name="logger">Optional logger instance</param>
    /// <param name="skipValidation">Whether to skip command validation (default: false, not recommended)</param>
    public ScriptExecutor(ILogger? logger = null, bool skipValidation = false)
    {
        _logger = logger;
        _commandValidator = new CommandValidator(logger);
        _skipValidation = skipValidation;

        if (skipValidation)
        {
            _logger?.LogWarning("Command validation is DISABLED - scripts will execute without safety checks");
        }

        // Register cleanup handlers on first instance
        RegisterCleanupHandlers();
    }

    /// <summary>
    /// Registers handlers to clean up processes when the application exits
    /// </summary>
    private void RegisterCleanupHandlers()
    {
        lock (_processLock)
        {
            if (_cleanupHandlersRegistered)
            {
                return;
            }

            // Handle Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                _logger?.LogInformation("Ctrl+C detected, cleaning up processes...");
                CleanupAllProcesses();
            };

            // Handle application exit
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                _logger?.LogInformation("Application exiting, cleaning up processes...");
                CleanupAllProcesses();
            };

            _cleanupHandlersRegistered = true;
            _logger?.LogDebug("Process cleanup handlers registered");
        }
    }

    /// <summary>
    /// Cleans up all active processes and their children
    /// </summary>
    private static void CleanupAllProcesses()
    {
        lock (_processLock)
        {
            foreach (var process in _activeProcesses.ToList())
            {
                try
                {
                    if (!process.HasExited)
                    {
                        KillProcessAndChildren(process);
                    }
                }
                catch
                {
                    // Process may have already exited
                }
            }
            _activeProcesses.Clear();
        }
    }

    /// <summary>
    /// Kills a process and all its child processes
    /// </summary>
    private static void KillProcessAndChildren(Process process)
    {
        try
        {
            if (process.HasExited)
            {
                return;
            }

            // On Unix systems, kill the entire process group
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    // Send SIGTERM to the process group
                    var killProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"pkill -P {process.Id}; kill {process.Id}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    killProcess.Start();
                    killProcess.WaitForExit(1000);
                }
                catch
                {
                    // Fallback to direct kill
                    process.Kill(entireProcessTree: true);
                }
            }
            else
            {
                // On Windows, kill the process tree
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Process may have already exited
        }
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

        // Validate script commands against allowlist (security check)
        var isWindows = OperatingSystem.IsWindows();
        if (!_skipValidation)
        {
            _logger?.LogInformation("Validating script commands against allowlist...");
            var (isValid, errors) = _commandValidator.ValidateScript(scriptContent, isWindows);

            if (!isValid)
            {
                AnsiConsole.MarkupLine("[red]✗ Script validation failed - dangerous commands detected:[/]");
                AnsiConsole.WriteLine();

                foreach (var error in errors)
                {
                    AnsiConsole.MarkupLine($"[red]  • {error.EscapeMarkup()}[/]");
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[yellow]The script contains commands that are not in the allowlist.[/]");
                AnsiConsole.MarkupLine("[yellow]This is a security measure to prevent execution of potentially dangerous commands.[/]");

                throw new PswException(
                    "Script validation failed - script contains disallowed commands",
                    string.Join(Environment.NewLine, errors)
                );
            }

            _logger?.LogInformation("Script validation passed");
            AnsiConsole.MarkupLine("[green]✓ Script validation passed[/]");
            AnsiConsole.WriteLine();
        }

        // Filter script content to handle comment lines
        var filteredScript = FilterScriptContent(scriptContent);

        // Determine shell for script execution
        string shell = isWindows ? "cmd.exe" : "/bin/bash";
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
            if (e.Data != null)
            {
                _logger?.LogDebug("Script output: {Output}", e.Data);
                // Use Console.WriteLine to preserve ANSI color codes from the process
                Console.WriteLine(e.Data);
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

        // Track the process for cleanup
        lock (_processLock)
        {
            _activeProcesses.Add(process);
        }

        try
        {
            // Write the filtered script content to stdin
            // For Windows cmd.exe, prepend @echo off to suppress command echoing
            if (isWindows)
            {
                await process.StandardInput.WriteLineAsync("@echo off");
            }
            await process.StandardInput.WriteAsync(filteredScript);
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
        finally
        {
            // Remove from active processes and clean up
            lock (_processLock)
            {
                _activeProcesses.Remove(process);
            }

            // Ensure any child processes are cleaned up
            if (!process.HasExited)
            {
                try
                {
                    KillProcessAndChildren(process);
                }
                catch
                {
                    // Process may have already exited
                }
            }

            process.Dispose();
        }
    }

    /// <summary>
    /// Filters script content to remove comment lines and display them separately
    /// </summary>
    private string FilterScriptContent(string scriptContent)
    {
        var lines = scriptContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var filteredLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.TrimStart();

            // Check if line is a comment (starts with #)
            if (trimmedLine.StartsWith('#'))
            {
                // Display comment to console but don't execute it
                _logger?.LogDebug("Skipping comment line: {Line}", line);
                AnsiConsole.MarkupLine($"[grey]{line.EscapeMarkup()}[/]");
            }
            else
            {
                // Include non-comment lines in the filtered script
                filteredLines.Add(line);
            }
        }

        return string.Join(Environment.NewLine, filteredLines);
    }
}
