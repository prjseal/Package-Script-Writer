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

            // Initialize services
            var apiClient = new ApiClient(ApiConfiguration.ApiBaseUrl, logger);
            var packageSelector = new PackageSelector(apiClient, logger);
            var scriptExecutor = new ScriptExecutor(logger);

            // Determine if we should use CLI mode or interactive mode
            bool useCLIMode = options.HasAnyOptions();

            if (useCLIMode)
            {
                var cliWorkflow = new CliModeWorkflow(apiClient, scriptExecutor, logger);
                await cliWorkflow.RunAsync(options);
            }
            else
            {
                var interactiveWorkflow = new InteractiveModeWorkflow(
                    apiClient,
                    packageSelector,
                    scriptExecutor,
                    logger);
                await interactiveWorkflow.RunAsync();
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
