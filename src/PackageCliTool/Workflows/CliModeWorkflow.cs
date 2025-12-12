using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Validation;
using PackageCliTool.Logging;
using PSW.Shared.Services;
using PackageCliTool.Extensions;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates the CLI mode workflow (when command-line flags are used)
/// </summary>
public class CliModeWorkflow
{
    private readonly ApiClient _apiClient;
    private readonly ScriptExecutor _scriptExecutor;
    private readonly ILogger? _logger;
    private readonly IScriptGeneratorService _scriptGeneratorService;

    public CliModeWorkflow(
        ApiClient apiClient,
        ScriptExecutor scriptExecutor,
        IScriptGeneratorService scriptGeneratorService,
        ILogger? logger = null)
    {
        _apiClient = apiClient;
        _scriptExecutor = scriptExecutor;
        _scriptGeneratorService = scriptGeneratorService;
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
            TemplateName = !string.IsNullOrWhiteSpace(options.TemplatePackageName)
                ? options.TemplatePackageName
                : "Umbraco.Templates",
            TemplateVersion = !string.IsNullOrWhiteSpace(options.TemplateVersion)
                ? options.TemplateVersion
                : "",
            ProjectName = !string.IsNullOrWhiteSpace(options.ProjectName)
                ? options.ProjectName
                : "MyProject",
            CreateSolutionFile = options.CreateSolution.HasValue
                ? options.CreateSolution.Value
                : true,
            SolutionName = !string.IsNullOrWhiteSpace(options.SolutionName)
                ? options.SolutionName
                : "MySolution",
            IncludeStarterKit = options.IncludeStarterKit.HasValue
                ? options.IncludeStarterKit.Value
                : true,
            StarterKitPackage = !string.IsNullOrWhiteSpace(options.StarterKitPackage)
                ? options.StarterKitPackage
                : "clean",
            IncludeDockerfile = options.IncludeDockerfile.HasValue
                ? options.IncludeDockerfile.Value
                : false,
            IncludeDockerCompose = options.IncludeDockerCompose.HasValue
                ? options.IncludeDockerCompose.Value
                : false,
            CanIncludeDocker = false,
            UseUnattendedInstall = options.UseUnattended.HasValue
                ? options.UseUnattended.Value
                : true,
            DatabaseType = !string.IsNullOrWhiteSpace(options.DatabaseType)
                ? options.DatabaseType
                : "SQLite",
            UserEmail = !string.IsNullOrWhiteSpace(options.AdminEmail)
                ? options.AdminEmail
                : "admin@example.com",
            UserPassword = !string.IsNullOrWhiteSpace(options.AdminPassword)
                ? options.AdminPassword
                : "1234567890",
            UserFriendlyName = !string.IsNullOrWhiteSpace(options.AdminName)
                ? options.AdminName
                : "Administrator",
            OnelinerOutput = options.OnelinerOutput.HasValue
                ? options.OnelinerOutput.Value
                : false,
            RemoveComments = options.RemoveComments.HasValue
                ? options.RemoveComments.Value
                : false
        };

        HandlePackages(options, model);
        HandleStarterKitPackage(options, model);
        HandleTemplatePackage(options, model);

        var script = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Generating default installation script...", async ctx =>
            {
                return _scriptGeneratorService.GenerateScript(model.ToViewModel());
                //return await _apiClient.GenerateScriptAsync(model);
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

        var projectName = !string.IsNullOrWhiteSpace(options.ProjectName)
            ? options.ProjectName
            : "MyProject";

        // Validate inputs
        InputValidator.ValidateProjectName(projectName);
        InputValidator.ValidateSolutionName(options.SolutionName);
        InputValidator.ValidateEmail(options.AdminEmail);
        InputValidator.ValidatePassword(options.AdminPassword);
        InputValidator.ValidateDatabaseType(options.DatabaseType);
        InputValidator.ValidateConnectionString(options.ConnectionString, options.DatabaseType);

        var model = new ScriptModel
        {
            TemplateName = !string.IsNullOrWhiteSpace(options.TemplatePackageName)
                ? options.TemplatePackageName
                : null,
            TemplateVersion = !string.IsNullOrWhiteSpace(options.TemplateVersion)
                ? options.TemplateVersion
                : null,
            ProjectName = !string.IsNullOrWhiteSpace(options.ProjectName)
                ? options.ProjectName
                : null,
            CreateSolutionFile = options.CreateSolution.HasValue
                ? options.CreateSolution.Value
                : !string.IsNullOrWhiteSpace(options.SolutionName),
            SolutionName = !string.IsNullOrWhiteSpace(options.SolutionName)
                ? options.SolutionName
                : null,
            IncludeStarterKit = options.IncludeStarterKit.HasValue
                ? options.IncludeStarterKit.Value
                : false,
            StarterKitPackage = !string.IsNullOrWhiteSpace(options.StarterKitPackage)
                ? options.StarterKitPackage
                : null,
            IncludeDockerfile = options.IncludeDockerfile.HasValue
                ? options.IncludeDockerfile.Value
                : false,
            IncludeDockerCompose = options.IncludeDockerCompose.HasValue
                ? options.IncludeDockerCompose.Value
                : false,
            CanIncludeDocker = (options.IncludeDockerfile.HasValue && options.IncludeDockerfile.Value) || (options.IncludeDockerCompose.HasValue && options.IncludeDockerCompose.Value),
            UseUnattendedInstall = options.UseUnattended.HasValue
                ? options.UseUnattended.Value
                : false,
            DatabaseType = !string.IsNullOrWhiteSpace(options.DatabaseType)
                ? options.DatabaseType
                : null,
            ConnectionString = !string.IsNullOrWhiteSpace(options.ConnectionString)
                ? options.ConnectionString
                : null,
            UserFriendlyName = !string.IsNullOrWhiteSpace(options.AdminName)
                ? options.AdminName
                : null,
            UserEmail = !string.IsNullOrWhiteSpace(options.AdminEmail)
                ? options.AdminEmail
                : null,
            UserPassword = !string.IsNullOrWhiteSpace(options.AdminPassword)
                ? options.AdminPassword
                : null,
            OnelinerOutput = options.OnelinerOutput.HasValue
                ? options.OnelinerOutput.Value
                : false,
            RemoveComments = options.RemoveComments.HasValue
                ? options.RemoveComments.Value
                : false
        };

        HandlePackages(options, model);
        HandleStarterKitPackage(options, model);
        HandleTemplatePackage(options, model);

        // Generate the script
        _logger?.LogInformation("Generating installation script via API");

        var script = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Generating installation script...", async ctx =>
            {
                return _scriptGeneratorService.GenerateScript(model.ToViewModel());
                //return await _apiClient.GenerateScriptAsync(model);
            });

        _logger?.LogInformation("Script generated successfully");

        ConsoleDisplay.DisplayGeneratedScript(script);

        // Handle auto-run or interactive run
        await HandleScriptExecutionAsync(script, options);
    }

    private void HandlePackages(CommandLineOptions options, ScriptModel model)
    {
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
    }

    private void HandleStarterKitPackage(CommandLineOptions options, ScriptModel model)
    {
        // Handle starter kit package
        if (!string.IsNullOrWhiteSpace(options.StarterKitPackage))
        {
            _logger?.LogDebug("Processing starter kit package: {StarterKitPackage}", options.StarterKitPackage);

            var packageEntries = options.StarterKitPackage.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            if (packageEntries.Count > 0)
            {
                var processedPackages = new List<string>();
                var firstPackage = packageEntries[0];
                // Check if version is specified with pipe character (e.g., "clean|7.0.3")
                if (firstPackage.Contains('|'))
                {
                    var parts = firstPackage.Split('|', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        var packageName = parts[0].Trim();
                        var version = parts[1].Trim();

                        // Validate package name and version
                        InputValidator.ValidatePackageName(packageName);
                        InputValidator.ValidateVersion(version);

                        processedPackages.Add($"{packageName}|{version}");
                        AnsiConsole.MarkupLine($"[green]✓[/] Using {packageName} version {version}");
                        _logger?.LogDebug("Added starter kit package {Package} with version {Version}", packageName, version);
                    }
                    else
                    {
                        ErrorHandler.Warning($"Invalid package format: {firstPackage}, skipping...", _logger);
                    }
                }
                else
                {
                    // No version specified, use package name without version
                    var packageName = firstPackage.Trim();
                    InputValidator.ValidatePackageName(packageName);

                    processedPackages.Add(packageName);
                    AnsiConsole.MarkupLine($"[green]✓[/] Using {packageName} (latest version)");
                    _logger?.LogDebug("Added starter kit package {Package} with latest version", packageName);
                }

                // Build packages string - can be mixed format: "Package1|Version1,Package2,Package3|Version3"
                if (processedPackages.Count > 0)
                {
                    model.IncludeStarterKit = true;
                    model.StarterKitPackage = processedPackages[0];
                }
            }
        }
    }

    private void HandleTemplatePackage(CommandLineOptions options, ScriptModel model)
    {
        // Handle template package
        if (!string.IsNullOrWhiteSpace(options.TemplatePackageName))
        {
            _logger?.LogDebug("Processing template package: {TemplatePackageName}", options.TemplatePackageName);

            // Validate template package name
            InputValidator.ValidatePackageName(options.TemplatePackageName);

            // Update model with template package name
            model.TemplateName = options.TemplatePackageName;

            // Handle template version if specified
            if (!string.IsNullOrWhiteSpace(options.TemplateVersion))
            {
                // Validate version
                InputValidator.ValidateVersion(options.TemplateVersion);

                model.TemplateVersion = options.TemplateVersion;

                AnsiConsole.MarkupLine($"[green]✓[/] Using {options.TemplatePackageName} version {options.TemplateVersion}");
                _logger?.LogDebug("Using template {Template} with version {Version}", options.TemplatePackageName, options.TemplateVersion);
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]✓[/] Using {options.TemplatePackageName} (latest version)");
                _logger?.LogDebug("Using template {Template} with latest version", options.TemplatePackageName);
            }
        }
    }

    /// <summary>
    /// Handles script execution based on options
    /// </summary>
    private async Task HandleScriptExecutionAsync(string script, CommandLineOptions options)
    {
        if (options.AutoRun || !string.IsNullOrWhiteSpace(options.RunDirectory))
        {
            var targetDir = !string.IsNullOrWhiteSpace(options.RunDirectory)
                ? options.RunDirectory
                : Directory.GetCurrentDirectory();

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
        else if (action == "Copy")
        {
            await ClipboardHelper.CopyToClipboardAsync(script, _logger);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleInteractiveScriptActionAsync(script);
            }
        }
        else if (action == "Start over")
        {
            AnsiConsole.MarkupLine("[yellow]To start over, please re-run the tool with different command-line options.[/]");
        }
    }
}
