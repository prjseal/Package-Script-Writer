using Spectre.Console;
using PackageCliTool.Models.Api;

namespace PackageCliTool.UI;

/// <summary>
/// Handles all interactive user prompts
/// </summary>
public static class InteractivePrompts
{
    /// <summary>
    /// Prompts user for all script configuration options
    /// </summary>
    public static ScriptModel PromptForScriptConfiguration(Dictionary<string, string> packageVersions)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 4:[/] Configure Project Options\n");

        // Build packages string with proper format handling
        var packageParts = new List<string>();
        foreach (var (package, version) in packageVersions)
        {
            if (string.IsNullOrEmpty(version))
            {
                // Latest Stable - just package name
                packageParts.Add(package);
            }
            else if (version == "--prerelease")
            {
                // Pre-release - package name with flag
                packageParts.Add($"{package} {version}");
            }
            else
            {
                // Specific version - package|version format
                packageParts.Add($"{package}|{version}");
            }
        }
        var packagesString = string.Join(",", packageParts);

        // Create a script model and populate it with user inputs
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            Packages = packagesString
        };

        // Template and Project Configuration
        AnsiConsole.MarkupLine("[bold yellow]Template & Project Settings[/]\n");

        model.TemplateVersion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select [green]Umbraco template version[/]:")
                .AddChoices(new[] { "Latest Stable", "Latest LTS", "14.3.0", "14.2.0", "14.1.0", "14.0.0", "13.5.2", "13.4.0", "13.3.0", "Custom..." }));

        if (model.TemplateVersion == "Custom...")
        {
            model.TemplateVersion = AnsiConsole.Ask<string>("Enter [green]custom version[/]:");
        }
        else if (model.TemplateVersion == "Latest Stable")
        {
            model.TemplateVersion = "";
        }
        else if (model.TemplateVersion == "Latest LTS")
        {
            model.TemplateVersion = "LTS";
        }

        model.ProjectName = AnsiConsole.Ask<string>("Enter [green]project name[/]:", "MyProject");

        model.CreateSolutionFile = AnsiConsole.Confirm("Create a [green]solution file[/]?", true);

        if (model.CreateSolutionFile)
        {
            model.SolutionName = AnsiConsole.Ask<string>("Enter [green]solution name[/]:", model.ProjectName);
        }

        // Starter Kit Configuration
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Starter Kit Options[/]\n");

        model.IncludeStarterKit = AnsiConsole.Confirm("Include a [green]starter kit[/]?", true);

        if (model.IncludeStarterKit)
        {
            model.StarterKitPackage = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select [green]starter kit[/]:")
                    .AddChoices(new[]
                    {
                        "clean",
                        "clean --version 4.1.0 (Umbraco 13)",
                        "clean --version 3.1.4 (Umbraco 9-12)",
                        "Articulate",
                        "Portfolio",
                        "LittleNorth.Igloo",
                        "Umbraco.BlockGrid.Example.Website",
                        "Umbraco.TheStarterKit",
                        "uSkinnedSiteBuilder"
                    }));
        }

        // Docker Configuration
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Docker Options[/]\n");

        model.IncludeDockerfile = AnsiConsole.Confirm("Include [green]Dockerfile[/]?", false);
        model.IncludeDockerCompose = AnsiConsole.Confirm("Include [green]Docker Compose[/]?", false);
        model.CanIncludeDocker = model.IncludeDockerfile || model.IncludeDockerCompose;

        // Unattended Install Configuration
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Unattended Install Options[/]\n");

        model.UseUnattendedInstall = AnsiConsole.Confirm("Use [green]unattended install[/]?", true);

        if (model.UseUnattendedInstall)
        {
            model.DatabaseType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select [green]database type[/]:")
                    .AddChoices(new[] { "SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE" }));

            if (model.DatabaseType == "SQLServer" || model.DatabaseType == "SQLAzure")
            {
                model.ConnectionString = AnsiConsole.Ask<string>("Enter [green]connection string[/]:");
            }

            model.UserFriendlyName = AnsiConsole.Ask<string>("Enter [green]admin user friendly name[/]:", "Administrator");
            model.UserEmail = AnsiConsole.Ask<string>("Enter [green]admin email[/]:", "admin@example.com");
            model.UserPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter [green]admin password[/] (min 10 characters):")
                    .PromptStyle("red")
                    .Secret()
                    .DefaultValue("1234567890"));
        }

        // Output Format Options
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Output Format Options[/]\n");

        model.OnelinerOutput = AnsiConsole.Confirm("Output as [green]one-liner[/]?", false);
        model.RemoveComments = AnsiConsole.Confirm("Remove [green]comments[/] from script?", false);

        return model;
    }

    /// <summary>
    /// Prompts user for what to do with the generated script
    /// </summary>
    public static string PromptForScriptAction()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nWhat would you like to do with this script?")
                .AddChoices(new[] { "Edit", "Run" }));
    }

    /// <summary>
    /// Prompts user for directory to run script in
    /// </summary>
    public static string PromptForRunDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var targetDir = AnsiConsole.Ask<string>(
            $"Enter [green]directory path[/] to run the script (leave blank for current directory: {currentDir}):",
            string.Empty);

        if (string.IsNullOrWhiteSpace(targetDir))
        {
            return currentDir;
        }

        return targetDir;
    }

    /// <summary>
    /// Prompts user to confirm directory creation
    /// </summary>
    public static bool ConfirmDirectoryCreation(string directory)
    {
        return AnsiConsole.Confirm($"Directory [yellow]{directory}[/] doesn't exist. Create it?");
    }

    /// <summary>
    /// Prompts user to confirm script generation
    /// </summary>
    public static bool ConfirmScriptGeneration()
    {
        return AnsiConsole.Confirm("\nGenerate script with these settings?", true);
    }
}
