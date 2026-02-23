using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using PackageCliTool.Models.Templates;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Validation;
using PackageCliTool.Exceptions;
using PackageCliTool.Extensions;
using PSW.Shared.Services;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates template-related workflows
/// </summary>
public class TemplateWorkflow
{
    private readonly TemplateService _templateService;
    private readonly ScriptExecutor _scriptExecutor;
    private readonly IScriptGeneratorService _scriptGeneratorService;
    private readonly HistoryService _historyService;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateWorkflow"/> class
    /// </summary>
    /// <param name="templateService">The service for managing templates</param>
    /// <param name="scriptExecutor">The script executor for running generated scripts</param>
    /// <param name="scriptGeneratorService">The service for generating scripts</param>
    /// <param name="historyService">The service for managing command history</param>
    /// <param name="logger">Optional logger instance</param>
    public TemplateWorkflow(
        TemplateService templateService,
        ScriptExecutor scriptExecutor,
        IScriptGeneratorService scriptGeneratorService,
        HistoryService historyService,
        ILogger? logger = null)
    {
        _templateService = templateService;
        _scriptExecutor = scriptExecutor;
        _scriptGeneratorService = scriptGeneratorService;
        _historyService = historyService;
        _logger = logger;
    }

    /// <summary>
    /// Runs a template command based on options
    /// </summary>
    public async Task RunAsync(CommandLineOptions options)
    {
        var command = options.TemplateCommand?.ToLower();

        // If no command was provided (e.g., just "psw template"), show help
        if (string.IsNullOrEmpty(command))
        {
            AnsiConsole.MarkupLine("[red]Error: Missing template subcommand[/]\n");
            ConsoleDisplay.DisplayTemplateHelp();
            Environment.ExitCode = 1;
            return;
        }

        switch (command)
        {
            case "save":
                await SaveTemplateAsync(options);
                break;

            case "load":
                await LoadTemplateAsync(options);
                break;

            case "list":
                ListTemplates(options);
                break;

            case "delete":
                DeleteTemplate(options);
                break;

            case "export":
                await ExportTemplateAsync(options);
                break;

            case "import":
                await ImportTemplateAsync(options);
                break;

            case "validate":
                await ValidateTemplateAsync(options);
                break;

            default:
                AnsiConsole.MarkupLine($"[red]Error: Unknown template command '{command}'[/]\n");
                ConsoleDisplay.DisplayTemplateHelp();
                Environment.ExitCode = 1;
                break;
        }
    }

    /// <summary>
    /// Saves a template from current configuration
    /// </summary>
    private async Task SaveTemplateAsync(CommandLineOptions options)
    {
        var name = options.TemplateName;

        if (string.IsNullOrWhiteSpace(name))
        {
            name = AnsiConsole.Ask<string>("Enter [green]template name[/]:");
        }

        _logger?.LogInformation("Saving template: {Name}", name);

        var description = options.TemplateDescription ?? AnsiConsole.Ask<string>(
            "Enter [green]template description[/] (optional):",
            string.Empty);

        // Create template from options
        var template = _templateService.FromCommandLineOptions(options, name, description);

        // Add tags if provided
        if (options.TemplateTags.Count > 0)
        {
            template.Metadata.Tags = options.TemplateTags;
        }
        else
        {
            var tagsInput = AnsiConsole.Ask<string>("Enter [green]tags[/] (comma-separated, optional):", string.Empty);
            if (!string.IsNullOrWhiteSpace(tagsInput))
            {
                template.Metadata.Tags = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
            }
        }

        // Validate template
        TemplateValidator.ValidateAndThrow(template);

        // Check if template already exists
        if (_templateService.TemplateExists(name))
        {
            var overwrite = AnsiConsole.Confirm($"Template [yellow]{name}[/] already exists. Overwrite?", false);
            if (!overwrite)
            {
                AnsiConsole.MarkupLine("[yellow]Template save cancelled.[/]");
                return;
            }
        }

        // Save template
        await _templateService.SaveTemplateAsync(template);

        AnsiConsole.MarkupLine($"[green]✓ Template saved:[/] {name}");
        _logger?.LogInformation("Template saved successfully: {Name}", name);
    }

    /// <summary>
    /// Loads and executes a template
    /// </summary>
    private async Task LoadTemplateAsync(CommandLineOptions options)
    {
        var name = options.TemplateName;

        if (string.IsNullOrWhiteSpace(name))
        {
            // Show list and prompt for selection
            var templates = _templateService.ListTemplates();

            if (templates.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No templates found. Create one with 'psw template save'[/]");
                return;
            }

            name = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a [green]template[/] to load:")
                    .AddChoices(templates.Select(t => t.Name)));
        }

        _logger?.LogInformation("Loading template: {Name}", name);

        // Load template
        var template = await _templateService.LoadTemplateAsync(name);

        AnsiConsole.MarkupLine($"[green]✓ Template loaded:[/] {name}");

        // Apply command-line overrides (similar to how --default works)
        var overrides = new Dictionary<string, object>();

        // Template package overrides
        if (!string.IsNullOrWhiteSpace(options.TemplatePackageName))
            overrides["TemplateName"] = options.TemplatePackageName;

        if (!string.IsNullOrWhiteSpace(options.TemplateVersion))
            overrides["TemplateVersion"] = options.TemplateVersion;

        // Project overrides
        if (!string.IsNullOrWhiteSpace(options.ProjectName))
            overrides["ProjectName"] = options.ProjectName;

        if (options.CreateSolution.HasValue)
            overrides["CreateSolution"] = options.CreateSolution.Value;

        if (!string.IsNullOrWhiteSpace(options.SolutionName))
            overrides["SolutionName"] = options.SolutionName;

        // Package overrides
        if (!string.IsNullOrWhiteSpace(options.Packages))
            overrides["Packages"] = options.Packages;

        // Starter kit overrides
        if (options.IncludeStarterKit.HasValue)
            overrides["IncludeStarterKit"] = options.IncludeStarterKit.Value;

        if (!string.IsNullOrWhiteSpace(options.StarterKitPackage))
            overrides["StarterKitPackage"] = options.StarterKitPackage;

        if (!string.IsNullOrWhiteSpace(options.StarterKitVersion))
            overrides["StarterKitVersion"] = options.StarterKitVersion;

        // Docker overrides
        if (options.IncludeDockerfile.HasValue)
            overrides["IncludeDockerfile"] = options.IncludeDockerfile.Value;

        if (options.IncludeDockerCompose.HasValue)
            overrides["IncludeDockerCompose"] = options.IncludeDockerCompose.Value;

        // Unattended install overrides
        if (options.UseUnattended.HasValue)
            overrides["UseUnattended"] = options.UseUnattended.Value;

        if (!string.IsNullOrWhiteSpace(options.DatabaseType))
            overrides["DatabaseType"] = options.DatabaseType;

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            overrides["ConnectionString"] = options.ConnectionString;

        if (!string.IsNullOrWhiteSpace(options.AdminName))
            overrides["AdminName"] = options.AdminName;

        if (!string.IsNullOrWhiteSpace(options.AdminEmail))
            overrides["AdminEmail"] = options.AdminEmail;

        if (!string.IsNullOrWhiteSpace(options.AdminPassword))
            overrides["AdminPassword"] = options.AdminPassword;

        // Output overrides
        if (options.OnelinerOutput.HasValue)
            overrides["OnelinerOutput"] = options.OnelinerOutput.Value;

        if (options.RemoveComments.HasValue)
            overrides["RemoveComments"] = options.RemoveComments.Value;

        if (options.IncludePrerelease.HasValue)
            overrides["IncludePrerelease"] = options.IncludePrerelease.Value;

        // Execution overrides
        if (options.AutoRun)
            overrides["AutoRun"] = true;

        if (!string.IsNullOrWhiteSpace(options.RunDirectory))
            overrides["RunDirectory"] = options.RunDirectory;

        // Convert template to script model
        var scriptModel = _templateService.ToScriptModel(template, overrides.Count > 0 ? overrides : null);

        // Handle password prompt if needed
        if (scriptModel.UseUnattendedInstall && string.IsNullOrWhiteSpace(scriptModel.UserPassword))
        {
            scriptModel.UserPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter [green]admin password[/] (min 10 characters):")
                    .PromptStyle("red")
                    .Secret()
                    .Validate(pwd => pwd.Length >= 10 ? ValidationResult.Success() : ValidationResult.Error("Password must be at least 10 characters")));
        }

        // Apply --build-only flag
        if (options.BuildOnly)
        {
            scriptModel.SkipDotnetRun = true;
            overrides["BuildOnly"] = true;
        }

        // Display configuration summary
        DisplayConfigurationSummary(template, overrides);

        // Generate script
        _logger?.LogInformation("Generating script from template");

        var script = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Generating installation script...", async ctx =>
            {
                return await Task.Run(() => _scriptGeneratorService.GenerateScript(scriptModel.ToViewModel()));
            });

        _logger?.LogInformation("Script generated successfully");

        // Save to history
        _historyService.AddEntry(
            scriptModel,
            templateName: template.Metadata.Name,
            description: $"From template: {template.Metadata.Name}");

        // Handle --save-only: write script to file and exit immediately
        if (options.SaveOnly)
        {
            await SaveScriptToFileAsync(script, options);
            return;
        }

        // Display script
        ConsoleDisplay.DisplayGeneratedScript(script, "Generated Installation Script");

        // Build packageVersions dictionary for template saving
        var packageVersions = new Dictionary<string, string>();
        foreach (var package in template.Configuration.Packages)
        {
            var version = package.Version.ToLower() switch
            {
                "latest" => "",
                "prerelease" => "--prerelease",
                _ => package.Version
            };
            packageVersions[package.Name] = version;
        }

        // Handle auto-run or run-dir if specified (similar to CliModeWorkflow)
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
            // Handle script actions interactively
            await HandleScriptActionsAsync(script, scriptModel, packageVersions, template.Metadata.Name);
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
    /// Handles script actions after generation
    /// </summary>
    private async Task HandleScriptActionsAsync(string script, ScriptModel scriptModel, Dictionary<string, string> packageVersions, string templateName)
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
            AnsiConsole.MarkupLine("[yellow]To edit the configuration, please use the interactive mode.[/]");
            AnsiConsole.MarkupLine("[dim]Run 'psw' without arguments to enter interactive mode.[/]");
        }
        else if (action == "Copy")
        {
            await ClipboardHelper.CopyToClipboardAsync(script, _logger);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleScriptActionsAsync(script, scriptModel, packageVersions, templateName);
            }
        }
        else if (action == "Save")
        {
            await SaveAsTemplateAsync(scriptModel, packageVersions);

            // Ask if they want to do something else with the script
            var continueAction = AnsiConsole.Confirm("\nWould you like to do something else with this script?", false);
            if (continueAction)
            {
                await HandleScriptActionsAsync(script, scriptModel, packageVersions, templateName);
            }
        }
        else if (action == "Start over")
        {
            AnsiConsole.MarkupLine("[yellow]To start over, please re-run the command with different options.[/]");
        }
    }

    /// <summary>
    /// Saves the current script configuration as a template
    /// </summary>
    private async Task SaveAsTemplateAsync(ScriptModel scriptModel, Dictionary<string, string> packageVersions)
    {
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
    /// Lists all available templates
    /// </summary>
    private void ListTemplates(CommandLineOptions options)
    {
        _logger?.LogInformation("Listing templates");

        var templates = _templateService.ListTemplates();

        if (templates.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No templates found.[/]");
            AnsiConsole.MarkupLine("Create a template with: [green]psw template save <name>[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn("[bold]Name[/]");
        table.AddColumn("[bold]Description[/]");
        table.AddColumn("[bold]Version[/]");
        table.AddColumn("[bold]Tags[/]");
        table.AddColumn("[bold]Modified[/]");

        foreach (var template in templates)
        {
            table.AddRow(
                $"[green]{template.Name}[/]",
                template.Description,
                template.Version,
                string.Join(", ", template.Tags),
                template.Modified.ToString("yyyy-MM-dd HH:mm")
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]Total: {templates.Count} template(s)[/]");
        AnsiConsole.MarkupLine("[dim]Use 'psw template load <name>' to use a template[/]");
    }

    /// <summary>
    /// Deletes a template
    /// </summary>
    private void DeleteTemplate(CommandLineOptions options)
    {
        var name = options.TemplateName;

        if (string.IsNullOrWhiteSpace(name))
        {
            name = AnsiConsole.Ask<string>("Enter [green]template name[/] to delete:");
        }

        _logger?.LogInformation("Deleting template: {Name}", name);

        var confirm = AnsiConsole.Confirm($"Are you sure you want to delete template [yellow]{name}[/]?", false);

        if (!confirm)
        {
            AnsiConsole.MarkupLine("[yellow]Delete cancelled.[/]");
            return;
        }

        _templateService.DeleteTemplate(name);
        AnsiConsole.MarkupLine($"[green]✓ Template deleted:[/] {name}");
    }

    /// <summary>
    /// Exports a template to a file
    /// </summary>
    private async Task ExportTemplateAsync(CommandLineOptions options)
    {
        var name = options.TemplateName;

        if (string.IsNullOrWhiteSpace(name))
        {
            name = AnsiConsole.Ask<string>("Enter [green]template name[/] to export:");
        }

        var outputPath = options.TemplateFile ?? $"{name}.yaml";

        _logger?.LogInformation("Exporting template {Name} to {Path}", name, outputPath);

        await _templateService.ExportTemplateAsync(name, outputPath);

        AnsiConsole.MarkupLine($"[green]✓ Template exported to:[/] {outputPath}");
    }

    /// <summary>
    /// Imports a template from a file
    /// </summary>
    private async Task ImportTemplateAsync(CommandLineOptions options)
    {
        var filePath = options.TemplateFile;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = AnsiConsole.Ask<string>("Enter [green]template file path[/]:");
        }

        _logger?.LogInformation("Importing template from {Path}", filePath);

        var newName = options.TemplateName;
        var template = await _templateService.ImportTemplateAsync(filePath, newName);

        AnsiConsole.MarkupLine($"[green]✓ Template imported:[/] {template.Metadata.Name}");
    }

    /// <summary>
    /// Validates a template file
    /// </summary>
    private async Task ValidateTemplateAsync(CommandLineOptions options)
    {
        var filePath = options.TemplateFile;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = AnsiConsole.Ask<string>("Enter [green]template file path[/]:");
        }

        _logger?.LogInformation("Validating template: {Path}", filePath);

        try
        {
            var template = await _templateService.ImportTemplateAsync(filePath, $"temp-{Guid.NewGuid()}");
            var errors = TemplateValidator.Validate(template);

            // Delete the temp template
            _templateService.DeleteTemplate(template.Metadata.Name);

            if (errors.Count == 0)
            {
                AnsiConsole.MarkupLine("[green]✓ Template is valid[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Template validation warnings:[/]");
                foreach (var error in errors)
                {
                    AnsiConsole.MarkupLine($"  [yellow]•[/] {error}");
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Template validation failed:[/] {ex.Message}");
        }
    }

    /// <summary>
    /// Displays configuration summary before generating script
    /// </summary>
    private void DisplayConfigurationSummary(Template template, Dictionary<string, object> overrides)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .Title("[bold yellow]Configuration Summary[/]");

        table.AddColumn("[bold]Setting[/]");
        table.AddColumn("[bold]Value[/]");

        table.AddRow("Template", template.Metadata.Name);
        table.AddRow("Project", template.Configuration.Project.Name);

        if (overrides.Count > 0)
        {
            table.AddRow("[dim]Overrides[/]", $"[dim]{overrides.Count} applied[/]");

            foreach (var (key, value) in overrides)
            {
                table.AddRow($"  └─ {key}", value.ToString() ?? "");
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
