using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using PackageCliTool.Models.CommunityTemplates;
using PackageCliTool.Models.Templates;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Validation;
using PackageCliTool.Logging;
using PSW.Shared.Services;
using PackageCliTool.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    private readonly HistoryService _historyService;
    private readonly CommunityTemplateService _communityTemplateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CliModeWorkflow"/> class
    /// </summary>
    /// <param name="apiClient">The API client for making requests</param>
    /// <param name="scriptExecutor">The script executor for running generated scripts</param>
    /// <param name="scriptGeneratorService">The service for generating scripts</param>
    /// <param name="historyService">The service for managing command history</param>
    /// <param name="communityTemplateService">The service for fetching community templates</param>
    /// <param name="logger">Optional logger instance</param>
    public CliModeWorkflow(
        ApiClient apiClient,
        ScriptExecutor scriptExecutor,
        IScriptGeneratorService scriptGeneratorService,
        HistoryService historyService,
        CommunityTemplateService communityTemplateService,
        ILogger? logger = null)
    {
        _apiClient = apiClient;
        _scriptExecutor = scriptExecutor;
        _scriptGeneratorService = scriptGeneratorService;
        _historyService = historyService;
        _communityTemplateService = communityTemplateService;
        _logger = logger;
    }

    /// <summary>
    /// Runs the CLI mode workflow
    /// </summary>
    public async Task RunAsync(CommandLineOptions options)
    {
        // Check for community template command first
        if (options.IsCommunityTemplateCommand())
        {
            await HandleCommunityTemplateAsync(options);
            return;
        }

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
        var machineReadable = OutputHelper.IsMachineReadable(options.OutputFormat) || options.ScriptOnly;

        if (!machineReadable)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold blue]Generating Default Script[/]\n");
            AnsiConsole.MarkupLine("[dim]Using default configuration (latest stable Umbraco with clean starter kit)[/]");
            AnsiConsole.WriteLine();
        }

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
            EnableContentDeliveryApi = options.EnableContentDeliveryApi.HasValue
                ? options.EnableContentDeliveryApi.Value
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

        // Apply --no-run flag
        if (options.NoRun)
        {
            model.SkipDotnetRun = true;
        }

        // Dry-run: validate and show config without generating
        if (options.DryRun)
        {
            WriteDryRunResult(options, model);
            return;
        }

        string script;
        if (machineReadable)
        {
            script = _scriptGeneratorService.GenerateScript(model.ToViewModel());
        }
        else
        {
            script = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Generating default installation script...", async ctx =>
                {
                    return _scriptGeneratorService.GenerateScript(model.ToViewModel());
                });
        }

        _logger?.LogInformation("Default script generated successfully");

        // Save to history
        _historyService.AddEntry(
            model,
            templateName: model.TemplateName,
            description: $"Default script for {model.ProjectName}");

        // Handle --save-only: write script to file and exit immediately
        if (options.SaveOnly)
        {
            await SaveScriptToFileAsync(script, options);
            return;
        }

        OutputHelper.WriteScript(script, options.ScriptOnly ? OutputFormat.Plain : options.OutputFormat, model, "Generated Default Installation Script");

        // Handle auto-run or interactive run
        await HandleScriptExecutionAsync(script, options);
    }

    /// <summary>
    /// Generates a custom script from command-line options
    /// </summary>
    private async Task GenerateCustomScriptFromOptionsAsync(CommandLineOptions options)
    {
        _logger?.LogInformation("Generating custom script from command-line options");
        var machineReadable = OutputHelper.IsMachineReadable(options.OutputFormat) || options.ScriptOnly;

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
                : string.Empty,
            TemplateVersion = !string.IsNullOrWhiteSpace(options.TemplateVersion)
                ? options.TemplateVersion
                : string.Empty,
            ProjectName = !string.IsNullOrWhiteSpace(options.ProjectName)
                ? options.ProjectName
                : string.Empty,
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
            EnableContentDeliveryApi = options.EnableContentDeliveryApi.HasValue
                ? options.EnableContentDeliveryApi.Value
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

        // Apply --no-run flag
        if (options.NoRun)
        {
            model.SkipDotnetRun = true;
        }

        // Dry-run: validate and show config without generating
        if (options.DryRun)
        {
            WriteDryRunResult(options, model);
            return;
        }

        // Generate the script
        _logger?.LogInformation("Generating installation script via API");

        string script;
        if (machineReadable)
        {
            script = _scriptGeneratorService.GenerateScript(model.ToViewModel());
        }
        else
        {
            script = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Generating installation script...", async ctx =>
                {
                    return _scriptGeneratorService.GenerateScript(model.ToViewModel());
                });
        }

        _logger?.LogInformation("Script generated successfully");

        // Save to history
        _historyService.AddEntry(
            model,
            templateName: model.TemplateName,
            description: $"Custom script for {model.ProjectName ?? "project"}");

        // Handle --save-only: write script to file and exit immediately
        if (options.SaveOnly)
        {
            await SaveScriptToFileAsync(script, options);
            return;
        }

        OutputHelper.WriteScript(script, options.ScriptOnly ? OutputFormat.Plain : options.OutputFormat, model);

        // Handle auto-run or interactive run
        await HandleScriptExecutionAsync(script, options);
    }

    private void HandlePackages(CommandLineOptions options, ScriptModel model)
    {
        var machineReadable = OutputHelper.IsMachineReadable(options.OutputFormat) || options.ScriptOnly;

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
                            if (!machineReadable)
                                AnsiConsole.MarkupLine($"[green]✓[/] Using {packageName} version {version}");
                            _logger?.LogDebug("Added package {Package} with version {Version}", packageName, version);
                        }
                        else
                        {
                            if (!machineReadable)
                                ErrorHandler.Warning($"Invalid package format: {entry}, skipping...", _logger);
                        }
                    }
                    else
                    {
                        // No version specified, use package name without version
                        var packageName = entry.Trim();
                        InputValidator.ValidatePackageName(packageName);

                        processedPackages.Add(packageName);
                        if (!machineReadable)
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
        var machineReadable = OutputHelper.IsMachineReadable(options.OutputFormat) || options.ScriptOnly;

        // Handle starter kit package
        if (!string.IsNullOrWhiteSpace(options.StarterKitPackage))
        {
            _logger?.LogDebug("Processing starter kit package: {StarterKitPackage}", options.StarterKitPackage);

            // Validate starter kit package name
            InputValidator.ValidatePackageName(options.StarterKitPackage);

            model.IncludeStarterKit = true;

            // Handle starter kit version if specified
            if (!string.IsNullOrWhiteSpace(options.StarterKitVersion))
            {
                // Validate version
                InputValidator.ValidateVersion(options.StarterKitVersion);

                // Store with --version flag for the model
                model.StarterKitPackage = $"{options.StarterKitPackage} --version {options.StarterKitVersion}";

                if (!machineReadable)
                    AnsiConsole.MarkupLine($"[green]✓[/] Using starter kit {options.StarterKitPackage} version {options.StarterKitVersion}");
                _logger?.LogDebug("Using starter kit {Package} with version {Version}", options.StarterKitPackage, options.StarterKitVersion);
            }
            else
            {
                model.StarterKitPackage = options.StarterKitPackage;

                if (!machineReadable)
                    AnsiConsole.MarkupLine($"[green]✓[/] Using starter kit {options.StarterKitPackage} (latest version)");
                _logger?.LogDebug("Using starter kit {Package} with latest version", options.StarterKitPackage);
            }
        }
    }

    private void HandleTemplatePackage(CommandLineOptions options, ScriptModel model)
    {
        var machineReadable = OutputHelper.IsMachineReadable(options.OutputFormat) || options.ScriptOnly;

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

                if (!machineReadable)
                    AnsiConsole.MarkupLine($"[green]✓[/] Using {options.TemplatePackageName} version {options.TemplateVersion}");
                _logger?.LogDebug("Using template {Template} with version {Version}", options.TemplatePackageName, options.TemplateVersion);
            }
            else
            {
                if (!machineReadable)
                    AnsiConsole.MarkupLine($"[green]✓[/] Using {options.TemplatePackageName} (latest version)");
                _logger?.LogDebug("Using template {Template} with latest version", options.TemplatePackageName);
            }
        }
    }

    /// <summary>
    /// Saves the generated script to a file and exits
    /// </summary>
    private async Task SaveScriptToFileAsync(string script, CommandLineOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.OutputFile))
        {
            AnsiConsole.MarkupLine("[red]Error: --save-only requires --output-file <file> to specify the output file path[/]");
            Environment.ExitCode = 1;
            return;
        }

        var outputPath = Path.GetFullPath(options.OutputFile);
        var outputDir = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
            _logger?.LogInformation("Created output directory: {Directory}", outputDir);
        }

        await File.WriteAllTextAsync(outputPath, script);

        _logger?.LogInformation("Script saved to: {Path}", outputPath);
        AnsiConsole.MarkupLine($"[green]✓ Script saved to:[/] {outputPath}");
    }

    /// <summary>
    /// Handles script execution based on options
    /// </summary>
    private async Task HandleScriptExecutionAsync(string script, CommandLineOptions options)
    {
        // In machine-readable or non-interactive mode, skip post-generation prompts
        if (OutputHelper.IsMachineReadable(options.OutputFormat) || options.ScriptOnly || options.NonInteractive)
        {
            // Only auto-run if explicitly requested
            if (options.AutoRun || !string.IsNullOrWhiteSpace(options.RunDirectory))
            {
                var targetDir = !string.IsNullOrWhiteSpace(options.RunDirectory)
                    ? options.RunDirectory
                    : Directory.GetCurrentDirectory();

                InputValidator.ValidateDirectoryPath(targetDir);
                targetDir = Path.GetFullPath(targetDir);

                if (!Directory.Exists(targetDir))
                {
                    _logger?.LogInformation("Creating directory: {Directory}", targetDir);
                    Directory.CreateDirectory(targetDir);
                }

                await _scriptExecutor.RunScriptAsync(script, targetDir);
            }
            return;
        }

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

    /// <summary>
    /// Handles community template commands
    /// </summary>
    private async Task HandleCommunityTemplateAsync(CommandLineOptions options)
    {
        // Special case: list all community templates
        if (options.CommunityTemplate!.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            await ListCommunityTemplatesAsync();
            return;
        }

        // Load and execute community template
        await LoadAndExecuteCommunityTemplateAsync(options);
    }

    /// <summary>
    /// Lists all available community templates
    /// </summary>
    private async Task ListCommunityTemplatesAsync()
    {
        try
        {
            _logger?.LogInformation("Listing community templates");

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold blue]Available Community Templates[/]\n");

            var templates = await _communityTemplateService.GetAllTemplatesAsync();

            if (!templates.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No community templates available at this time.[/]");
                return;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("[bold]Template[/]").LeftAligned());
            table.AddColumn(new TableColumn("[bold]Description[/]").LeftAligned());
            table.AddColumn(new TableColumn("[bold]Author[/]").LeftAligned());
            table.AddColumn(new TableColumn("[bold]Tags[/]").LeftAligned());

            foreach (var template in templates)
            {
                table.AddRow(
                    $"[green]{template.DisplayName}[/]\n[dim]{template.Name}[/]",
                    template.Description,
                    template.Author,
                    string.Join(", ", template.Tags.Select(t => $"[dim]{t}[/]"))
                );
            }

            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]Found {templates.Count} template(s)[/]");
            AnsiConsole.MarkupLine("[dim]Use: psw --community-template <name> to use a template[/]");
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to list community templates");
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }

    /// <summary>
    /// Loads and executes a community template
    /// </summary>
    private async Task LoadAndExecuteCommunityTemplateAsync(CommandLineOptions options)
    {
        try
        {
            _logger?.LogInformation("Loading community template: {TemplateName}", options.CommunityTemplate);

            AnsiConsole.WriteLine();
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start($"[yellow]Fetching community template '{options.CommunityTemplate}'...[/]", ctx =>
                {
                    // UI feedback only
                });

            // Load the template
            var template = await _communityTemplateService.GetTemplateAsync(options.CommunityTemplate!);

            AnsiConsole.MarkupLine($"[green]✓[/] Loaded template: [bold]{template.Metadata.Name}[/]");
            AnsiConsole.MarkupLine($"[dim]  {template.Metadata.Description}[/]");
            AnsiConsole.WriteLine();

            // Convert template to ScriptModel
            var model = ConvertTemplateToScriptModel(template, options);

            // Apply --no-run flag
            if (options.NoRun)
            {
                model.SkipDotnetRun = true;
            }

            // Generate script
            var script = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Generating installation script...", async ctx =>
                {
                    return await Task.Run(() => _scriptGeneratorService.GenerateScript(model.ToViewModel()));
                });

            _logger?.LogInformation("Script generated successfully from community template");

            // Save to history
            _historyService.AddEntry(
                model,
                templateName: template.Metadata.Name,
                description: $"Community template: {template.Metadata.Description}");

            // Handle --save-only: write script to file and exit immediately
            if (options.SaveOnly)
            {
                await SaveScriptToFileAsync(script, options);
                return;
            }

            // Display script
            ConsoleDisplay.DisplayGeneratedScript(script, $"Generated Script from '{template.Metadata.Name}'");

            // Handle execution
            await HandleScriptExecutionAsync(script, options);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load community template: {TemplateName}", options.CommunityTemplate);
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }

    /// <summary>
    /// Converts a Template to ScriptModel, applying CLI overrides
    /// </summary>
    private ScriptModel ConvertTemplateToScriptModel(Template template, CommandLineOptions options)
    {
        var config = template.Configuration;

        // Convert packages to comma-separated string
        var packagesStr = config.Packages.Any()
            ? string.Join(",", config.Packages.Select(p => $"{p.Name}|{p.Version}"))
            : null;

        // Create base model from template
        var model = new ScriptModel
        {
            TemplateName = config.Template.Name,
            TemplateVersion = config.Template.Version,
            ProjectName = config.Project.Name,
            CreateSolutionFile = config.Project.CreateSolution,
            SolutionName = config.Project.SolutionName ?? config.Project.Name,
            IncludeStarterKit = config.StarterKit.Enabled,
            StarterKitPackage = config.StarterKit.Package,
            IncludeDockerfile = config.Docker.Dockerfile,
            IncludeDockerCompose = config.Docker.DockerCompose,
            EnableContentDeliveryApi = config.Docker.EnableContentDeliveryApi,
            CanIncludeDocker = config.Docker.Dockerfile || config.Docker.DockerCompose,
            UseUnattendedInstall = config.Unattended.Enabled,
            DatabaseType = config.Unattended.Database.Type,
            ConnectionString = config.Unattended.Database.ConnectionString,
            UserFriendlyName = config.Unattended.Admin.Name,
            UserEmail = config.Unattended.Admin.Email,
            UserPassword = config.Unattended.Admin.Password,
            OnelinerOutput = config.Output.Oneliner,
            RemoveComments = config.Output.RemoveComments,
            Packages = packagesStr
        };

        // Apply CLI overrides (same as local templates)
        if (!string.IsNullOrWhiteSpace(options.ProjectName))
            model.ProjectName = options.ProjectName;

        if (options.CreateSolution.HasValue)
            model.CreateSolutionFile = options.CreateSolution.Value;

        if (!string.IsNullOrWhiteSpace(options.SolutionName))
            model.SolutionName = options.SolutionName;

        if (!string.IsNullOrWhiteSpace(options.TemplatePackageName))
            model.TemplateName = options.TemplatePackageName;

        if (!string.IsNullOrWhiteSpace(options.TemplateVersion))
            model.TemplateVersion = options.TemplateVersion;

        // Add additional packages from CLI if specified
        if (!string.IsNullOrWhiteSpace(options.Packages))
        {
            if (string.IsNullOrWhiteSpace(model.Packages))
            {
                model.Packages = options.Packages;
            }
            else
            {
                model.Packages += "," + options.Packages;
            }
        }

        if (options.IncludeStarterKit.HasValue)
            model.IncludeStarterKit = options.IncludeStarterKit.Value;

        if (!string.IsNullOrWhiteSpace(options.StarterKitPackage))
            model.StarterKitPackage = options.StarterKitPackage;

        if (options.IncludeDockerfile.HasValue)
            model.IncludeDockerfile = options.IncludeDockerfile.Value;

        if (options.IncludeDockerCompose.HasValue)
            model.IncludeDockerCompose = options.IncludeDockerCompose.Value;

        if (options.EnableContentDeliveryApi.HasValue)
            model.EnableContentDeliveryApi = options.EnableContentDeliveryApi.Value;

        // Update CanIncludeDocker based on the flags
        model.CanIncludeDocker = model.IncludeDockerfile || model.IncludeDockerCompose;

        if (options.UseUnattended.HasValue)
            model.UseUnattendedInstall = options.UseUnattended.Value;

        if (!string.IsNullOrWhiteSpace(options.DatabaseType))
            model.DatabaseType = options.DatabaseType;

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            model.ConnectionString = options.ConnectionString;

        if (!string.IsNullOrWhiteSpace(options.AdminName))
            model.UserFriendlyName = options.AdminName;

        if (!string.IsNullOrWhiteSpace(options.AdminEmail))
            model.UserEmail = options.AdminEmail;

        if (!string.IsNullOrWhiteSpace(options.AdminPassword))
            model.UserPassword = options.AdminPassword;

        if (options.OnelinerOutput.HasValue)
            model.OnelinerOutput = options.OnelinerOutput.Value;

        if (options.RemoveComments.HasValue)
            model.RemoveComments = options.RemoveComments.Value;

        return model;
    }

    /// <summary>
    /// Outputs a dry-run result showing validated configuration without generating a script
    /// </summary>
    private void WriteDryRunResult(CommandLineOptions options, ScriptModel model)
    {
        _logger?.LogInformation("Dry-run: validation passed, displaying configuration");

        if (options.OutputFormat == OutputFormat.Json)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var result = new
            {
                success = true,
                dryRun = true,
                message = "Validation passed. Configuration is valid.",
                configuration = new
                {
                    templateName = model.TemplateName,
                    templateVersion = model.TemplateVersion,
                    projectName = model.ProjectName,
                    solutionName = model.SolutionName,
                    createSolutionFile = model.CreateSolutionFile,
                    packages = model.Packages,
                    includeStarterKit = model.IncludeStarterKit,
                    starterKitPackage = model.StarterKitPackage,
                    includeDockerfile = model.IncludeDockerfile,
                    includeDockerCompose = model.IncludeDockerCompose,
                    enableContentDeliveryApi = model.EnableContentDeliveryApi,
                    useUnattendedInstall = model.UseUnattendedInstall,
                    databaseType = model.DatabaseType,
                    onelinerOutput = model.OnelinerOutput,
                    removeComments = model.RemoveComments
                }
            };
            Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
        }
        else if (options.OutputFormat == OutputFormat.Plain || options.ScriptOnly)
        {
            Console.WriteLine("Dry-run: validation passed");
            Console.WriteLine($"Template: {model.TemplateName} {model.TemplateVersion}");
            Console.WriteLine($"Project: {model.ProjectName}");
            if (!string.IsNullOrWhiteSpace(model.Packages))
                Console.WriteLine($"Packages: {model.Packages}");
            if (model.UseUnattendedInstall)
                Console.WriteLine($"Database: {model.DatabaseType}");
        }
        else
        {
            AnsiConsole.MarkupLine("[green]✓ Dry-run: validation passed[/]");
            AnsiConsole.MarkupLine("[dim]Configuration is valid. Remove --dry-run to generate the script.[/]");
            AnsiConsole.WriteLine();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title("[bold green]Validated Configuration[/]");

            table.AddColumn("[bold]Setting[/]");
            table.AddColumn("[bold]Value[/]");

            table.AddRow("Template", $"{model.TemplateName} {model.TemplateVersion}".Trim());
            table.AddRow("Project Name", model.ProjectName ?? "N/A");
            if (model.CreateSolutionFile)
                table.AddRow("Solution Name", model.SolutionName ?? "N/A");
            if (!string.IsNullOrWhiteSpace(model.Packages))
                table.AddRow("Packages", model.Packages);
            if (model.IncludeStarterKit)
                table.AddRow("Starter Kit", model.StarterKitPackage ?? "N/A");
            if (model.IncludeDockerfile)
                table.AddRow("Dockerfile", "Yes");
            if (model.IncludeDockerCompose)
                table.AddRow("Docker Compose", "Yes");
            if (model.UseUnattendedInstall)
            {
                table.AddRow("Unattended Install", "Yes");
                table.AddRow("Database Type", model.DatabaseType ?? "N/A");
            }

            AnsiConsole.Write(table);
        }
    }
}
