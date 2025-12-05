using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace PackageCliTool;

/// <summary>
/// Main program class for the Package Script Writer CLI tool
/// </summary>
class Program
{
    // API configuration
    private const string ApiBaseUrl = "https://psw.codeshare.co.uk";
    private const string GetVersionsEndpoint = "/api/scriptgeneratorapi/getpackageversions";
    private const string GenerateScriptEndpoint = "/api/scriptgeneratorapi/generatescript";

    // Popular Umbraco packages for quick selection
    private static readonly List<string> PopularPackages = new()
    {
        "Umbraco.Community.BlockPreview",
        "Diplo.GodMode",
        "uSync",
        "Umbraco.Community.Contentment",
        "Our.Umbraco.GMaps",
        "Umbraco.Forms",
        "Umbraco.Deploy",
        "Umbraco.TheStarterKit",
        "SEOChecker",
        "Umbraco.Community.SimpleTinyMceConfiguration"
    };

    static async Task Main(string[] args)
    {
        try
        {
            // Display welcome banner
            DisplayWelcomeBanner();

            // Step 1: Select packages
            var selectedPackages = await SelectPackagesAsync();

            if (selectedPackages.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No packages selected. Exiting...[/]");
                return;
            }

            // Step 2: For each package, select version
            var packageVersions = await SelectVersionsForPackagesAsync(selectedPackages);

            // Step 3: Display final selection
            DisplayFinalSelection(packageVersions);

            // Step 4: Optional - Generate script (if we want to call the generate endpoint)
            var shouldGenerate = AnsiConsole.Confirm("Would you like to generate a complete installation script?");

            if (shouldGenerate)
            {
                await GenerateAndDisplayScriptAsync(packageVersions);
            }

            // Display completion message
            AnsiConsole.MarkupLine("\n[green]✓ Process completed successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            AnsiConsole.WriteException(ex);
        }
    }

    /// <summary>
    /// Displays the welcome banner using Spectre.Console
    /// </summary>
    private static void DisplayWelcomeBanner()
    {
        AnsiConsole.Write(
            new FigletText("Package CLI Tool")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[dim]Package Script Writer - Interactive CLI[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Allows user to select multiple packages using MultiSelectionPrompt
    /// </summary>
    private static async Task<List<string>> SelectPackagesAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Step 1:[/] Select Packages\n");

        var selections = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select [green]one or more packages[/] (use Space to select, Enter to confirm):")
                .PageSize(15)
                .MoreChoicesText("[grey](Move up and down to see more packages)[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a package, [green]<enter>[/] to accept)[/]")
                .AddChoices(PopularPackages)
                .AddChoices(new[] { "[italic]+ Add custom package...[/italic]" }));

        var selectedPackages = new List<string>();

        // Process selections
        foreach (var selection in selections)
        {
            if (selection == "[italic]+ Add custom package...[/italic]")
            {
                // Allow user to add custom package name
                var customPackage = AnsiConsole.Ask<string>("Enter [green]custom package name[/]:");
                if (!string.IsNullOrWhiteSpace(customPackage))
                {
                    selectedPackages.Add(customPackage.Trim());
                }
            }
            else
            {
                selectedPackages.Add(selection);
            }
        }

        return selectedPackages;
    }

    /// <summary>
    /// For each selected package, fetch versions and let user select one
    /// </summary>
    private static async Task<Dictionary<string, string>> SelectVersionsForPackagesAsync(List<string> packages)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 2:[/] Select Versions\n");

        var packageVersions = new Dictionary<string, string>();
        var apiClient = new ApiClient(ApiBaseUrl);

        foreach (var package in packages)
        {
            try
            {
                // Fetch versions with spinner
                var versions = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("green"))
                    .StartAsync($"Fetching versions for [yellow]{package}[/]...", async ctx =>
                    {
                        return await apiClient.GetPackageVersionsAsync(package, includePrerelease: false);
                    });

                if (versions.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]⚠ No versions found for {package}. Skipping...[/]");
                    continue;
                }

                // Let user select a version
                var selectedVersion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Select version for [green]{package}[/]:")
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to see more versions)[/]")
                        .AddChoices(versions));

                packageVersions[package] = selectedVersion;
                AnsiConsole.MarkupLine($"[green]✓[/] Selected {package} version {selectedVersion}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error fetching versions for {package}: {ex.Message}[/]");
            }
        }

        return packageVersions;
    }

    /// <summary>
    /// Displays the final package selection in a nicely formatted table
    /// </summary>
    private static void DisplayFinalSelection(Dictionary<string, string> packageVersions)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 3:[/] Final Selection\n");

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
            table.AddRow(package, $"[green]{version}[/]");
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Generates a complete installation script using the API
    /// </summary>
    private static async Task GenerateAndDisplayScriptAsync(Dictionary<string, string> packageVersions)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Step 4:[/] Generate Script\n");

        // Build packages string in format: "Package1|Version1,Package2|Version2"
        var packagesString = string.Join(",", packageVersions.Select(kvp => $"{kvp.Key}|{kvp.Value}"));

        // Prompt for basic project details
        var projectName = AnsiConsole.Ask<string>("Enter [green]project name[/]:", "MyUmbracoProject");
        var templateVersion = AnsiConsole.Ask<string>("Enter [green]Umbraco template version[/] (or 'LTS'):", "LTS");

        var apiClient = new ApiClient(ApiBaseUrl);

        try
        {
            var script = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Generating installation script...", async ctx =>
                {
                    return await apiClient.GenerateScriptAsync(new ScriptRequest
                    {
                        Model = new ScriptModel
                        {
                            TemplateName = "Umbraco.Templates",
                            TemplateVersion = templateVersion,
                            ProjectName = projectName,
                            Packages = packagesString,
                            UseUnattendedInstall = false,
                            CreateSolutionFile = false
                        }
                    });
                });

            // Display the generated script in a panel
            var panel = new Panel(script)
                .Header("[bold green]Generated Installation Script[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Padding(1, 1);

            AnsiConsole.Write(panel);

            // Option to save to file
            if (AnsiConsole.Confirm("Would you like to save this script to a file?"))
            {
                var fileName = AnsiConsole.Ask<string>("Enter [green]file name[/]:", "install-script.sh");
                await File.WriteAllTextAsync(fileName, script);
                AnsiConsole.MarkupLine($"[green]✓ Script saved to {fileName}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error generating script: {ex.Message}[/]");
        }
    }
}

/// <summary>
/// API client for communicating with the Package Script Writer API
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Fetches available versions for a specific package from the API
    /// </summary>
    public async Task<List<string>> GetPackageVersionsAsync(string packageId, bool includePrerelease = false)
    {
        try
        {
            var request = new PackageVersionRequest
            {
                PackageId = packageId,
                IncludePrerelease = includePrerelease
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/scriptgeneratorapi/getpackageversions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PackageVersionResponse>(responseContent);

            return result?.Versions ?? new List<string>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to fetch versions for package '{packageId}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error fetching versions: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates an installation script using the API
    /// </summary>
    public async Task<string> GenerateScriptAsync(ScriptRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/scriptgeneratorapi/generatescript", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ScriptResponse>(responseContent);

            return result?.Script ?? "No script generated.";
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to generate script: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error generating script: {ex.Message}", ex);
        }
    }
}

// API Request/Response Models

public class PackageVersionRequest
{
    [JsonPropertyName("packageId")]
    public string PackageId { get; set; } = string.Empty;

    [JsonPropertyName("includePrerelease")]
    public bool IncludePrerelease { get; set; }
}

public class PackageVersionResponse
{
    [JsonPropertyName("versions")]
    public List<string> Versions { get; set; } = new();
}

public class ScriptRequest
{
    [JsonPropertyName("model")]
    public ScriptModel Model { get; set; } = new();
}

public class ScriptModel
{
    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; } = string.Empty;

    [JsonPropertyName("templateVersion")]
    public string TemplateVersion { get; set; } = string.Empty;

    [JsonPropertyName("createSolutionFile")]
    public bool CreateSolutionFile { get; set; }

    [JsonPropertyName("solutionName")]
    public string? SolutionName { get; set; }

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("useUnattendedInstall")]
    public bool UseUnattendedInstall { get; set; }

    [JsonPropertyName("databaseType")]
    public string? DatabaseType { get; set; }

    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    [JsonPropertyName("userFriendlyName")]
    public string? UserFriendlyName { get; set; }

    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    [JsonPropertyName("userPassword")]
    public string? UserPassword { get; set; }

    [JsonPropertyName("packages")]
    public string? Packages { get; set; }

    [JsonPropertyName("includeStarterKit")]
    public bool IncludeStarterKit { get; set; }

    [JsonPropertyName("starterKitPackage")]
    public string? StarterKitPackage { get; set; }

    [JsonPropertyName("canIncludeDocker")]
    public bool CanIncludeDocker { get; set; }

    [JsonPropertyName("includeDockerfile")]
    public bool IncludeDockerfile { get; set; }

    [JsonPropertyName("includeDockerCompose")]
    public bool IncludeDockerCompose { get; set; }

    [JsonPropertyName("onelinerOutput")]
    public bool OnelinerOutput { get; set; }

    [JsonPropertyName("removeComments")]
    public bool RemoveComments { get; set; }
}

public class ScriptResponse
{
    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;
}
