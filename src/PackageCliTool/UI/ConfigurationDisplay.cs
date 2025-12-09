using Spectre.Console;
using PackageCliTool.Models.Api;

namespace PackageCliTool.UI;

/// <summary>
/// Handles display of configuration summaries and tables
/// </summary>
public static class ConfigurationDisplay
{
    /// <summary>
    /// Displays the final package selection in a nicely formatted table
    /// </summary>
    public static void DisplayFinalSelection(Dictionary<string, string> packageVersions)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 5:[/] Final Selection\n");

        if (packageVersions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No packages with versions selected.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[bold]Package Name[/]").Centered())
            .AddColumn(new TableColumn("[bold]Selected Version[/]").Centered());

        foreach (var (package, version) in packageVersions)
        {
            string versionDisplay = version switch
            {
                "" => "Latest Stable",
                "--prerelease" => "Pre-release",
                _ => version
            };
            table.AddRow(package, $"[green]{versionDisplay}[/]");
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Displays a summary of the configuration before generating the script
    /// </summary>
    public static void DisplayConfigurationSummary(ScriptModel model, Dictionary<string, string> packageVersions)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Configuration Summary[/]\n");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Aqua)
            .AddColumn(new TableColumn("[bold]Setting[/]"))
            .AddColumn(new TableColumn("[bold]Value[/]"));

        table.AddRow("Template", $"{model.TemplateName} @ {(string.IsNullOrEmpty(model.TemplateVersion) ? "Latest Stable" : model.TemplateVersion)}");
        table.AddRow("Project Name", model.ProjectName);

        if (model.CreateSolutionFile)
        {
            table.AddRow("Solution Name", model.SolutionName ?? "N/A");
        }

        if (packageVersions.Count > 0)
        {
            table.AddRow("Packages", $"{packageVersions.Count} package(s) selected");
        }

        if (model.IncludeStarterKit)
        {
            table.AddRow("Starter Kit", model.StarterKitPackage ?? "N/A");
        }

        if (model.IncludeDockerfile)
        {
            table.AddRow("Docker", "Dockerfile included");
        }

        if (model.IncludeDockerCompose)
        {
            table.AddRow("Docker Compose", "Included");
        }

        if (model.UseUnattendedInstall)
        {
            table.AddRow("Unattended Install", "Enabled");
            table.AddRow("Database Type", model.DatabaseType ?? "N/A");
            table.AddRow("Admin User", model.UserFriendlyName ?? "N/A");
            table.AddRow("Admin Email", model.UserEmail ?? "N/A");
        }

        if (model.OnelinerOutput)
        {
            table.AddRow("Output Format", "One-liner");
        }

        if (model.RemoveComments)
        {
            table.AddRow("Comments", "Removed");
        }

        AnsiConsole.Write(table);
    }
}
