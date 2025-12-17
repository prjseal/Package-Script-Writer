using Microsoft.Extensions.Logging;
using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Extensions;
using PSW.Shared.Services;
using Spectre.Console;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates history-related workflows
/// </summary>
public class HistoryWorkflow
{
    private readonly HistoryService _historyService;
    private readonly ScriptExecutor _scriptExecutor;
    private readonly IScriptGeneratorService _scriptGeneratorService;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryWorkflow"/> class
    /// </summary>
    /// <param name="historyService">The service for managing command history</param>
    /// <param name="scriptExecutor">The script executor for running generated scripts</param>
    /// <param name="scriptGeneratorService">The service for generating scripts</param>
    /// <param name="logger">Optional logger instance</param>
    public HistoryWorkflow(
        HistoryService historyService,
        ScriptExecutor scriptExecutor,
        IScriptGeneratorService scriptGeneratorService,
        ILogger? logger = null)
    {
        _historyService = historyService;
        _scriptExecutor = scriptExecutor;
        _scriptGeneratorService = scriptGeneratorService;
        _logger = logger;
    }

    /// <summary>
    /// Runs a history command based on options
    /// </summary>
    public async Task RunAsync(CommandLineOptions options)
    {
        var command = options.HistoryCommand?.ToLower();

        // If no command was provided (e.g., just "psw history"), show help
        if (string.IsNullOrEmpty(command))
        {
            AnsiConsole.MarkupLine("[red]Error: Missing history subcommand[/]\n");
            ConsoleDisplay.DisplayHistoryHelp();
            Environment.ExitCode = 1;
            return;
        }

        switch (command)
        {
            case "list":
                ListHistory(options);
                break;

            case "rerun":
            case "re-run":
                await RerunHistoryEntryAsync(options);
                break;

            case "delete":
                DeleteHistoryEntry(options);
                break;

            case "clear":
                ClearHistory();
                break;

            default:
                AnsiConsole.MarkupLine($"[red]Error: Unknown history command '{command}'[/]\n");
                ConsoleDisplay.DisplayHistoryHelp();
                Environment.ExitCode = 1;
                break;
        }
    }

    /// <summary>
    /// Lists recent history entries
    /// </summary>
    private void ListHistory(CommandLineOptions options)
    {
        _logger?.LogInformation("Listing history");

        var entries = _historyService.GetRecentEntries(options.HistoryLimit);

        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No history entries found.[/]");
            AnsiConsole.MarkupLine("History is automatically created when you generate scripts.");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn("[bold]#[/]");
        table.AddColumn("[bold]Date/Time[/]");
        table.AddColumn("[bold]Project[/]");
        table.AddColumn("[bold]Template[/]");
        table.AddColumn("[bold]Status[/]");
        table.AddColumn("[bold]Description[/]");

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var index = i + 1;

            var statusText = entry.WasExecuted
                ? (entry.ExitCode == 0 ? "[green]✓ Executed[/]" : $"[red]✗ Failed ({entry.ExitCode})[/]")
                : "[dim]Not executed[/]";

            table.AddRow(
                index.ToString(),
                entry.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                entry.ScriptModel.ProjectName ?? "[dim]N/A[/]",
                entry.TemplateName ?? "[dim]None[/]",
                statusText,
                entry.Description ?? "[dim]-[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]Showing {entries.Count} of {_historyService.GetCount()} total entries[/]");
        AnsiConsole.MarkupLine("[dim]Use 'psw history rerun <#>' to re-run a script[/]");
    }


    /// <summary>
    /// Re-runs a script from history
    /// </summary>
    private async Task RerunHistoryEntryAsync(CommandLineOptions options)
    {
        var id = options.HistoryId;

        if (string.IsNullOrWhiteSpace(id))
        {
            id = AnsiConsole.Ask<string>("Enter [green]history entry number[/] to re-run:");
        }

        _logger?.LogInformation("Re-running history entry: {Id}", id);

        var entry = _historyService.GetEntry(id);

        if (entry == null)
        {
            AnsiConsole.MarkupLine($"[red]History entry not found: {id}[/]");
            AnsiConsole.MarkupLine("Use [green]psw history list[/] to see available entries");
            return;
        }

        // Show what will be re-run
        AnsiConsole.MarkupLine($"[bold blue]Re-running:[/] {entry.GetDisplayName()}");
        AnsiConsole.MarkupLine($"[dim]Original timestamp: {entry.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm}[/]");
        AnsiConsole.WriteLine();

        // Display current configuration
        DisplayScriptConfiguration(entry.ScriptModel);
        AnsiConsole.WriteLine();

        // Ask if user wants to modify before re-running
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .AddChoices(new[] {
                    "Run with same configuration",
                    "Modify configuration first",
                    "Cancel"
                }));

        if (action == "Cancel")
        {
            AnsiConsole.MarkupLine("[yellow]Re-run cancelled.[/]");
            return;
        }

        var scriptModel = entry.ScriptModel;

        if (action == "Modify configuration first")
        {
            scriptModel = ModifyScriptModel(entry.ScriptModel);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]✓ Configuration updated[/]");
            AnsiConsole.WriteLine();
        }

        // Regenerate the script using the (possibly modified) model
        var script = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Regenerating installation script...", async ctx =>
            {
                return await Task.Run(() => _scriptGeneratorService.GenerateScript(scriptModel.ToViewModel()));
            });

        _logger?.LogInformation("Script regenerated successfully");

        AnsiConsole.MarkupLine("[green]✓ Script regenerated[/]");
        AnsiConsole.WriteLine();

        // Display script
        var panel = new Panel(script)
            .Header("[bold cyan]Regenerated Installation Script[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(4, 2);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Ask if they want to execute
        var shouldExecute = AnsiConsole.Confirm("Execute the script?", true);

        if (shouldExecute)
        {
            var runDir = AnsiConsole.Ask<string>(
                "Enter [green]directory path[/] to run the script (leave blank for current directory):",
                string.Empty);

            if (string.IsNullOrWhiteSpace(runDir))
            {
                runDir = Directory.GetCurrentDirectory();
            }
            else
            {
                runDir = Path.GetFullPath(runDir);

                if (!Directory.Exists(runDir))
                {
                    if (AnsiConsole.Confirm($"Directory [yellow]{runDir}[/] doesn't exist. Create it?"))
                    {
                        Directory.CreateDirectory(runDir);
                        AnsiConsole.MarkupLine($"[green]✓ Created directory {runDir}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Execution cancelled.[/]");
                        return;
                    }
                }
            }

            await _scriptExecutor.RunScriptAsync(script, runDir);

            // Update history with new execution info
            var newEntry = _historyService.AddEntry(
                entry.ScriptModel,
                entry.TemplateName,
                $"Re-run of: {entry.GetDisplayName()}",
                entry.Tags
            );

            _historyService.UpdateExecution(newEntry.Id, runDir, 0); // Assuming success
        }
    }

    /// <summary>
    /// Deletes a history entry
    /// </summary>
    private void DeleteHistoryEntry(CommandLineOptions options)
    {
        var id = options.HistoryId;

        if (string.IsNullOrWhiteSpace(id))
        {
            id = AnsiConsole.Ask<string>("Enter [green]history entry number[/] to delete:");
        }

        _logger?.LogInformation("Deleting history entry: {Id}", id);

        var entry = _historyService.GetEntry(id);

        if (entry == null)
        {
            AnsiConsole.MarkupLine($"[red]History entry not found: {id}[/]");
            return;
        }

        var confirm = AnsiConsole.Confirm($"Delete history entry: [yellow]{entry.GetDisplayName()}[/]?", false);

        if (!confirm)
        {
            AnsiConsole.MarkupLine("[yellow]Delete cancelled.[/]");
            return;
        }

        _historyService.DeleteEntry(id);
        AnsiConsole.MarkupLine($"[green]✓ History entry deleted[/]");
    }

    /// <summary>
    /// Clears all history
    /// </summary>
    private void ClearHistory()
    {
        _logger?.LogInformation("Clearing all history");

        var count = _historyService.GetCount();

        if (count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]History is already empty.[/]");
            return;
        }

        var confirm = AnsiConsole.Confirm($"Delete all [yellow]{count}[/] history entries? This cannot be undone.", false);

        if (!confirm)
        {
            AnsiConsole.MarkupLine("[yellow]Clear cancelled.[/]");
            return;
        }

        _historyService.ClearAll();
        AnsiConsole.MarkupLine($"[green]✓ Cleared {count} history entries[/]");
    }

    /// <summary>
    /// Displays the script configuration in a table
    /// </summary>
    private void DisplayScriptConfiguration(ScriptModel model)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .Title("[bold yellow]Current Configuration[/]");

        table.AddColumn("[bold]Setting[/]");
        table.AddColumn("[bold]Value[/]");

        table.AddRow("Project Name", model.ProjectName ?? "N/A");
        table.AddRow("Template", $"{model.TemplateName} @ {model.TemplateVersion ?? "Latest"}");

        if (!string.IsNullOrWhiteSpace(model.Packages))
        {
            table.AddRow("Packages", model.Packages);
        }

        if (model.CreateSolutionFile)
        {
            table.AddRow("Solution", model.SolutionName ?? model.ProjectName ?? "Yes");
        }

        if (model.IncludeStarterKit)
        {
            table.AddRow("Starter Kit", model.StarterKitPackage ?? "Yes");
        }

        if (model.IncludeDockerfile || model.IncludeDockerCompose)
        {
            var dockerOpts = new List<string>();
            if (model.IncludeDockerfile) dockerOpts.Add("Dockerfile");
            if (model.IncludeDockerCompose) dockerOpts.Add("Docker Compose");
            table.AddRow("Docker", string.Join(", ", dockerOpts));
        }

        if (model.UseUnattendedInstall)
        {
            table.AddRow("Unattended Install", "Yes");
            table.AddRow("Database", model.DatabaseType ?? "N/A");
            table.AddRow("Admin Email", model.UserEmail ?? "N/A");
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Allows user to modify script model values interactively
    /// </summary>
    private ScriptModel ModifyScriptModel(ScriptModel original)
    {
        // Create a copy to modify
        var model = new ScriptModel
        {
            TemplateName = original.TemplateName,
            TemplateVersion = original.TemplateVersion,
            ProjectName = original.ProjectName,
            CreateSolutionFile = original.CreateSolutionFile,
            SolutionName = original.SolutionName,
            Packages = original.Packages,
            IncludeStarterKit = original.IncludeStarterKit,
            StarterKitPackage = original.StarterKitPackage,
            IncludeDockerfile = original.IncludeDockerfile,
            IncludeDockerCompose = original.IncludeDockerCompose,
            CanIncludeDocker = original.CanIncludeDocker,
            UseUnattendedInstall = original.UseUnattendedInstall,
            DatabaseType = original.DatabaseType,
            ConnectionString = original.ConnectionString,
            UserFriendlyName = original.UserFriendlyName,
            UserEmail = original.UserEmail,
            UserPassword = original.UserPassword,
            OnelinerOutput = original.OnelinerOutput,
            RemoveComments = original.RemoveComments
        };

        AnsiConsole.MarkupLine("[bold yellow]Modify Configuration[/]");
        AnsiConsole.MarkupLine("[dim]Press Enter to keep current value, or type new value[/]\n");

        // Project Name
        var newProjectName = AnsiConsole.Ask<string>(
            $"Project Name [{model.ProjectName}]:",
            model.ProjectName ?? "");
        if (!string.IsNullOrWhiteSpace(newProjectName))
        {
            model.ProjectName = newProjectName;
        }

        // Template Version
        var changeTemplate = AnsiConsole.Confirm("Change template version?", false);
        if (changeTemplate)
        {
            var newVersion = AnsiConsole.Ask<string>(
                $"Template Version [{model.TemplateVersion ?? "Latest"}]:",
                model.TemplateVersion ?? "");
            model.TemplateVersion = newVersion;
        }

        // Packages
        var changePackages = AnsiConsole.Confirm("Change packages?", false);
        if (changePackages)
        {
            var newPackages = AnsiConsole.Ask<string>(
                $"Packages (comma-separated) [{model.Packages ?? "None"}]:",
                model.Packages ?? "");
            model.Packages = newPackages;
        }

        // Solution
        var changeSolution = AnsiConsole.Confirm("Change solution settings?", false);
        if (changeSolution)
        {
            model.CreateSolutionFile = AnsiConsole.Confirm("Create solution file?", model.CreateSolutionFile);
            if (model.CreateSolutionFile)
            {
                var newSolutionName = AnsiConsole.Ask<string>(
                    $"Solution Name [{model.SolutionName ?? model.ProjectName}]:",
                    model.SolutionName ?? model.ProjectName ?? "");
                model.SolutionName = newSolutionName;
            }
        }

        // Starter Kit
        var changeStarterKit = AnsiConsole.Confirm("Change starter kit?", false);
        if (changeStarterKit)
        {
            model.IncludeStarterKit = AnsiConsole.Confirm("Include starter kit?", model.IncludeStarterKit);
            if (model.IncludeStarterKit)
            {
                var newStarterKit = AnsiConsole.Ask<string>(
                    $"Starter Kit Package [{model.StarterKitPackage ?? "clean"}]:",
                    model.StarterKitPackage ?? "clean");
                model.StarterKitPackage = newStarterKit;
            }
        }

        // Database
        if (model.UseUnattendedInstall)
        {
            var changeDatabase = AnsiConsole.Confirm("Change database settings?", false);
            if (changeDatabase)
            {
                var newDbType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select database type:")
                        .AddChoices(new[] { "SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE" })
                        .HighlightStyle(new Style(Color.Green)));
                model.DatabaseType = newDbType;

                if (newDbType == "SQLServer" || newDbType == "SQLAzure")
                {
                    var newConnStr = AnsiConsole.Ask<string>("Connection string:");
                    model.ConnectionString = newConnStr;
                }
            }
        }

        return model;
    }
}
