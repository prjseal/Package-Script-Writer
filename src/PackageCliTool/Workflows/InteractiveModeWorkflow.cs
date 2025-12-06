using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Models.Api;
using PackageCliTool.Logging;
using PackageCliTool.Validation;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates the interactive mode workflow
/// </summary>
public class InteractiveModeWorkflow
{
    private readonly ApiClient _apiClient;
    private readonly PackageSelector _packageSelector;
    private readonly ScriptExecutor _scriptExecutor;
    private readonly ILogger? _logger;

    public InteractiveModeWorkflow(
        ApiClient apiClient,
        PackageSelector packageSelector,
        ScriptExecutor scriptExecutor,
        ILogger? logger = null)
    {
        _apiClient = apiClient;
        _packageSelector = packageSelector;
        _scriptExecutor = scriptExecutor;
        _logger = logger;
    }

    /// <summary>
    /// Runs the interactive mode workflow
    /// </summary>
    public async Task RunAsync()
    {
        // Display welcome banner
        ConsoleDisplay.DisplayWelcomeBanner();

        // Populate all packages from API
        await _packageSelector.PopulateAllPackagesAsync();

        // Ask if user wants a default script (fast route)
        var useDefaultScript = AnsiConsole.Confirm("Do you want to generate a default script?", true);

        if (useDefaultScript)
        {
            await GenerateDefaultScriptAsync();
        }
        else
        {
            await RunCustomFlowAsync();
        }
    }

    /// <summary>
    /// Generates a default script with minimal configuration
    /// </summary>
    private async Task GenerateDefaultScriptAsync()
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

        // Option to save and run the script
        await HandleScriptSaveAndRunAsync(script);
    }

    /// <summary>
    /// Runs the custom configuration flow for script generation
    /// </summary>
    private async Task RunCustomFlowAsync()
    {
        _logger?.LogInformation("Starting custom configuration flow");

        // Step 1: Select packages
        var selectedPackages = await _packageSelector.SelectPackagesAsync();

        if (selectedPackages.Count == 0)
        {
            ErrorHandler.Warning("No packages selected. Continuing without packages...", _logger);
            AnsiConsole.WriteLine();

            // Skip to script generation with no packages
            var shouldGenerate = AnsiConsole.Confirm("Would you like to generate a complete installation script?");
            if (shouldGenerate)
            {
                await GenerateAndDisplayScriptAsync(new Dictionary<string, string>());
            }
            return;
        }

        // Step 2: For each package, select version
        var packageVersions = await _packageSelector.SelectVersionsForPackagesAsync(selectedPackages);

        // Step 3: Display final selection
        ConfigurationDisplay.DisplayFinalSelection(packageVersions);

        // Step 4: Optional - Generate script
        var shouldGenerate2 = AnsiConsole.Confirm("Would you like to generate a complete installation script?");

        if (shouldGenerate2)
        {
            await GenerateAndDisplayScriptAsync(packageVersions);
        }
    }

    /// <summary>
    /// Generates a complete installation script using the API
    /// </summary>
    private async Task GenerateAndDisplayScriptAsync(Dictionary<string, string> packageVersions)
    {
        _logger?.LogInformation("Generating complete installation script");

        // Prompt for configuration
        var model = InteractivePrompts.PromptForScriptConfiguration(packageVersions);

        // Display configuration summary
        ConfigurationDisplay.DisplayConfigurationSummary(model, packageVersions);

        // Confirm generation
        if (!InteractivePrompts.ConfirmScriptGeneration())
        {
            _logger?.LogInformation("Script generation cancelled by user");
            AnsiConsole.MarkupLine("[yellow]Script generation cancelled.[/]");
            return;
        }

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

        // Option to save and run the script
        await HandleScriptSaveAndRunAsync(script);
    }

    /// <summary>
    /// Handles running or editing the generated script
    /// </summary>
    private async Task HandleScriptSaveAndRunAsync(string script)
    {
        // Ask user what they want to do with the script
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
            AnsiConsole.MarkupLine("\n[blue]Let's configure a custom script...[/]\n");
            await RunCustomFlowAsync();
        }
    }
}
