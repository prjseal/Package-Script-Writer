using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Models.Api;
using PackageCliTool.Logging;
using PackageCliTool.Validation;
using PackageCliTool.Extensions;
using PSW.Shared.Services;
using PSW.Shared.Configuration;

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
    private readonly VersionCheckService _versionCheckService;
    private readonly HistoryService _historyService;
    private readonly ILogger? _logger;
    private readonly IScriptGeneratorService _scriptGeneratorService;
    private readonly PSWConfig _pswConfig;

    public InteractiveModeWorkflow(
        ApiClient apiClient,
        PackageSelector packageSelector,
        ScriptExecutor scriptExecutor,
        IScriptGeneratorService scriptGeneratorService,
        VersionCheckService versionCheckService,
        HistoryService historyService,
        PSWConfig pswConfig,
        ILogger? logger = null)
    {
        _apiClient = apiClient;
        _packageSelector = packageSelector;
        _scriptExecutor = scriptExecutor;
        _templateService = new TemplateService(logger: logger);
        _versionCheckService = versionCheckService;
        _historyService = historyService;
        _scriptGeneratorService = scriptGeneratorService;
        _pswConfig = pswConfig;
        _logger = logger;
    }

    /// <summary>
    /// Runs the interactive mode workflow with main menu loop
    /// </summary>
    public async Task RunAsync()
    {
        // Display welcome banner
        ConsoleDisplay.DisplayWelcomeBanner();

        // Check for updates (non-blocking)
        await CheckForUpdatesAsync();

        // Populate all packages from API
        await _packageSelector.PopulateAllPackagesAsync();

        // Main menu loop
        bool keepRunning = true;
        while (keepRunning)
        {
            AnsiConsole.WriteLine();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]What would you like to do?[/]")
                    .AddChoices(new[]
                    {
                        "Create script from scratch",
                        "Create script from defaults",
                        "See Umbraco versions table",
                        "See templates",
                        "See history",
                        "See help",
                        "See version",
                        "Clear cache"
                    }));

            switch (choice)
            {
                case "Create script from scratch":
                    await RunConfigurationEditorAsync(useDefaults: false);
                    break;

                case "Create script from defaults":
                    await RunConfigurationEditorAsync(useDefaults: true);
                    break;

                case "See Umbraco versions table":
                    ShowUmbracoVersionsTable();
                    break;

                case "See templates":
                    await ShowTemplatesAsync();
                    break;

                case "See history":
                    await ShowHistoryAsync();
                    break;

                case "See help":
                    ConsoleDisplay.DisplayHelp();
                    keepRunning = false; // Exit after showing help
                    break;

                case "See version":
                    ConsoleDisplay.DisplayVersion();
                    keepRunning = false; // Exit after showing version
                    break;

                case "Clear cache":
                    await ClearCacheAsync();
                    break;
            }
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

        // Save to history
        _historyService.AddEntry(
            model,
            templateName: model.TemplateName,
            description: $"Default script for {model.ProjectName}");

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

        // Step 1: Select template
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 1:[/] Select Template\n");
        var templateName = await _packageSelector.SelectTemplateAsync();

        // Step 2: Select template version
        var templateVersion = await _packageSelector.SelectTemplateVersionAsync(templateName);

        // Step 3: Select packages
        var selectedPackages = await _packageSelector.SelectPackagesAsync();

        if (selectedPackages.Count == 0)
        {
            ErrorHandler.Warning("No packages selected. Continuing without packages...", _logger);
            AnsiConsole.WriteLine();

            // Skip to script generation with no packages
            var shouldGenerate = AnsiConsole.Confirm("Would you like to generate a complete installation script?");
            if (shouldGenerate)
            {
                await GenerateAndDisplayScriptAsync(new Dictionary<string, string>(), templateName, templateVersion);
            }
            return;
        }

        // Step 4: For each package, select version
        var packageVersions = await _packageSelector.SelectVersionsForPackagesAsync(selectedPackages);

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

        // Save to history
        _historyService.AddEntry(
            model,
            templateName: templateName,
            description: $"Custom script for {model.ProjectName ?? "project"}");

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
        else if (action == "Copy")
        {
            await ClipboardHelper.CopyToClipboardAsync(script, _logger);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleScriptSaveAndRunAsync(script, scriptModel, packageVersions, templateName, templateVersion);
            }
        }
        else if (action == "Save")
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

    /// <summary>
    /// Runs the configuration editor workflow
    /// </summary>
    private async Task RunConfigurationEditorAsync(bool useDefaults)
    {
        _logger?.LogInformation("Starting configuration editor (useDefaults: {UseDefaults})", useDefaults);

        // Initialize configuration with defaults or empty values
        var config = new ScriptModel();
        var packageVersions = new Dictionary<string, string>();
        string templateName = "Umbraco.Templates";
        string templateVersion = "";

        if (useDefaults)
        {
            // Set default values
            config.TemplateName = "Umbraco.Templates";
            config.TemplateVersion = "";
            config.ProjectName = "MyProject";
            config.CreateSolutionFile = true;
            config.SolutionName = "MySolution";
            config.IncludeStarterKit = true;
            config.StarterKitPackage = "clean";
            config.IncludeDockerfile = false;
            config.IncludeDockerCompose = false;
            config.CanIncludeDocker = false;
            config.UseUnattendedInstall = true;
            config.DatabaseType = "SQLite";
            config.ConnectionString = "";
            config.UserFriendlyName = "Administrator";
            config.UserEmail = "admin@example.com";
            config.UserPassword = "1234567890";
            config.OnelinerOutput = false;
            config.RemoveComments = false;
            config.Packages = "";
        }
        else
        {
            // Set minimal defaults for scratch mode
            config.TemplateName = "";
            config.TemplateVersion = "";
            config.ProjectName = "";
            config.CreateSolutionFile = false;
            config.SolutionName = "";
            config.IncludeStarterKit = false;
            config.StarterKitPackage = "";
            config.IncludeDockerfile = false;
            config.IncludeDockerCompose = false;
            config.CanIncludeDocker = false;
            config.UseUnattendedInstall = false;
            config.DatabaseType = "SQLite";
            config.ConnectionString = "";
            config.UserFriendlyName = "";
            config.UserEmail = "";
            config.UserPassword = "";
            config.OnelinerOutput = false;
            config.RemoveComments = false;
            config.Packages = "";
        }

        // If using defaults, display configuration table first
        if (useDefaults)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold blue]Default Configuration[/]\n");
            DisplayConfigurationTable(config, packageVersions);
        }

        // Configuration editor loop
        bool keepEditing = true;
        while (keepEditing)
        {
            // If using defaults, show different initial prompt
            if (useDefaults)
            {
                AnsiConsole.WriteLine();
                var initialAction = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What would you like to do?")
                        .AddChoices(new[] { "Edit configuration", "Generate script", "Cancel" }));

                if (initialAction == "Generate script")
                {
                    keepEditing = false;

                    // Generate the script
                    _logger?.LogInformation("Generating installation script with defaults");

                    var script = await AnsiConsole.Status()
                        .Spinner(Spinner.Known.Star)
                        .SpinnerStyle(Style.Parse("green"))
                        .StartAsync("Generating installation script...", async ctx =>
                        {
                            return _scriptGeneratorService.GenerateScript(config.ToViewModel());
                        });

                    _logger?.LogInformation("Script generated successfully");

                    // Save to history
                    _historyService.AddEntry(
                        config,
                        templateName: templateName,
                        description: $"Script for {config.ProjectName ?? "project"}");

                    ConsoleDisplay.DisplayGeneratedScript(script);

                    // Handle script actions (returns to main menu when done)
                    await HandleScriptActionsAsync(script, config, packageVersions, templateName, templateVersion);
                    return;
                }
                else if (initialAction == "Cancel")
                {
                    return; // Return to main menu
                }

                // If "Edit configuration" was selected, continue to field selection below
                useDefaults = false; // Switch to edit mode for subsequent loops
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold blue]Configuration Editor[/]\n");

            // Display multi-select list with current values
            var fieldChoices = GetConfigurationFieldChoices(config, packageVersions);

            var selectedFields = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select [green]configuration fields to edit[/] (use Space to select, Enter to confirm):")
                    .PageSize(20)
                    .MoreChoicesText("[grey](Move up and down to see more fields)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to toggle a field, [green]<enter>[/] to accept)[/]")
                    .AddChoices(fieldChoices));

            if (selectedFields.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No fields selected for editing.[/]");

                // Ask if they want to generate with current settings or edit again
                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What would you like to do?")
                        .AddChoices(new[] { "Edit configuration", "Generate script", "Cancel" }));

                if (action == "Generate script")
                {
                    keepEditing = false;
                }
                else if (action == "Cancel")
                {
                    return; // Return to main menu
                }
                continue;
            }

            // Process each selected field
            foreach (var fieldDisplay in selectedFields)
            {
                await ProcessConfigurationFieldAsync(fieldDisplay, config, packageVersions, templateName, templateVersion);
            }

            // Update packages string from dictionary
            if (packageVersions.Count > 0)
            {
                var packageParts = new List<string>();
                foreach (var (package, version) in packageVersions)
                {
                    if (string.IsNullOrEmpty(version))
                    {
                        packageParts.Add(package);
                    }
                    else if (version == "--prerelease")
                    {
                        packageParts.Add($"{package} {version}");
                    }
                    else
                    {
                        packageParts.Add($"{package}|{version}");
                    }
                }
                config.Packages = string.Join(",", packageParts);
            }

            // Sync template fields from config to local variables
            templateName = config.TemplateName;
            templateVersion = config.TemplateVersion;

            // Display configuration table
            AnsiConsole.WriteLine();
            DisplayConfigurationTable(config, packageVersions);

            // Ask if they want to edit again or generate
            var nextAction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\nWhat would you like to do next?")
                    .AddChoices(new[] { "Edit configuration", "Generate script", "Cancel" }));

            if (nextAction == "Generate script")
            {
                keepEditing = false;

                // Generate the script
                _logger?.LogInformation("Generating installation script");

                var script = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("green"))
                    .StartAsync("Generating installation script...", async ctx =>
                    {
                        return _scriptGeneratorService.GenerateScript(config.ToViewModel());
                    });

                _logger?.LogInformation("Script generated successfully");

                // Save to history
                _historyService.AddEntry(
                    config,
                    templateName: templateName,
                    description: $"Script for {config.ProjectName ?? "project"}");

                ConsoleDisplay.DisplayGeneratedScript(script);

                // Handle script actions (returns to main menu when done)
                await HandleScriptActionsAsync(script, config, packageVersions, templateName, templateVersion);
            }
            else if (nextAction == "Cancel")
            {
                keepEditing = false;
                return; // Return to main menu
            }
        }
    }

    /// <summary>
    /// Gets the list of configuration field choices with current values
    /// </summary>
    private List<string> GetConfigurationFieldChoices(ScriptModel config, Dictionary<string, string> packageVersions)
    {
        var choices = new List<string>
        {
            $"Template - {config.TemplateName} @ {(string.IsNullOrEmpty(config.TemplateVersion) ? "Latest" : config.TemplateVersion)}",
            $"Project name - {config.ProjectName}",
            $"Create solution file - {config.CreateSolutionFile}",
            $"Solution name - {config.SolutionName ?? "N/A"}",
            $"Packages - {(packageVersions.Count > 0 ? $"{packageVersions.Count} selected" : "None")}",
            $"Include starter kit - {config.IncludeStarterKit}",
            $"Starter kit package - {config.StarterKitPackage ?? "N/A"}",
            $"Include Dockerfile - {config.IncludeDockerfile}",
            $"Include Docker Compose - {config.IncludeDockerCompose}",
            $"Use unattended install defaults - {config.UseUnattendedInstall}",
            $"Database type - {config.DatabaseType ?? "N/A"}",
            $"Connection string - {(string.IsNullOrEmpty(config.ConnectionString) ? "N/A" : "***")}",
            $"Admin user friendly name - {config.UserFriendlyName ?? "N/A"}",
            $"Admin email - {config.UserEmail ?? "N/A"}",
            $"Admin password - {(string.IsNullOrEmpty(config.UserPassword) ? "N/A" : "***")}",
            $"One-liner output - {config.OnelinerOutput}",
            $"Remove comments - {config.RemoveComments}"
        };

        return choices;
    }

    /// <summary>
    /// Processes a selected configuration field
    /// </summary>
    private async Task ProcessConfigurationFieldAsync(string fieldDisplay, ScriptModel config, Dictionary<string, string> packageVersions, string templateName, string templateVersion)
    {
        AnsiConsole.WriteLine();
        var fieldName = fieldDisplay.Split(" - ")[0];

        switch (fieldName)
        {
            case "Template":
                AnsiConsole.MarkupLine("[bold blue]Select Template[/]\n");
                config.TemplateName = await _packageSelector.SelectTemplateAsync();
                config.TemplateVersion = await _packageSelector.SelectTemplateVersionAsync(config.TemplateName);
                break;

            case "Project name":
                config.ProjectName = AnsiConsole.Ask<string>("Enter [green]project name[/]:", config.ProjectName);
                InputValidator.ValidateProjectName(config.ProjectName);
                break;

            case "Create solution file":
                config.CreateSolutionFile = AnsiConsole.Confirm("Create a [green]solution file[/]?", config.CreateSolutionFile);
                break;

            case "Solution name":
                if (config.CreateSolutionFile)
                {
                    var defaultSolution = string.IsNullOrEmpty(config.SolutionName) ? config.ProjectName : config.SolutionName;
                    config.SolutionName = AnsiConsole.Ask<string>("Enter [green]solution name[/]:", defaultSolution);
                    InputValidator.ValidateSolutionName(config.SolutionName);
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Create solution file is disabled. Enable it first.[/]");
                }
                break;

            case "Packages":
                AnsiConsole.MarkupLine("[bold blue]Select Packages[/]\n");
                var selectedPackages = await _packageSelector.SelectPackagesAsync();
                if (selectedPackages.Count > 0)
                {
                    var newPackageVersions = await _packageSelector.SelectVersionsForPackagesAsync(selectedPackages);
                    // Merge with existing
                    foreach (var kvp in newPackageVersions)
                    {
                        packageVersions[kvp.Key] = kvp.Value;
                    }
                }
                break;

            case "Include starter kit":
                config.IncludeStarterKit = AnsiConsole.Confirm("Include a [green]starter kit[/]?", config.IncludeStarterKit);
                break;

            case "Starter kit package":
                if (config.IncludeStarterKit)
                {
                    AnsiConsole.MarkupLine("[bold blue]Select Starter Kit[/]\n");
                    var starterKitName = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select [green]starter kit[/]:")
                            .AddChoices(new[]
                            {
                                "clean",
                                "Articulate",
                                "Portfolio",
                                "LittleNorth.Igloo",
                                "Umbraco.BlockGrid.Example.Website",
                                "Umbraco.TheStarterKit",
                                "uSkinnedSiteBuilder"
                            }));

                    // Fetch and select version for the starter kit
                    try
                    {
                        _logger?.LogDebug("Fetching versions for starter kit: {StarterKit}", starterKitName);

                        var versions = await AnsiConsole.Status()
                            .Spinner(Spinner.Known.Dots)
                            .SpinnerStyle(Style.Parse("green"))
                            .StartAsync($"Fetching versions for [yellow]{starterKitName}[/]...", async ctx =>
                            {
                                return await _apiClient.GetPackageVersionsAsync(starterKitName, includePrerelease: true);
                            });

                        _logger?.LogDebug("Found {Count} versions for starter kit {StarterKit}", versions.Count, starterKitName);

                        var versionChoices = new List<string> { "Latest Stable" };
                        if (versions.Count > 0)
                        {
                            versionChoices.AddRange(versions);
                        }
                        else
                        {
                            ErrorHandler.Warning($"No specific versions found for {starterKitName}. Using latest stable.", _logger);
                        }

                        var selectedVersion = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title($"Select version for [green]{starterKitName}[/]:")
                                .PageSize(12)
                                .MoreChoicesText("[grey](Move up and down to see more versions)[/]")
                                .AddChoices(versionChoices));

                        if (selectedVersion == "Latest Stable")
                        {
                            config.StarterKitPackage = starterKitName;
                            AnsiConsole.MarkupLine($"[green]✓[/] Selected {starterKitName} - Latest Stable");
                            _logger?.LogInformation("Selected {StarterKit} with latest stable version", starterKitName);
                        }
                        else
                        {
                            config.StarterKitPackage = $"{starterKitName} --version {selectedVersion}";
                            AnsiConsole.MarkupLine($"[green]✓[/] Selected {starterKitName} version {selectedVersion}");
                            _logger?.LogInformation("Selected {StarterKit} version {Version}", starterKitName, selectedVersion);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error fetching versions for starter kit {StarterKit}", starterKitName);
                        ErrorHandler.Warning($"Error fetching versions for {starterKitName}: {ex.Message}. Using latest stable.", _logger);
                        config.StarterKitPackage = starterKitName;
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Include starter kit is disabled. Enable it first.[/]");
                }
                break;

            case "Include Dockerfile":
                config.IncludeDockerfile = AnsiConsole.Confirm("Include [green]Dockerfile[/]?", config.IncludeDockerfile);
                config.CanIncludeDocker = config.IncludeDockerfile || config.IncludeDockerCompose;
                break;

            case "Include Docker Compose":
                config.IncludeDockerCompose = AnsiConsole.Confirm("Include [green]Docker Compose[/]?", config.IncludeDockerCompose);
                config.CanIncludeDocker = config.IncludeDockerfile || config.IncludeDockerCompose;
                break;

            case "Use unattended install defaults":
                config.UseUnattendedInstall = AnsiConsole.Confirm("Use [green]unattended install defaults[/]?", config.UseUnattendedInstall);
                if (config.UseUnattendedInstall)
                {
                    // Set some reasonable defaults if enabling
                    config.DatabaseType = "SQLite";
                    config.UserFriendlyName = "Administrator";
                    config.UserEmail = "admin@example.com";
                    config.UserPassword = "1234567890";
                }
                break;

            case "Database type":
                var databaseChoices = new[] { "SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE" };
                var defaultDatabase = config.DatabaseType ?? "SQLite";

                config.DatabaseType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select [green]database type[/]:")
                        .AddChoices(databaseChoices));

                // Conditional: If SQLServer or SQLAzure, prompt for connection string
                if (config.DatabaseType == "SQLServer" || config.DatabaseType == "SQLAzure")
                {
                    AnsiConsole.MarkupLine("[yellow]Connection string is required for SQLServer/SQLAzure[/]");
                    config.ConnectionString = AnsiConsole.Ask<string>("Enter [green]connection string[/]:", config.ConnectionString ?? string.Empty);
                    InputValidator.ValidateConnectionString(config.ConnectionString, config.DatabaseType);
                }
                break;

            case "Connection string":
                if (config.DatabaseType == "SQLServer" || config.DatabaseType == "SQLAzure")
                {
                    config.ConnectionString = AnsiConsole.Ask<string>("Enter [green]connection string[/]:", config.ConnectionString ?? string.Empty);
                    InputValidator.ValidateConnectionString(config.ConnectionString, config.DatabaseType);
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Connection string is only required for SQLServer/SQLAzure databases.[/]");
                }
                break;

            case "Admin user friendly name":
                config.UserFriendlyName = AnsiConsole.Ask<string>("Enter [green]admin user friendly name[/]:", config.UserFriendlyName ?? "Administrator");
                break;

            case "Admin email":
                config.UserEmail = AnsiConsole.Ask<string>("Enter [green]admin email[/]:", config.UserEmail ?? "admin@example.com");
                InputValidator.ValidateEmail(config.UserEmail);
                break;

            case "Admin password":
                config.UserPassword = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter [green]admin password[/] (min 10 characters):")
                        .PromptStyle("red")
                        .Secret()
                        .DefaultValue(config.UserPassword ?? "1234567890"));
                InputValidator.ValidatePassword(config.UserPassword);
                break;

            case "One-liner output":
                config.OnelinerOutput = AnsiConsole.Confirm("Output as [green]one-liner[/]?", config.OnelinerOutput);
                break;

            case "Remove comments":
                config.RemoveComments = AnsiConsole.Confirm("Remove [green]comments[/] from script?", config.RemoveComments);
                break;
        }
    }

    /// <summary>
    /// Displays the configuration table
    /// </summary>
    private void DisplayConfigurationTable(ScriptModel config, Dictionary<string, string> packageVersions)
    {
        AnsiConsole.MarkupLine("[bold blue]Current Configuration[/]\n");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Aqua)
            .AddColumn(new TableColumn("[bold]Field[/]"))
            .AddColumn(new TableColumn("[bold]Value[/]"));

        table.AddRow("Template", $"{config.TemplateName} @ {(string.IsNullOrEmpty(config.TemplateVersion) ? "Latest" : config.TemplateVersion)}");
        table.AddRow("Project name", config.ProjectName);
        table.AddRow("Create solution file", config.CreateSolutionFile.ToString());

        if (config.CreateSolutionFile)
        {
            table.AddRow("Solution name", config.SolutionName ?? "N/A");
        }

        if (packageVersions.Count > 0)
        {
            table.AddRow("Packages", $"{packageVersions.Count} selected");
            foreach (var kvp in packageVersions)
            {
                string versionDisplay = kvp.Value switch
                {
                    "" => "Latest Stable",
                    "--prerelease" => "Pre-release",
                    _ => kvp.Value
                };
                table.AddRow($"  - {kvp.Key}", versionDisplay);
            }
        }
        else
        {
            table.AddRow("Packages", "None");
        }

        table.AddRow("Include starter kit", config.IncludeStarterKit.ToString());

        if (config.IncludeStarterKit)
        {
            table.AddRow("Starter kit package", config.StarterKitPackage ?? "N/A");
        }

        table.AddRow("Include Dockerfile", config.IncludeDockerfile.ToString());
        table.AddRow("Include Docker Compose", config.IncludeDockerCompose.ToString());
        table.AddRow("Use unattended install defualts", config.UseUnattendedInstall.ToString());

        table.AddRow("Database type", config.DatabaseType ?? "N/A");

        if (config.DatabaseType == "SQLServer" || config.DatabaseType == "SQLAzure")
        {
            table.AddRow("Connection string", string.IsNullOrEmpty(config.ConnectionString) ? "N/A" : "***");
        }

        table.AddRow("Admin user friendly name", config.UserFriendlyName ?? "N/A");
        table.AddRow("Admin email", config.UserEmail ?? "N/A");
        table.AddRow("Admin password", string.IsNullOrEmpty(config.UserPassword) ? "N/A" : "***");

        table.AddRow("One-liner output", config.OnelinerOutput.ToString());
        table.AddRow("Remove comments", config.RemoveComments.ToString());

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Edits an existing configuration and regenerates the script
    /// </summary>
    private async Task EditConfigurationAsync(ScriptModel config, Dictionary<string, string> packageVersions, string templateName, string templateVersion)
    {
        _logger?.LogInformation("Editing existing configuration");

        // Configuration editor loop
        bool keepEditing = true;
        while (keepEditing)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold blue]Configuration Editor[/]\n");

            // Display multi-select list with current values
            var fieldChoices = GetConfigurationFieldChoices(config, packageVersions);

            var selectedFields = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select [green]configuration fields to edit[/] (use Space to select, Enter to confirm):")
                    .PageSize(20)
                    .MoreChoicesText("[grey](Move up and down to see more fields)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to toggle a field, [green]<enter>[/] to accept)[/]")
                    .AddChoices(fieldChoices));

            if (selectedFields.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No fields selected for editing.[/]");

                // Ask if they want to generate with current settings or edit again
                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What would you like to do?")
                        .AddChoices(new[] { "Edit configuration", "Generate script", "Cancel" }));

                if (action == "Generate script")
                {
                    keepEditing = false;
                }
                else if (action == "Cancel")
                {
                    return; // Return to main menu
                }
                continue;
            }

            // Process each selected field
            foreach (var fieldDisplay in selectedFields)
            {
                await ProcessConfigurationFieldAsync(fieldDisplay, config, packageVersions, templateName, templateVersion);
            }

            // Update packages string from dictionary
            if (packageVersions.Count > 0)
            {
                var packageParts = new List<string>();
                foreach (var (package, version) in packageVersions)
                {
                    if (string.IsNullOrEmpty(version))
                    {
                        packageParts.Add(package);
                    }
                    else if (version == "--prerelease")
                    {
                        packageParts.Add($"{package} {version}");
                    }
                    else
                    {
                        packageParts.Add($"{package}|{version}");
                    }
                }
                config.Packages = string.Join(",", packageParts);
            }

            // Sync template fields from config to local variables
            templateName = config.TemplateName;
            templateVersion = config.TemplateVersion;

            // Display configuration table
            AnsiConsole.WriteLine();
            DisplayConfigurationTable(config, packageVersions);

            // Ask if they want to edit again or generate
            var nextAction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\nWhat would you like to do next?")
                    .AddChoices(new[] { "Edit configuration", "Generate script", "Cancel" }));

            if (nextAction == "Generate script")
            {
                keepEditing = false;

                // Generate the script
                _logger?.LogInformation("Generating installation script");

                var script = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("green"))
                    .StartAsync("Generating installation script...", async ctx =>
                    {
                        return _scriptGeneratorService.GenerateScript(config.ToViewModel());
                    });

                _logger?.LogInformation("Script generated successfully");

                // Save to history
                _historyService.AddEntry(
                    config,
                    templateName: templateName,
                    description: $"Script for {config.ProjectName ?? "project"}");

                ConsoleDisplay.DisplayGeneratedScript(script);

                // Handle script actions (returns to main menu when done)
                await HandleScriptActionsAsync(script, config, packageVersions, templateName, templateVersion);
            }
            else if (nextAction == "Cancel")
            {
                keepEditing = false;
                return; // Return to main menu
            }
        }
    }

    /// <summary>
    /// Handles script actions (returns to main menu when done)
    /// </summary>
    private async Task HandleScriptActionsAsync(string script, ScriptModel? scriptModel = null, Dictionary<string, string>? packageVersions = null, string? templateName = null, string? templateVersion = null)
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
                        return; // Return to main menu
                    }
                }
            }
            else
            {
                targetDir = Directory.GetCurrentDirectory();
            }

            await _scriptExecutor.RunScriptAsync(script, targetDir);
            // After execution, return to main menu
        }
        else if (action == "Edit")
        {
            if (scriptModel != null && packageVersions != null && templateName != null && templateVersion != null)
            {
                AnsiConsole.MarkupLine("\n[blue]Editing script configuration...[/]\n");

                // Re-enter the configuration editor with existing values
                await EditConfigurationAsync(scriptModel, packageVersions, templateName, templateVersion);
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Cannot edit configuration - configuration data not available.[/]");
            }
        }
        else if (action == "Copy")
        {
            await ClipboardHelper.CopyToClipboardAsync(script, _logger);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleScriptActionsAsync(script, scriptModel, packageVersions, templateName, templateVersion);
            }
            // Otherwise return to main menu
        }
        else if (action == "Save")
        {
            await SaveAsTemplateAsync(scriptModel, packageVersions);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleScriptActionsAsync(script, scriptModel, packageVersions, templateName, templateVersion);
            }
            // Otherwise return to main menu
        }
        else if (action == "Start over")
        {
            // Return to main menu
            AnsiConsole.MarkupLine("\n[blue]Returning to main menu...[/]\n");
            _logger?.LogInformation("User chose to start over - returning to main menu");
        }
    }

    /// <summary>
    /// Shows the Umbraco versions table
    /// </summary>
    private void ShowUmbracoVersionsTable()
    {
        _logger?.LogInformation("Displaying Umbraco versions table");

        AnsiConsole.WriteLine();
        ConsoleDisplay.DisplayUmbracoVersions(_pswConfig);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Shows the list of saved templates
    /// </summary>
    private async Task ShowTemplatesAsync()
    {
        _logger?.LogInformation("Displaying templates list");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Saved Templates[/]\n");

        var templates = await _templateService.GetAllTemplatesAsync();

        if (!templates.Any())
        {
            AnsiConsole.MarkupLine("[dim]No templates found.[/]");
            AnsiConsole.MarkupLine("[dim]You can save a template from the script generation flow.[/]");
        }
        else
        {
            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Description");
            table.AddColumn("Packages");
            table.AddColumn("Created");

            foreach (var template in templates)
            {
                table.AddRow(
                    $"[green]{template.Name}[/]",
                    template.Description ?? "[dim]No description[/]",
                    template.Created.ToString("yyyy-MM-dd")
                );
            }

            AnsiConsole.Write(table);
        }

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Shows the history of generated scripts
    /// </summary>
    private async Task ShowHistoryAsync()
    {
        _logger?.LogInformation("Displaying history list");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Script Generation History[/]\n");

        var history = await _historyService.GetAllHistoryAsync();

        if (!history.Any())
        {
            AnsiConsole.MarkupLine("[dim]No history found.[/]");
            AnsiConsole.MarkupLine("[dim]History is saved automatically when you generate scripts.[/]");
        }
        else
        {
            var table = new Table();
            table.AddColumn("Date");
            table.AddColumn("Project");
            table.AddColumn("Template");
            table.AddColumn("Packages");

            foreach (var entry in history.OrderByDescending(h => h.Timestamp).Take(10))
            {
                table.AddRow(
                    entry.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    entry.ScriptModel?.ProjectName ?? "[dim]N/A[/]",
                    entry.TemplateName ?? "[dim]N/A[/]",
                    entry.ScriptModel?.Packages?.Count().ToString() ?? "0"
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[dim]Showing last {Math.Min(10, history.Count)} entries[/]");
        }

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Clears the package cache
    /// </summary>
    private async Task ClearCacheAsync()
    {
        _logger?.LogInformation("Clearing cache");

        // Clear the cache service
        var cacheService = new CacheService(ttlHours: 1, enabled: true, logger: _logger);
        cacheService.Clear();

        AnsiConsole.MarkupLine("[green]✓ Cache cleared successfully[/]");

        // Repopulate packages
        AnsiConsole.MarkupLine("[dim]Reloading packages...[/]");
        await _packageSelector.PopulateAllPackagesAsync(forceUpdate: true);

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Checks for newer versions of the CLI tool
    /// </summary>
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var result = await _versionCheckService.CheckForUpdateAsync();

            if (result?.IsUpdateAvailable == true)
            {
                AnsiConsole.MarkupLine($"[yellow]📦 A new version is available:[/] [bold]{result.LatestVersion}[/] [dim](current: {result.CurrentVersion})[/]");
                AnsiConsole.MarkupLine($"[dim]   Update with: [green]{result.UpdateCommand}[/][/]");
                AnsiConsole.WriteLine();
            }
        }
        catch
        {
            // Silently fail - don't interrupt the user experience
            _logger?.LogDebug("Version check failed but continuing normally");
        }
    }
}
