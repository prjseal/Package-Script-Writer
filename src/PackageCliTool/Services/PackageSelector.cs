using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using PackageCliTool.Models;
using PackageCliTool.Configuration;
using PackageCliTool.Logging;
using PackageCliTool.Validation;
using PSW.Shared.Services;

namespace PackageCliTool.Services;

/// <summary>
/// Service for handling package selection and version selection logic
/// </summary>
public class PackageSelector
{
    private readonly ApiClient _apiClient;
    private readonly IPackageService _packageService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger? _logger;
    private List<PagedPackagesPackage> _allPackages = new();

    public PackageSelector(ApiClient apiClient, IPackageService packageService, IMemoryCache memoryCache, ILogger? logger = null)
    {
        _apiClient = apiClient;
        _packageService = packageService;
        _memoryCache = memoryCache;
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

            AnsiConsole.MarkupLine($"Loaded [cyan]{_allPackages.Count}[/] packages from marketplace");
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
    /// Gets package versions directly from NuGet API (synchronous with caching)
    /// </summary>
    private List<string> GetPackageVersions(string packageId)
    {
        int cacheTime = 60;
        var packageUniqueId = packageId.ToLower();

        var packageVersions = _memoryCache.GetOrCreate(
            packageId + "_Versions",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                return _packageService.GetNugetPackageVersions($"https://api.nuget.org/v3-flatcontainer/{packageUniqueId}/index.json");
            });

        return packageVersions ?? new List<string>();
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
    /// Search for packages using a search term
    /// </summary>
    private async Task<List<string>> SearchForPackagesAsync()
    {
        var selectedPackages = new List<string>();
        var continueAdding = true;

        while (continueAdding)
        {
            // Ask user to enter a search term
            var searchTerm = AnsiConsole.Ask<string>("Enter [green]search term[/] to find packages:");

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                AnsiConsole.MarkupLine("[yellow]Search term cannot be empty.[/]");
                continue;
            }

            searchTerm = searchTerm.Trim();
            _logger?.LogInformation("Searching for packages with term: {SearchTerm}", searchTerm);

            // Search packages using LINQ - check Title, PackageId, and authors (case-insensitive)
            var matchingPackages = _allPackages
                .Where(p =>
                    (!string.IsNullOrWhiteSpace(p.Title) && p.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(p.PackageId) && p.PackageId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(p.authors) && p.authors.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                )
                .Select(p => new
                {
                    p.PackageId,
                    p.Title,
                    p.authors,
                    DisplayText = $"{p.PackageId} - {p.Title ?? "No title"} (by {p.authors ?? "Unknown"})"
                })
                .OrderBy(p => p.PackageId)
                .ToList();

            _logger?.LogInformation("Found {Count} matching packages", matchingPackages.Count);

            if (matchingPackages.Count > 0)
            {
                // Show matching packages in a select prompt (paged to 10)
                var displayChoices = matchingPackages.Select(p => p.DisplayText).ToList();

                var selectedDisplay = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Found [green]{matchingPackages.Count}[/] matching package(s). Select one:")
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to see more packages)[/]")
                        .AddChoices(displayChoices));

                // Extract the PackageId from the selected display text
                var selectedPackage = matchingPackages.First(p => p.DisplayText == selectedDisplay);
                selectedPackages.Add(selectedPackage.PackageId);
                AnsiConsole.MarkupLine($"[green]✓[/] Added package: {selectedPackage.PackageId}");
                _logger?.LogInformation("User selected package: {PackageId}", selectedPackage.PackageId);
            }
            else
            {
                // No matches found
                AnsiConsole.MarkupLine($"[yellow]No packages found matching '{searchTerm}'.[/]");
                _logger?.LogInformation("No packages found matching search term: {SearchTerm}", searchTerm);

                // Check if the search term is a valid NuGet package ID
                if (InputValidator.IsValidNuGetPackageId(searchTerm))
                {
                    AnsiConsole.MarkupLine("[dim]The search term appears to be a valid NuGet package ID.[/]");
                    var addAnyway = AnsiConsole.Confirm($"Would you like to add [green]{searchTerm}[/] as a package anyway?", true);

                    if (addAnyway)
                    {
                        selectedPackages.Add(searchTerm);
                        AnsiConsole.MarkupLine($"[green]✓[/] Added package: {searchTerm}");
                        _logger?.LogInformation("User added package not in marketplace: {PackageId}", searchTerm);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[dim]The search term is not a valid NuGet package ID format.[/]");
                }
            }

            // Ask if they want to add another package
            if (selectedPackages.Count > 0)
            {
                continueAdding = AnsiConsole.Confirm("Search for another package?", false);
            }
            else
            {
                continueAdding = AnsiConsole.Confirm("No packages added yet. Search for a package?", true);
            }
        }

        return selectedPackages;
    }

    /// <summary>
    /// For each selected package, fetch versions and let user select one
    /// </summary>
    public Dictionary<string, string> SelectVersionsForPackages(List<string> packages)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 2:[/] Select Versions\n");

        var packageVersions = new Dictionary<string, string>();

        foreach (var package in packages)
        {
            try
            {
                _logger?.LogDebug("Fetching versions for package: {Package}", package);

                // Fetch versions with spinner (synchronous)
                var versions = AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("green"))
                    .Start($"Fetching versions for [yellow]{package}[/]...", ctx =>
                    {
                        return GetPackageVersions(package);
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

    /// <summary>
    /// Gets the list of available templates (hard coded for now)
    /// </summary>
    public Task<List<string>> GetTemplatesAsync()
    {
        _logger?.LogInformation("Fetching available templates");

        var templates = new List<string>
        {
            "Umbraco.Templates",
            "Umbraco.Community.Templates.Clean",
            "Umbraco.Community.Templates.UmBootstrap"
        };

        _logger?.LogInformation("Loaded {Count} templates", templates.Count);
        return Task.FromResult(templates);
    }

    /// <summary>
    /// Allows user to select a template from available options
    /// </summary>
    public async Task<string> SelectTemplateAsync()
    {
        _logger?.LogInformation("User selecting template");

        var templates = await GetTemplatesAsync();

        var selectedTemplate = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select [green]template[/]:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to see more templates)[/]")
                .AddChoices(templates));

        AnsiConsole.MarkupLine($"[green]✓[/] Selected template: {selectedTemplate}");
        _logger?.LogInformation("User selected template: {Template}", selectedTemplate);

        return selectedTemplate;
    }

    /// <summary>
    /// Allows user to select a version for the selected template
    /// </summary>
    public string SelectTemplateVersion(string templateName)
    {
        AnsiConsole.WriteLine();
        _logger?.LogInformation("User selecting version for template: {Template}", templateName);

        try
        {
            // Fetch versions with spinner (synchronous)
            var versions = AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .Start($"Fetching versions for [yellow]{templateName}[/]...", ctx =>
                {
                    return GetPackageVersions(templateName);
                });

            _logger?.LogDebug("Found {Count} versions for template {Template}", versions.Count, templateName);

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
                ErrorHandler.Warning($"No specific versions found for {templateName}. Showing default options only.", _logger);
            }

            // Let user select a version
            var selectedVersion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Select version for [green]{templateName}[/]:")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to see more versions)[/]")
                    .AddChoices(versionChoices));

            // Map the selection to the appropriate value
            string versionValue;
            if (selectedVersion == "Latest Stable")
            {
                versionValue = "";
                AnsiConsole.MarkupLine($"[green]✓[/] Selected {templateName} - Latest Stable");
                _logger?.LogInformation("Selected {Template} with latest stable version", templateName);
            }
            else if (selectedVersion == "Pre-release")
            {
                versionValue = "--prerelease";
                AnsiConsole.MarkupLine($"[green]✓[/] Selected {templateName} - Pre-release");
                _logger?.LogInformation("Selected {Template} with pre-release version", templateName);
            }
            else
            {
                versionValue = selectedVersion;
                AnsiConsole.MarkupLine($"[green]✓[/] Selected {templateName} version {selectedVersion}");
                _logger?.LogInformation("Selected {Template} version {Version}", templateName, selectedVersion);
            }

            return versionValue;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error fetching versions for template {Template}", templateName);
            ErrorHandler.Warning($"Error fetching versions for {templateName}: {ex.Message}. Using latest stable.", _logger);
            return "";
        }
    }
}
