using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Logging;
using PackageCliTool.Exceptions;
using PackageCliTool.Services;
using PackageCliTool.Workflows;
using PackageCliTool.UI;
using PackageCliTool.Configuration;
using Microsoft.Extensions.Logging;

namespace PackageCliTool;

/// <summary>
/// Main program class for the Package Script Writer CLI tool
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        ILogger? logger = null;

        try
        {
            // Parse command-line arguments
            var options = CommandLineOptions.Parse(args);

            // Initialize logging (verbose mode based on environment or flag)
            var verboseMode = Environment.GetEnvironmentVariable("PSW_VERBOSE") == "1" || options.VerboseMode;
            LoggerSetup.Initialize(verboseMode, enableFileLogging: true);
            logger = LoggerSetup.CreateLogger("Program");

            logger.LogInformation("PSW CLI started with {ArgCount} arguments", args.Length);

            // Handle help flag
            if (options.ShowHelp)
            {
                ConsoleDisplay.DisplayHelp();
                return;
            }

            // Handle version flag
            if (options.ShowVersion)
            {
                ConsoleDisplay.DisplayVersion();
                return;
            }

            // Initialize cache service (1-hour TTL, disabled if --no-cache is set)
            var cacheService = new CacheService(ttlHours: 1, enabled: !options.NoCache, logger: logger);

            // Handle clear cache flag
            if (options.ClearCache)
            {
                cacheService.Clear();
                AnsiConsole.MarkupLine("[green]✓ Cache cleared successfully[/]");

                // If only clearing cache, exit
                if (!options.HasAnyOptions() && !options.IsTemplateCommand() && !options.IsHistoryCommand())
                {
                    return;
                }
            }

            // Initialize services
            var apiClient = new ApiClient(ApiConfiguration.ApiBaseUrl, logger, cacheService);
            var packageSelector = new PackageSelector(apiClient, logger);
            var scriptExecutor = new ScriptExecutor(logger);
            var templateService = new TemplateService(logger: logger);
            var historyService = new HistoryService(logger: logger);

            // Check if this is a history command
            if (options.IsHistoryCommand())
            {
                var historyWorkflow = new HistoryWorkflow(historyService, apiClient, scriptExecutor, logger);
                await historyWorkflow.RunAsync(options);
            }
            // Check if this is a template command
            else if (options.IsTemplateCommand())
            {
                var templateWorkflow = new TemplateWorkflow(templateService, apiClient, scriptExecutor, logger);
                await templateWorkflow.RunAsync(options);
            }
            // Determine if we should use CLI mode or interactive mode
            else if (options.HasAnyOptions())
            {
                var cliWorkflow = new CliModeWorkflow(apiClient, scriptExecutor, logger);
                await cliWorkflow.RunAsync(options);
            }
            else
            {
                // Interactive mode with Ctrl+C restart support
                var keepRunning = true;
                while (keepRunning)
                {
                    try
                    {
                        var interactiveWorkflow = new InteractiveModeWorkflow(
                            apiClient,
                            packageSelector,
                            scriptExecutor,
                            logger);
                        await interactiveWorkflow.RunAsync();
                        keepRunning = false; // Exit loop on normal completion
                    }
                    catch (OperationCanceledException)
                    {
                        // User pressed Ctrl+C - restart the workflow
                        logger?.LogInformation("User pressed Ctrl+C, restarting interactive mode");
                        AnsiConsole.Clear();
                        AnsiConsole.MarkupLine("\n[yellow]↻ Restarting...[/]\n");
                        await Task.Delay(500); // Brief pause before restart
                    }
                }
            }

            // Display completion message
            AnsiConsole.MarkupLine("\n[green]✓ Process completed successfully![/]");
            logger.LogInformation("PSW CLI completed successfully");
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, logger, showStackTrace: logger != null);
            Environment.ExitCode = 1;
        }
        finally
        {
            LoggerSetup.Shutdown();
        }
    }
}
