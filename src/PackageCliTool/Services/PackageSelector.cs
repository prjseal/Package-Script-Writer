using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Configuration;
using PackageCliTool.Logging;

namespace PackageCliTool.Services;

/// <summary>
/// Service for handling package selection and version selection logic
/// </summary>
public class PackageSelector
{
    private readonly ApiClient _apiClient;
    private readonly ILogger? _logger;
    private List<PagedPackagesPackage> _allPackages = new();

    public PackageSelector(ApiClient apiClient, ILogger? logger = null)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Populates the allPackages list from the API
    /// </summary>
    public async Task PopulateAllPackagesAsync()
    {
        try
        {
            _logger?.LogInformation("Fetching available packages from API");

            _allPackages = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Loading available packages...", async ctx =>
                {
                    return await _apiClient.GetAllPackagesAsync();
                });

            AnsiConsole.MarkupLine($"[green]✓[/] Loaded {_allPackages.Count} packages from marketplace");
            _logger?.LogInformation("Loaded {Count} packages from marketplace", _allPackages.Count);
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load packages from API");
            ErrorHandler.Warning($"Unable to load packages from API: {ex.Message}", _logger);
            AnsiConsole.MarkupLine("[dim]Continuing with limited package selection...[/]");
            AnsiConsole.WriteLine();
            _allPackages = new List<PagedPackagesPackage>();
        }
    }

    /// <summary>
    /// Allows user to select multiple packages using MultiSelectionPrompt
    /// </summary>
    public async Task<List<string>> SelectPackagesAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Step 1:[/] Select Packages\n");

        // Ask user how they want to select packages
        var selectionMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("How would you like to add packages?")
                .AddChoices(new[] { "Select from popular packages", "Search for package", "None - skip packages" }));

        var selectedPackages = new List<string>();

        if (selectionMode == "None - skip packages")
        {
            AnsiConsole.MarkupLine("[dim]Skipping package selection...[/]");
            return selectedPackages;
        }

        if (selectionMode == "Select from popular packages")
        {
            selectedPackages = await SelectPackagesFromListAsync();
        }
        else if (selectionMode == "Search for package")
        {
            selectedPackages = await SearchForPackagesAsync();
        }

        return selectedPackages;
    }

    /// <summary>
    /// Select packages from the full list with pagination
    /// </summary>
    private async Task<List<string>> SelectPackagesFromListAsync()
    {
        var packageChoices = new List<string>();

        if (_allPackages.Count > 0)
        {
            // Use all packages from the API
            packageChoices = _allPackages
                .Where(p => !string.IsNullOrWhiteSpace(p.PackageId))
                .Select(p => p.PackageId)
                .ToList();
        }

        // Fallback to popular packages if no valid packages found
        if (packageChoices.Count == 0)
        {
            _logger?.LogWarning("No packages with valid PackageId found, falling back to popular packages");
            packageChoices = ApiConfiguration.PopularPackages.ToList();
        }

        var selections = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select [green]one or more packages[/] (use Space to select, Enter to confirm):")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to see more packages)[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a package, [green]<enter>[/] to accept)[/]")
                .AddChoices(packageChoices));

        return selections.ToList();
    }

    /// <summary>
    /// Search for packages with autocomplete
    /// </summary>
    private async Task<List<string>> SearchForPackagesAsync()
    {
        var selectedPackages = new List<string>();
        var continueAdding = true;

        while (continueAdding)
        {
            // Build autocomplete choices from allPackages
            var packageChoices = new List<string>();

            if (_allPackages.Count > 0)
            {
                packageChoices = _allPackages
                    .Where(p => !string.IsNullOrWhiteSpace(p.PackageId))
                    .OrderBy(p => p.PackageId)
                    .Select(p => p.PackageId)
                    .ToList();
            }

            string packageName;

            if (packageChoices.Count > 0)
            {
                // Use TextPrompt with autocomplete
                packageName = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter [green]package name[/] (or type to search):")
                        .AllowEmpty()
                        .ShowDefaultValue(false)
                        .DefaultValue("")
                        .AddChoices(packageChoices));
            }
            else
            {
                // Fallback to simple input if no packages loaded
                _logger?.LogWarning("No packages with valid NuGetPackageId found for autocomplete, using simple input");
                packageName = AnsiConsole.Ask<string>("Enter [green]package name[/]:", string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(packageName))
            {
                selectedPackages.Add(packageName.Trim());
                AnsiConsole.MarkupLine($"[green]✓[/] Added package: {packageName.Trim()}");
            }

            // Ask if they want to add another package
            if (selectedPackages.Count > 0)
            {
                continueAdding = AnsiConsole.Confirm("Add another package?", false);
            }
            else
            {
                continueAdding = AnsiConsole.Confirm("No packages added yet. Add a package?", true);
            }
        }

        return selectedPackages;
    }

    /// <summary>
    /// For each selected package, fetch versions and let user select one
    /// </summary>
    public async Task<Dictionary<string, string>> SelectVersionsForPackagesAsync(List<string> packages)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 2:[/] Select Versions\n");

        var packageVersions = new Dictionary<string, string>();

        foreach (var package in packages)
        {
            try
            {
                _logger?.LogDebug("Fetching versions for package: {Package}", package);

                // Fetch versions with spinner
                var versions = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("green"))
                    .StartAsync($"Fetching versions for [yellow]{package}[/]...", async ctx =>
                    {
                        return await _apiClient.GetPackageVersionsAsync(package, includePrerelease: true);
                    });

                _logger?.LogDebug("Found {Count} versions for package {Package}", versions.Count, package);

                // Build version choices with special options first
                var versionChoices = new List<string>
                {
                    "Latest Stable",
                    "Pre-release"
                };

                // Add actual versions if available
                if (versions.Count > 0)
                {
                    versionChoices.AddRange(versions);
                }
                else
                {
                    ErrorHandler.Warning($"No specific versions found for {package}. Showing default options only.", _logger);
                }

                // Let user select a version
                var selectedVersion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Select version for [green]{package}[/]:")
                        .PageSize(12)
                        .MoreChoicesText("[grey](Move up and down to see more versions)[/]")
                        .AddChoices(versionChoices));

                // Map the selection to the appropriate value
                if (selectedVersion == "Latest Stable")
                {
                    // No version specified means latest stable
                    packageVersions[package] = "";
                    AnsiConsole.MarkupLine($"[green]✓[/] Selected {package} - Latest Stable");
                    _logger?.LogInformation("Selected {Package} with latest stable version", package);
                }
                else if (selectedVersion == "Pre-release")
                {
                    // Pre-release flag
                    packageVersions[package] = "--prerelease";
                    AnsiConsole.MarkupLine($"[green]✓[/] Selected {package} - Pre-release");
                    _logger?.LogInformation("Selected {Package} with pre-release version", package);
                }
                else
                {
                    // Specific version selected
                    packageVersions[package] = selectedVersion;
                    AnsiConsole.MarkupLine($"[green]✓[/] Selected {package} version {selectedVersion}");
                    _logger?.LogInformation("Selected {Package} version {Version}", package, selectedVersion);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error fetching versions for package {Package}", package);
                ErrorHandler.Warning($"Error fetching versions for {package}: {ex.Message}. Using latest stable.", _logger);
                packageVersions[package] = "";
            }
        }

        return packageVersions;
    }
}
