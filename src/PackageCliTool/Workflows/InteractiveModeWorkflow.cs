using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Models.Api;
using PackageCliTool.Logging;
using PackageCliTool.Validation;
using PackageCliTool.Extensions;
using PSW.Shared.Services;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates the interactive mode workflow
/// </summary>
public class InteractiveModeWorkflow
{
    private readonly ApiClient _apiClient;
    private readonly PackageSelector _packageSelector;
    private readonly ScriptExecutor _scriptExecutor;
    private readonly TemplateService _templateService;
    private readonly ILogger? _logger;
    private readonly IScriptGeneratorService _scriptGeneratorService;

    public InteractiveModeWorkflow(
        ApiClient apiClient,
        PackageSelector packageSelector,
        ScriptExecutor scriptExecutor,
        IScriptGeneratorService scriptGeneratorService,
        ILogger? logger = null)
    {
        _apiClient = apiClient;
        _packageSelector = packageSelector;
        _scriptExecutor = scriptExecutor;
        _templateService = new TemplateService(logger: logger);
        _scriptGeneratorService = scriptGeneratorService;
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
                return _scriptGeneratorService.GenerateScript(model.ToViewModel());
                //return await _apiClient.GenerateScriptAsync(model);
            });

        _logger?.LogInformation("Default script generated successfully");

        ConsoleDisplay.DisplayGeneratedScript(script, "Generated Default Installation Script");

        // Option to save and run the script
        // Create empty packageVersions dict since default script has no packages
        var packageVersions = new Dictionary<string, string>();
        await HandleScriptSaveAndRunAsync(script, model, packageVersions);
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
        var packageVersions = _packageSelector.SelectVersionsForPackages(selectedPackages);

        // Step 3: Select template
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 3:[/] Select Template\n");
        var templateName = await _packageSelector.SelectTemplateAsync();

        // Step 4: Select template version
        var templateVersion = _packageSelector.SelectTemplateVersion(templateName);

        // Step 5: Display final selection
        ConfigurationDisplay.DisplayFinalSelection(packageVersions);

        // Step 6: Optional - Generate script
        var shouldGenerate2 = AnsiConsole.Confirm("Would you like to generate a complete installation script?");

        if (shouldGenerate2)
        {
            await GenerateAndDisplayScriptAsync(packageVersions, templateName, templateVersion);
        }
    }

    /// <summary>
    /// Generates a complete installation script using the API
    /// </summary>
    private async Task GenerateAndDisplayScriptAsync(Dictionary<string, string> packageVersions, string? templateName = null, string? templateVersion = null, ScriptModel? existingModel = null)
    {
        _logger?.LogInformation("Generating complete installation script");

        // Prompt for configuration (with existing model values as defaults if editing)
        var model = await InteractivePrompts.PromptForScriptConfigurationAsync(packageVersions, _apiClient, _logger, templateName, templateVersion, existingModel);

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
                return _scriptGeneratorService.GenerateScript(model.ToViewModel());
                //return await _apiClient.GenerateScriptAsync(model);
            });

        _logger?.LogInformation("Script generated successfully");

        ConsoleDisplay.DisplayGeneratedScript(script);

        // Option to save and run the script
        await HandleScriptSaveAndRunAsync(script, model, packageVersions, templateName, templateVersion);
    }

    /// <summary>
    /// Handles running or editing the generated script
    /// </summary>
    private async Task HandleScriptSaveAndRunAsync(string script, ScriptModel? scriptModel = null, Dictionary<string, string>? packageVersions = null, string? templateName = null, string? templateVersion = null)
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
            AnsiConsole.MarkupLine("\n[blue]Editing script configuration...[/]\n");

            if (scriptModel != null && packageVersions != null)
            {
                // Edit with existing values as defaults
                await GenerateAndDisplayScriptAsync(packageVersions, templateName, templateVersion, scriptModel);
            }
            else
            {
                // Fallback to starting from scratch
                await RunCustomFlowAsync();
            }
        }
        else if (action == "Copy to clipboard")
        {
            await ClipboardHelper.CopyToClipboardAsync(script, _logger);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleScriptSaveAndRunAsync(script, scriptModel, packageVersions, templateName, templateVersion);
            }
        }
        else if (action == "Save as template")
        {
            await SaveAsTemplateAsync(scriptModel, packageVersions);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleScriptSaveAndRunAsync(script, scriptModel, packageVersions, templateName, templateVersion);
            }
        }
        else if (action == "Start over")
        {
            AnsiConsole.MarkupLine("\n[blue]Starting over...[/]\n");
            _logger?.LogInformation("User chose to start over");
            await RunAsync();
        }
    }

    /// <summary>
    /// Saves the current script configuration as a template
    /// </summary>
    private async Task SaveAsTemplateAsync(ScriptModel? scriptModel, Dictionary<string, string>? packageVersions)
    {
        if (scriptModel == null || packageVersions == null)
        {
            AnsiConsole.MarkupLine("[yellow]Cannot save template - script configuration not available.[/]");
            return;
        }

        _logger?.LogInformation("Saving script configuration as template");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Save as Template[/]\n");

        // Prompt for template name
        var templateName = AnsiConsole.Ask<string>("Enter [green]template name[/]:");

        // Prompt for description
        var description = AnsiConsole.Ask<string>(
            "Enter [green]template description[/] (optional):",
            string.Empty);

        // Prompt for tags
        var tagsInput = AnsiConsole.Ask<string>(
            "Enter [green]tags[/] (comma-separated, optional):",
            string.Empty);

        List<string> tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(tagsInput))
        {
            tags = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();
        }

        // Create template from script model
        var template = _templateService.FromScriptModel(
            scriptModel,
            packageVersions,
            templateName,
            string.IsNullOrWhiteSpace(description) ? null : description);

        // Add tags if provided
        if (tags.Count > 0)
        {
            template.Metadata.Tags = tags;
        }

        // Check if template already exists
        if (_templateService.TemplateExists(templateName))
        {
            var overwrite = AnsiConsole.Confirm(
                $"Template [yellow]{templateName}[/] already exists. Overwrite?",
                false);

            if (!overwrite)
            {
                AnsiConsole.MarkupLine("[yellow]Template save cancelled.[/]");
                return;
            }
        }

        // Save template
        try
        {
            await _templateService.SaveTemplateAsync(template);
            AnsiConsole.MarkupLine($"[green]✓ Template saved:[/] {templateName}");
            _logger?.LogInformation("Template saved successfully: {Name}", templateName);

            // Show helpful message
            AnsiConsole.MarkupLine($"[dim]You can load this template with: psw template load {templateName}[/]");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to save template: {Name}", templateName);
            AnsiConsole.MarkupLine($"[red]✗ Failed to save template:[/] {ex.Message}");
        }
    }
}
