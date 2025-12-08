using Spectre.Console;
using PackageCliTool.Models.Api;
using PackageCliTool.Services;
using PackageCliTool.Logging;
using Microsoft.Extensions.Logging;

namespace PackageCliTool.UI;

/// <summary>
/// Handles all interactive user prompts
/// </summary>
public static class InteractivePrompts
{
    /// <summary>
    /// Prompts user for all script configuration options
    /// </summary>
    public static async Task<ScriptModel> PromptForScriptConfigurationAsync(Dictionary<string, string> packageVersions, ApiClient apiClient, ILogger? logger = null, string? templateName = null, string? templateVersion = null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 7:[/] Configure Project Options\n");

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
            TemplateName = templateName ?? "Umbraco.Templates",
            TemplateVersion = templateVersion ?? "",
            Packages = packagesString
        };

        // Template and Project Configuration
        AnsiConsole.MarkupLine("[bold yellow]Template & Project Settings[/]\n");

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
            // Select the starter kit package
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
                logger?.LogDebug("Fetching versions for starter kit: {StarterKit}", starterKitName);

                var versions = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("green"))
                    .StartAsync($"Fetching versions for [yellow]{starterKitName}[/]...", async ctx =>
                    {
                        return await apiClient.GetPackageVersionsAsync(starterKitName, includePrerelease: true);
                    });

                logger?.LogDebug("Found {Count} versions for starter kit {StarterKit}", versions.Count, starterKitName);

                // Build version choices with special options first
                var versionChoices = new List<string>
                {
                    "Latest Stable"
                };

                // Add actual versions if available
                if (versions.Count > 0)
                {
                    versionChoices.AddRange(versions);
                }
                else
                {
                    ErrorHandler.Warning($"No specific versions found for {starterKitName}. Using latest stable.", logger);
                }

                // Let user select a version
                var selectedVersion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Select version for [green]{starterKitName}[/]:")
                        .PageSize(12)
                        .MoreChoicesText("[grey](Move up and down to see more versions)[/]")
                        .AddChoices(versionChoices));

                // Build the starter kit package string
                if (selectedVersion == "Latest Stable")
                {
                    model.StarterKitPackage = starterKitName;
                    AnsiConsole.MarkupLine($"[green]✓[/] Selected {starterKitName} - Latest Stable");
                    logger?.LogInformation("Selected {StarterKit} with latest stable version", starterKitName);
                }
                else
                {
                    model.StarterKitPackage = $"{starterKitName} --version {selectedVersion}";
                    AnsiConsole.MarkupLine($"[green]✓[/] Selected {starterKitName} version {selectedVersion}");
                    logger?.LogInformation("Selected {StarterKit} version {Version}", starterKitName, selectedVersion);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Error fetching versions for starter kit {StarterKit}", starterKitName);
                ErrorHandler.Warning($"Error fetching versions for {starterKitName}: {ex.Message}. Using latest stable.", logger);
                model.StarterKitPackage = starterKitName;
            }
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
                .Title("\nWhat would you like to do with this script?\n[dim](Press Ctrl+C at any time to start over)[/]")
                .AddChoices(new[] { "Run", "Edit", "Copy to clipboard", "Save as template", "Start over" }));
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
