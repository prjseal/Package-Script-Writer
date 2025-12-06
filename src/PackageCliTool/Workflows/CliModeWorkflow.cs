using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Validation;
using PackageCliTool.Logging;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates the CLI mode workflow (when command-line flags are used)
/// </summary>
public class CliModeWorkflow
{
    private readonly ApiClient _apiClient;
    private readonly ScriptExecutor _scriptExecutor;
    private readonly ILogger? _logger;

    public CliModeWorkflow(
        ApiClient apiClient,
        ScriptExecutor scriptExecutor,
        ILogger? logger = null)
    {
        _apiClient = apiClient;
        _scriptExecutor = scriptExecutor;
        _logger = logger;
    }

    /// <summary>
    /// Runs the CLI mode workflow
    /// </summary>
    public async Task RunAsync(CommandLineOptions options)
    {
        if (options.UseDefault)
        {
            await GenerateDefaultScriptAsync(options);
        }
        else
        {
            await GenerateCustomScriptFromOptionsAsync(options);
        }
    }

    /// <summary>
    /// Generates a default script with minimal configuration
    /// </summary>
    private async Task GenerateDefaultScriptAsync(CommandLineOptions options)
    {
        _logger?.LogInformation("Generating default script");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Generating Default Script[/]\n");
        AnsiConsole.MarkupLine("[dim]Using default configuration (latest stable Umbraco with clean starter kit)[/]");
        AnsiConsole.WriteLine();

        // Create default script model matching website defaults
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = "", // Latest stable
            ProjectName = "MyProject",
            CreateSolutionFile = true,
            SolutionName = "MySolution",
            IncludeStarterKit = true,
            StarterKitPackage = "clean",
            IncludeDockerfile = false,
            IncludeDockerCompose = false,
            CanIncludeDocker = false,
            UseUnattendedInstall = true,
            DatabaseType = "SQLite",
            UserEmail = "admin@example.com",
            UserPassword = "1234567890",
            UserFriendlyName = "Administrator",
            OnelinerOutput = false,
            RemoveComments = false
        };

        var script = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Generating default installation script...", async ctx =>
            {
                return await _apiClient.GenerateScriptAsync(new ScriptRequest { Model = model });
            });

        _logger?.LogInformation("Default script generated successfully");

        ConsoleDisplay.DisplayGeneratedScript(script, "Generated Default Installation Script");

        // Handle auto-run or interactive run
        await HandleScriptExecutionAsync(script, options);
    }

    /// <summary>
    /// Generates a custom script from command-line options
    /// </summary>
    private async Task GenerateCustomScriptFromOptionsAsync(CommandLineOptions options)
    {
        _logger?.LogInformation("Generating custom script from command-line options");

        var projectName = options.ProjectName ?? "MyUmbracoProject";

        // Validate inputs
        InputValidator.ValidateProjectName(projectName);
        InputValidator.ValidateSolutionName(options.SolutionName);
        InputValidator.ValidateEmail(options.AdminEmail);
        InputValidator.ValidatePassword(options.AdminPassword);
        InputValidator.ValidateDatabaseType(options.DatabaseType);
        InputValidator.ValidateConnectionString(options.ConnectionString, options.DatabaseType);

        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = options.TemplateVersion ?? "",
            ProjectName = projectName,
            CreateSolutionFile = options.CreateSolution,
            SolutionName = options.SolutionName,
            IncludeStarterKit = options.IncludeStarterKit,
            StarterKitPackage = options.StarterKitPackage,
            IncludeDockerfile = options.IncludeDockerfile,
            IncludeDockerCompose = options.IncludeDockerCompose,
            CanIncludeDocker = options.IncludeDockerfile || options.IncludeDockerCompose,
            UseUnattendedInstall = options.UseUnattended,
            DatabaseType = options.DatabaseType,
            ConnectionString = options.ConnectionString,
            UserFriendlyName = options.AdminName,
            UserEmail = options.AdminEmail,
            UserPassword = options.AdminPassword,
            OnelinerOutput = options.OnelinerOutput,
            RemoveComments = options.RemoveComments
        };

        // Handle packages
        if (!string.IsNullOrWhiteSpace(options.Packages))
        {
            _logger?.LogDebug("Processing packages: {Packages}", options.Packages);

            var packageEntries = options.Packages.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            if (packageEntries.Count > 0)
            {
                var processedPackages = new List<string>();

                foreach (var entry in packageEntries)
                {
                    // Check if version is specified with pipe character (e.g., "uSync|17.0.0")
                    if (entry.Contains('|'))
                    {
                        var parts = entry.Split('|', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            var packageName = parts[0].Trim();
                            var version = parts[1].Trim();

                            // Validate package name and version
                            InputValidator.ValidatePackageName(packageName);
                            InputValidator.ValidateVersion(version);

                            processedPackages.Add($"{packageName}|{version}");
                            AnsiConsole.MarkupLine($"[green]✓[/] Using {packageName} version {version}");
                            _logger?.LogDebug("Added package {Package} with version {Version}", packageName, version);
                        }
                        else
                        {
                            ErrorHandler.Warning($"Invalid package format: {entry}, skipping...", _logger);
                        }
                    }
                    else
                    {
                        // No version specified, use package name without version
                        var packageName = entry.Trim();
                        InputValidator.ValidatePackageName(packageName);

                        processedPackages.Add(packageName);
                        AnsiConsole.MarkupLine($"[green]✓[/] Using {packageName} (latest version)");
                        _logger?.LogDebug("Added package {Package} with latest version", packageName);
                    }
                }

                // Build packages string - can be mixed format: "Package1|Version1,Package2,Package3|Version3"
                if (processedPackages.Count > 0)
                {
                    model.Packages = string.Join(",", processedPackages);
                }
            }
        }

        // Generate the script
        _logger?.LogInformation("Generating installation script via API");

        var script = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Generating installation script...", async ctx =>
            {
                return await _apiClient.GenerateScriptAsync(new ScriptRequest { Model = model });
            });

        _logger?.LogInformation("Script generated successfully");

        ConsoleDisplay.DisplayGeneratedScript(script);

        // Handle auto-run or interactive run
        await HandleScriptExecutionAsync(script, options);
    }

    /// <summary>
    /// Handles script execution based on options
    /// </summary>
    private async Task HandleScriptExecutionAsync(string script, CommandLineOptions options)
    {
        if (options.AutoRun || !string.IsNullOrWhiteSpace(options.RunDirectory))
        {
            var targetDir = options.RunDirectory ?? Directory.GetCurrentDirectory();

            // Validate and expand path
            InputValidator.ValidateDirectoryPath(targetDir);
            targetDir = Path.GetFullPath(targetDir);

            if (!Directory.Exists(targetDir))
            {
                _logger?.LogInformation("Creating directory: {Directory}", targetDir);
                Directory.CreateDirectory(targetDir);
                AnsiConsole.MarkupLine($"[green]✓ Created directory {targetDir}[/]");
            }

            await _scriptExecutor.RunScriptAsync(script, targetDir);
        }
        else
        {
            // Option to save and run the script
            await HandleInteractiveScriptActionAsync(script);
        }
    }

    /// <summary>
    /// Handles interactive script action prompts
    /// </summary>
    private async Task HandleInteractiveScriptActionAsync(string script)
    {
        var action = InteractivePrompts.PromptForScriptAction();

        if (action == "Run")
        {
            var targetDir = InteractivePrompts.PromptForRunDirectory();

            if (!string.IsNullOrWhiteSpace(targetDir) && targetDir != Directory.GetCurrentDirectory())
            {
                // Validate and expand path
                InputValidator.ValidateDirectoryPath(targetDir);
                targetDir = Path.GetFullPath(targetDir);

                if (!Directory.Exists(targetDir))
                {
                    if (InteractivePrompts.ConfirmDirectoryCreation(targetDir))
                    {
                        _logger?.LogInformation("Creating directory: {Directory}", targetDir);
                        Directory.CreateDirectory(targetDir);
                        AnsiConsole.MarkupLine($"[green]✓ Created directory {targetDir}[/]");
                    }
                    else
                    {
                        _logger?.LogInformation("Script execution cancelled by user");
                        AnsiConsole.MarkupLine("[yellow]Script execution cancelled.[/]");
                        return;
                    }
                }
            }
            else
            {
                targetDir = Directory.GetCurrentDirectory();
            }

            await _scriptExecutor.RunScriptAsync(script, targetDir);
        }
        else if (action == "Edit")
        {
            AnsiConsole.MarkupLine("[yellow]Please re-run the tool with different options to edit the script.[/]");
        }
    }
}
