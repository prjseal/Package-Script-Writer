using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Models.Templates;
using PackageCliTool.Services;
using PackageCliTool.Validation;
using PackageCliTool.Exceptions;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates template-related workflows
/// </summary>
public class TemplateWorkflow
{
    private readonly TemplateService _templateService;
    private readonly ApiClient _apiClient;
    private readonly ScriptExecutor _scriptExecutor;
    private readonly ILogger? _logger;

    public TemplateWorkflow(
        TemplateService templateService,
        ApiClient apiClient,
        ScriptExecutor scriptExecutor,
        ILogger? logger = null)
    {
        _templateService = templateService;
        _apiClient = apiClient;
        _scriptExecutor = scriptExecutor;
        _logger = logger;
    }

    /// <summary>
    /// Runs a template command based on options
    /// </summary>
    public async Task RunAsync(CommandLineOptions options)
    {
        var command = options.TemplateCommand?.ToLower();

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

            case "show":
                await ShowTemplateAsync(options);
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
                AnsiConsole.MarkupLine($"[red]Unknown template command: {command}[/]");
                AnsiConsole.MarkupLine("Use [green]psw template --help[/] for available commands");
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

        // Convert overrides dictionary
        var overrides = new Dictionary<string, object>();
        foreach (var (key, value) in options.TemplateOverrides)
        {
            overrides[key] = value;
        }

        // Apply command-line overrides if provided
        if (!string.IsNullOrWhiteSpace(options.ProjectName))
        {
            overrides["ProjectName"] = options.ProjectName;
        }

        if (!string.IsNullOrWhiteSpace(options.RunDirectory))
        {
            overrides["RunDirectory"] = options.RunDirectory;
        }

        if (options.AutoRun)
        {
            overrides["AutoRun"] = true;
        }

        // Convert template to script model
        var scriptModel = _templateService.ToGeneratorApiRequest(template, overrides.Count > 0 ? overrides : null);

        // Handle password prompt if needed
        if (scriptModel.UseUnattendedInstall && string.IsNullOrWhiteSpace(scriptModel.UserPassword))
        {
            scriptModel.UserPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter [green]admin password[/] (min 10 characters):")
                    .PromptStyle("red")
                    .Secret()
                    .Validate(pwd => pwd.Length >= 10 ? ValidationResult.Success() : ValidationResult.Error("Password must be at least 10 characters")));
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
                return await _apiClient.GenerateScriptAsync(scriptModel);
            });

        _logger?.LogInformation("Script generated successfully");

        AnsiConsole.MarkupLine("\n[green]✓ Script generated from template[/]");

        // Display script
        var panel = new Panel(script)
        {
            Header = new PanelHeader("Generated Installation Script"),
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Green),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);

        // Handle execution
        if (scriptModel.UseUnattendedInstall && template.Configuration.Execution.AutoRun)
        {
            var runDir = template.Configuration.Execution.RunDirectory;

            if (!string.IsNullOrWhiteSpace(runDir) && runDir != ".")
            {
                if (!Directory.Exists(runDir))
                {
                    Directory.CreateDirectory(runDir);
                    AnsiConsole.MarkupLine($"[green]✓ Created directory:[/] {runDir}");
                }

                await _scriptExecutor.RunScriptAsync(script, runDir);
            }
            else
            {
                await _scriptExecutor.RunScriptAsync(script, Directory.GetCurrentDirectory());
            }
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
        AnsiConsole.MarkupLine("[dim]Use 'psw template show <name>' to view details[/]");
        AnsiConsole.MarkupLine("[dim]Use 'psw template load <name>' to use a template[/]");
    }

    /// <summary>
    /// Shows details of a specific template
    /// </summary>
    private async Task ShowTemplateAsync(CommandLineOptions options)
    {
        var name = options.TemplateName;

        if (string.IsNullOrWhiteSpace(name))
        {
            name = AnsiConsole.Ask<string>("Enter [green]template name[/]:");
        }

        _logger?.LogInformation("Showing template: {Name}", name);

        var template = await _templateService.LoadTemplateAsync(name);

        // Display metadata
        var metadataTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .Title($"[bold blue]Template: {template.Metadata.Name}[/]");

        metadataTable.AddColumn("[bold]Property[/]");
        metadataTable.AddColumn("[bold]Value[/]");

        metadataTable.AddRow("Description", template.Metadata.Description);
        metadataTable.AddRow("Author", template.Metadata.Author);
        metadataTable.AddRow("Version", template.Metadata.Version);
        metadataTable.AddRow("Created", template.Metadata.Created.ToString("yyyy-MM-dd HH:mm"));
        metadataTable.AddRow("Modified", template.Metadata.Modified.ToString("yyyy-MM-dd HH:mm"));
        metadataTable.AddRow("Tags", string.Join(", ", template.Metadata.Tags));

        AnsiConsole.Write(metadataTable);
        AnsiConsole.WriteLine();

        // Display configuration
        var configTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .Title("[bold green]Configuration[/]");

        configTable.AddColumn("[bold]Setting[/]");
        configTable.AddColumn("[bold]Value[/]");

        configTable.AddRow("Template", $"{template.Configuration.Template.Name} @ {template.Configuration.Template.Version}");
        configTable.AddRow("Project", template.Configuration.Project.Name);
        configTable.AddRow("Solution", template.Configuration.Project.CreateSolution ? template.Configuration.Project.SolutionName ?? template.Configuration.Project.Name : "No");
        configTable.AddRow("Packages", template.Configuration.Packages.Count > 0 ? $"{template.Configuration.Packages.Count} package(s)" : "None");

        if (template.Configuration.Packages.Count > 0)
        {
            foreach (var pkg in template.Configuration.Packages)
            {
                configTable.AddRow($"  └─ {pkg.Name}", pkg.Version);
            }
        }

        configTable.AddRow("Starter Kit", template.Configuration.StarterKit.Enabled ? template.Configuration.StarterKit.Package ?? "Yes" : "No");
        configTable.AddRow("Docker", template.Configuration.Docker.Dockerfile || template.Configuration.Docker.DockerCompose ? "Yes" : "No");
        configTable.AddRow("Unattended", template.Configuration.Unattended.Enabled ? $"Yes ({template.Configuration.Unattended.Database.Type})" : "No");

        AnsiConsole.Write(configTable);
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
