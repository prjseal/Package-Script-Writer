using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
using PackageCliTool.Models;

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
    private const string GetAllPackagesEndpoint = "/api/scriptgeneratorapi/getallpackages";
    private const string Version = "1.0.0-beta";

    // All available packages from the Umbraco marketplace
    private static List<PagedPackagesPackage> allPackages = new();

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
            // Parse command-line arguments
            var options = CommandLineOptions.Parse(args);

            // Handle help flag
            if (options.ShowHelp)
            {
                DisplayHelp();
                return;
            }

            // Handle version flag
            if (options.ShowVersion)
            {
                DisplayVersion();
                return;
            }

            // Determine if we should use CLI mode or interactive mode
            bool useCLIMode = options.HasAnyOptions();

            if (useCLIMode)
            {
                await RunCLIModeAsync(options);
            }
            else
            {
                await RunInteractiveModeAsync();
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
    /// Runs the tool in CLI mode using command-line flags
    /// </summary>
    private static async Task RunCLIModeAsync(CommandLineOptions options)
    {
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
    /// Runs the tool in interactive mode
    /// </summary>
    private static async Task RunInteractiveModeAsync()
    {
        // Display welcome banner
        DisplayWelcomeBanner();

        // Populate all packages from API
        await PopulateAllPackagesAsync();

        // Ask if user wants a default script (fast route)
        var useDefaultScript = AnsiConsole.Confirm("Do you want to generate a default script?", true);

        if (useDefaultScript)
        {
            await GenerateDefaultScriptAsync();
        }
        else
        {
            await RunCustomFlowAsync();
        }
    }

    /// <summary>
    /// Displays help information with all available flags
    /// </summary>
    private static void DisplayHelp()
    {
        AnsiConsole.Write(
            new FigletText("PSW CLI")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[dim]Package Script Writer - Interactive CLI[/]");
        AnsiConsole.MarkupLine("[dim]By Paul Seal[/]\n");

        var helpPanel = new Panel(
            @"[bold yellow]USAGE:[/]
  psw [options]

[bold yellow]OPTIONS:[/]
  [green]-h, --help[/]                    Show help information
  [green]-v, --version[/]                 Show version information
  [green]-d, --default[/]                 Generate a default script with minimal configuration

[bold yellow]SCRIPT CONFIGURATION:[/]
  [green]-p, --packages[/] <packages>     Comma-separated list of packages with optional versions
                                   Format: ""Package1|Version1,Package2|Version2""
                                   Or just package names: ""uSync,Umbraco.Forms"" (uses latest)
                                   Example: ""uSync|17.0.0,clean|7.0.1""
  [green]-t, --template-version[/] <ver>  Template version (Latest, LTS, or specific version)
  [green]-n, --project-name[/] <name>     Project name (default: MyUmbracoProject)
  [green]-s, --solution[/]                Create a solution file
  [green]    --solution-name[/] <name>    Solution name (used with --solution)

[bold yellow]STARTER KIT:[/]
  [green]-k, --starter-kit[/]             Include a starter kit
  [green]    --starter-kit-package[/] <pkg> Starter kit package name

[bold yellow]DOCKER:[/]
  [green]    --dockerfile[/]              Include Dockerfile
  [green]    --docker-compose[/]          Include Docker Compose file

[bold yellow]UNATTENDED INSTALL:[/]
  [green]-u, --unattended[/]              Use unattended install
  [green]    --database-type[/] <type>    Database type (SQLite, LocalDb, SQLServer, SQLAzure, SQLCE)
  [green]    --connection-string[/] <str> Connection string (for SQLServer/SQLAzure)
  [green]    --admin-name[/] <name>       Admin user friendly name
  [green]    --admin-email[/] <email>     Admin email
  [green]    --admin-password[/] <pwd>    Admin password

[bold yellow]OUTPUT OPTIONS:[/]
  [green]-o, --oneliner[/]                Output as one-liner
  [green]-r, --remove-comments[/]         Remove comments from script
  [green]    --include-prerelease[/]      Include prerelease package versions

[bold yellow]EXECUTION:[/]
  [green]    --auto-run[/]                Automatically run the generated script
  [green]    --run-dir[/] <directory>     Directory to run script in

[bold yellow]EXAMPLES:[/]
  Generate default script:
    [cyan]psw --default[/]

  Generate custom script with packages (latest versions):
    [cyan]psw --packages ""uSync,Umbraco.Forms"" --project-name MyProject[/]

  Generate script with specific package versions:
    [cyan]psw --packages ""uSync|17.0.0,clean|7.0.1"" --project-name MyProject[/]

  Mixed: some with versions, some without:
    [cyan]psw -p ""uSync|17.0.0,Umbraco.Forms"" -n MyProject[/]

  Full configuration example:
    [cyan]psw -p ""uSync|17.0.0"" -n MyProject -s --solution-name MySolution -u --database-type SQLite --admin-email admin@test.com --admin-password MyPass123! --auto-run[/]

  Interactive mode (no flags):
    [cyan]psw[/]")
            .Header("[bold blue]Package Script Writer Help[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue)
            .Padding(1, 1);

        AnsiConsole.Write(helpPanel);
    }

    /// <summary>
    /// Displays version information
    /// </summary>
    private static void DisplayVersion()
    {
        AnsiConsole.Write(
            new FigletText("PSW CLI")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine($"[bold]Version:[/] {Version}");
        AnsiConsole.MarkupLine("[dim]Package Script Writer CLI Tool[/]");
        AnsiConsole.MarkupLine("[dim]By Paul Seal[/]");
        AnsiConsole.MarkupLine($"[dim]https://github.com/prjseal/Package-Script-Writer[/]");
    }

    /// <summary>
    /// Generates a custom script from command-line options
    /// </summary>
    private static async Task GenerateCustomScriptFromOptionsAsync(CommandLineOptions options)
    {
        var apiClient = new ApiClient(ApiBaseUrl);
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = options.TemplateVersion ?? "",
            ProjectName = options.ProjectName ?? "MyUmbracoProject",
            CreateSolutionFile = options.CreateSolution,
            SolutionName = options.SolutionName,
            IncludeStarterKit = options.IncludeStarterKit,
            StarterKitPackage = options.StarterKitPackage,
            IncludeDockerfile = options.IncludeDockerfile,
            IncludeDockerCompose = options.IncludeDockerCompose,
            CanIncludeDocker = options.IncludeDockerfile || options.IncludeDockerCompose,
            UseUnattendedInstall = options.UseUnattended,
            DatabaseType = options.DatabaseType,
            ConnectionString = options.ConnectionString,
            UserFriendlyName = options.AdminName,
            UserEmail = options.AdminEmail,
            UserPassword = options.AdminPassword,
            OnelinerOutput = options.OnelinerOutput,
            RemoveComments = options.RemoveComments
        };

        // Handle packages
        if (!string.IsNullOrWhiteSpace(options.Packages))
        {
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
                            processedPackages.Add($"{packageName}|{version}");
                            AnsiConsole.MarkupLine($"[green]✓[/] Using {packageName} version {version}");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"[yellow]⚠[/] Invalid package format: {entry}, skipping...");
                        }
                    }
                    else
                    {
                        // No version specified, use package name without version
                        var packageName = entry.Trim();
                        processedPackages.Add(packageName);
                        AnsiConsole.MarkupLine($"[green]✓[/] Using {packageName} (latest version)");
                    }
                }

                // Build packages string - can be mixed format: "Package1|Version1,Package2,Package3|Version3"
                if (processedPackages.Count > 0)
                {
                    model.Packages = string.Join(",", processedPackages);
                }
            }
        }

        // Generate the script
        try
        {
            var script = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Generating installation script...", async ctx =>
                {
                    return await apiClient.GenerateScriptAsync(new ScriptRequest { Model = model });
                });

            // Display the generated script
            AnsiConsole.WriteLine();
            var panel = new Panel(script)
                .Header("[bold green]Generated Installation Script[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Padding(1, 1);

            AnsiConsole.Write(panel);

            // Handle auto-run or interactive run
            if (options.AutoRun || !string.IsNullOrWhiteSpace(options.RunDirectory))
            {
                var targetDir = options.RunDirectory ?? Directory.GetCurrentDirectory();

                // Expand path and verify it exists
                targetDir = Path.GetFullPath(targetDir);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                    AnsiConsole.MarkupLine($"[green]✓ Created directory {targetDir}[/]");
                }

                await RunScriptAsync(script, targetDir);
            }
            else
            {
                // Option to save and run the script
                await HandleScriptSaveAndRunAsync(script);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error generating script: {ex.Message}[/]");
        }
    }

    /// <summary>
    /// Runs the custom configuration flow for script generation
    /// </summary>
    private static async Task RunCustomFlowAsync()
    {
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
    }

    /// <summary>
    /// Populates the allPackages list from the API
    /// </summary>
    private static async Task PopulateAllPackagesAsync()
    {
        var apiClient = new ApiClient(ApiBaseUrl);

        try
        {
            allPackages = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Loading available packages...", async ctx =>
                {
                    return await apiClient.GetAllPackagesAsync();
                });

            AnsiConsole.MarkupLine($"[green]✓[/] Loaded {allPackages.Count} packages from marketplace");
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]⚠[/] Unable to load packages from API: {ex.Message}");
            AnsiConsole.MarkupLine("[dim]Continuing with limited package selection...[/]");
            AnsiConsole.WriteLine();
            allPackages = new List<PagedPackagesPackage>();
        }
    }

    /// <summary>
    /// Displays the welcome banner using Spectre.Console
    /// </summary>
    private static void DisplayWelcomeBanner()
    {
        AnsiConsole.Write(
            new FigletText("PSW CLI")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[dim]Package Script Writer - Interactive CLI[/]");
        AnsiConsole.MarkupLine("[dim]By Paul Seal[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Generates a default script with minimal configuration
    /// </summary>
    private static async Task GenerateDefaultScriptAsync(CommandLineOptions? options = null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Generating Default Script[/]\n");
        AnsiConsole.MarkupLine("[dim]Using default configuration (latest stable Umbraco, no packages)[/]");
        AnsiConsole.WriteLine();

        // Create default script model
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = "", // Latest stable
            ProjectName = "MyUmbracoProject",
            CreateSolutionFile = false,
            IncludeStarterKit = false,
            IncludeDockerfile = false,
            IncludeDockerCompose = false,
            CanIncludeDocker = false,
            UseUnattendedInstall = false,
            OnelinerOutput = false,
            RemoveComments = false
        };

        var apiClient = new ApiClient(ApiBaseUrl);

        try
        {
            var script = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Generating default installation script...", async ctx =>
                {
                    return await apiClient.GenerateScriptAsync(new ScriptRequest { Model = model });
                });

            // Display the generated script in a panel
            AnsiConsole.WriteLine();
            var panel = new Panel(script)
                .Header("[bold green]Generated Default Installation Script[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Padding(1, 1);

            AnsiConsole.Write(panel);

            // Handle auto-run or interactive run
            if (options?.AutoRun == true || !string.IsNullOrWhiteSpace(options?.RunDirectory))
            {
                var targetDir = options?.RunDirectory ?? Directory.GetCurrentDirectory();

                // Expand path and verify it exists
                targetDir = Path.GetFullPath(targetDir);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                    AnsiConsole.MarkupLine($"[green]✓ Created directory {targetDir}[/]");
                }

                await RunScriptAsync(script, targetDir);
            }
            else
            {
                // Option to save and run the script
                await HandleScriptSaveAndRunAsync(script);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error generating script: {ex.Message}[/]");
        }
    }

    /// <summary>
    /// Handles running or editing the generated script
    /// </summary>
    private static async Task HandleScriptSaveAndRunAsync(string script)
    {
        // Ask user what they want to do with the script
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nWhat would you like to do with this script?")
                .AddChoices(new[] { "Edit", "Run" }));

        if (action == "Run")
        {
            var currentDir = Directory.GetCurrentDirectory();
            var targetDir = AnsiConsole.Ask<string>(
                $"Enter [green]directory path[/] to run the script (leave blank for current directory: {currentDir}):",
                string.Empty);

            if (string.IsNullOrWhiteSpace(targetDir))
            {
                targetDir = currentDir;
            }
            else
            {
                // Expand path and verify it exists
                targetDir = Path.GetFullPath(targetDir);
                if (!Directory.Exists(targetDir))
                {
                    if (AnsiConsole.Confirm($"Directory [yellow]{targetDir}[/] doesn't exist. Create it?"))
                    {
                        Directory.CreateDirectory(targetDir);
                        AnsiConsole.MarkupLine($"[green]✓ Created directory {targetDir}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Script execution cancelled.[/]");
                        return;
                    }
                }
            }

            await RunScriptAsync(script, targetDir);
        }
        else if (action == "Edit")
        {
            AnsiConsole.MarkupLine("\n[blue]Let's configure a custom script...[/]\n");
            await RunCustomFlowAsync();
        }
    }

    /// <summary>
    /// Executes the script content in the specified directory
    /// </summary>
    private static async Task RunScriptAsync(string scriptContent, string workingDirectory)
    {
        try
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold blue]Running script in:[/] {workingDirectory}");
            AnsiConsole.WriteLine();

            // Determine shell for script execution
            string shell;
            if (OperatingSystem.IsWindows())
            {
                shell = "cmd.exe";
            }
            else
            {
                shell = "/bin/bash";
            }

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = shell,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    AnsiConsole.MarkupLine($"[dim]{e.Data.EscapeMarkup()}[/]");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    AnsiConsole.MarkupLine($"[red]{e.Data.EscapeMarkup()}[/]");
                }
            };

            process.Start();

            // Write the script content to stdin
            await process.StandardInput.WriteAsync(scriptContent);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[green]✓ Script executed successfully![/]");
            }
            else
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[yellow]⚠ Script exited with code {process.ExitCode}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error running script: {ex.Message}[/]");
        }
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
                .AddChoices(new[] { "+ Add custom package..." }));

        var selectedPackages = new List<string>();

        // Process selections
        foreach (var selection in selections)
        {
            if (selection == "+ Add custom package...")
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
                        return await apiClient.GetPackageVersionsAsync(package, includePrerelease: true);
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
        AnsiConsole.MarkupLine("[bold blue]Step 4:[/] Configure Project Options\n");

        // Build packages string in format: "Package1|Version1,Package2|Version2"
        var packagesString = string.Join(",", packageVersions.Select(kvp => $"{kvp.Key}|{kvp.Value}"));

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

        model.ProjectName = AnsiConsole.Ask<string>("Enter [green]project name[/]:", "MyUmbracoProject");

        model.CreateSolutionFile = AnsiConsole.Confirm("Create a [green]solution file[/]?", false);

        if (model.CreateSolutionFile)
        {
            model.SolutionName = AnsiConsole.Ask<string>("Enter [green]solution name[/]:", model.ProjectName);
        }

        // Starter Kit Configuration
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Starter Kit Options[/]\n");

        model.IncludeStarterKit = AnsiConsole.Confirm("Include a [green]starter kit[/]?", false);

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

        model.UseUnattendedInstall = AnsiConsole.Confirm("Use [green]unattended install[/]?", false);

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
                    .Secret());
        }

        // Output Format Options
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Output Format Options[/]\n");

        model.OnelinerOutput = AnsiConsole.Confirm("Output as [green]one-liner[/]?", false);
        model.RemoveComments = AnsiConsole.Confirm("Remove [green]comments[/] from script?", false);

        // Display configuration summary
        DisplayConfigurationSummary(model, packageVersions);

        // Confirm generation
        if (!AnsiConsole.Confirm("\nGenerate script with these settings?", true))
        {
            AnsiConsole.MarkupLine("[yellow]Script generation cancelled.[/]");
            return;
        }

        var apiClient = new ApiClient(ApiBaseUrl);

        try
        {
            var script = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Generating installation script...", async ctx =>
                {
                    return await apiClient.GenerateScriptAsync(new ScriptRequest { Model = model });
                });

            // Display the generated script in a panel
            AnsiConsole.WriteLine();
            var panel = new Panel(script)
                .Header("[bold green]Generated Installation Script[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Padding(1, 1);

            AnsiConsole.Write(panel);

            // Option to save and run the script
            await HandleScriptSaveAndRunAsync(script);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error generating script: {ex.Message}[/]");
        }
    }

    /// <summary>
    /// Displays a summary of the configuration before generating the script
    /// </summary>
    private static void DisplayConfigurationSummary(ScriptModel model, Dictionary<string, string> packageVersions)
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
            Timeout = TimeSpan.FromSeconds(90)
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

            // Try to deserialize as a raw array first
            try
            {
                var versions = JsonSerializer.Deserialize<List<string>>(responseContent);
                if (versions != null)
                    return versions;
            }
            catch
            {
                // Fallback to object with 'versions' property
                var result = JsonSerializer.Deserialize<PackageVersionResponse>(responseContent);
                return result?.Versions ?? new List<string>();
            }

            return new List<string>();
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

            // Log the raw response for debugging
            AnsiConsole.MarkupLine($"[yellow]Raw script API response: {responseContent}[/]");

            var result = responseContent;

            return result ?? "# No script generated.";
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

    /// <summary>
    /// Retrieves all available Umbraco packages from the marketplace
    /// </summary>
    public async Task<List<PagedPackagesPackage>> GetAllPackagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/scriptgeneratorapi/getallpackages");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var packages = JsonSerializer.Deserialize<List<PagedPackagesPackage>>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return packages ?? new List<PagedPackagesPackage>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to fetch all packages: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error fetching all packages: {ex.Message}", ex);
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

/// <summary>
/// Command-line options parser and container
/// </summary>
public class CommandLineOptions
{
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }
    public bool UseDefault { get; set; }
    public string? Packages { get; set; }
    public string? TemplateVersion { get; set; }
    public string? ProjectName { get; set; }
    public bool CreateSolution { get; set; }
    public string? SolutionName { get; set; }
    public bool IncludeStarterKit { get; set; }
    public string? StarterKitPackage { get; set; }
    public bool IncludeDockerfile { get; set; }
    public bool IncludeDockerCompose { get; set; }
    public bool UseUnattended { get; set; }
    public string? DatabaseType { get; set; }
    public string? ConnectionString { get; set; }
    public string? AdminName { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
    public bool OnelinerOutput { get; set; }
    public bool RemoveComments { get; set; }
    public bool IncludePrerelease { get; set; }
    public bool AutoRun { get; set; }
    public string? RunDirectory { get; set; }

    /// <summary>
    /// Checks if any configuration options are set (excluding help/version/default)
    /// </summary>
    public bool HasAnyOptions()
    {
        return UseDefault ||
               !string.IsNullOrWhiteSpace(Packages) ||
               !string.IsNullOrWhiteSpace(TemplateVersion) ||
               !string.IsNullOrWhiteSpace(ProjectName) ||
               CreateSolution ||
               !string.IsNullOrWhiteSpace(SolutionName) ||
               IncludeStarterKit ||
               !string.IsNullOrWhiteSpace(StarterKitPackage) ||
               IncludeDockerfile ||
               IncludeDockerCompose ||
               UseUnattended ||
               !string.IsNullOrWhiteSpace(DatabaseType) ||
               !string.IsNullOrWhiteSpace(ConnectionString) ||
               !string.IsNullOrWhiteSpace(AdminName) ||
               !string.IsNullOrWhiteSpace(AdminEmail) ||
               !string.IsNullOrWhiteSpace(AdminPassword) ||
               OnelinerOutput ||
               RemoveComments ||
               IncludePrerelease ||
               AutoRun ||
               !string.IsNullOrWhiteSpace(RunDirectory);
    }

    /// <summary>
    /// Parses command-line arguments into options
    /// </summary>
    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg.ToLower())
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "-v":
                case "--version":
                    options.ShowVersion = true;
                    break;

                case "-d":
                case "--default":
                    options.UseDefault = true;
                    break;

                case "-p":
                case "--packages":
                    options.Packages = GetNextArgument(args, ref i);
                    break;

                case "-t":
                case "--template-version":
                    options.TemplateVersion = GetNextArgument(args, ref i);
                    break;

                case "-n":
                case "--project-name":
                    options.ProjectName = GetNextArgument(args, ref i);
                    break;

                case "-s":
                case "--solution":
                    options.CreateSolution = true;
                    break;

                case "--solution-name":
                    options.SolutionName = GetNextArgument(args, ref i);
                    break;

                case "-k":
                case "--starter-kit":
                    options.IncludeStarterKit = true;
                    break;

                case "--starter-kit-package":
                    options.StarterKitPackage = GetNextArgument(args, ref i);
                    break;

                case "--dockerfile":
                    options.IncludeDockerfile = true;
                    break;

                case "--docker-compose":
                    options.IncludeDockerCompose = true;
                    break;

                case "-u":
                case "--unattended":
                    options.UseUnattended = true;
                    break;

                case "--database-type":
                    options.DatabaseType = GetNextArgument(args, ref i);
                    break;

                case "--connection-string":
                    options.ConnectionString = GetNextArgument(args, ref i);
                    break;

                case "--admin-name":
                    options.AdminName = GetNextArgument(args, ref i);
                    break;

                case "--admin-email":
                    options.AdminEmail = GetNextArgument(args, ref i);
                    break;

                case "--admin-password":
                    options.AdminPassword = GetNextArgument(args, ref i);
                    break;

                case "-o":
                case "--oneliner":
                    options.OnelinerOutput = true;
                    break;

                case "-r":
                case "--remove-comments":
                    options.RemoveComments = true;
                    break;

                case "--include-prerelease":
                    options.IncludePrerelease = true;
                    break;

                case "--auto-run":
                    options.AutoRun = true;
                    break;

                case "--run-dir":
                    options.RunDirectory = GetNextArgument(args, ref i);
                    break;

                default:
                    // Unknown argument - ignore or warn
                    if (arg.StartsWith("-"))
                    {
                        AnsiConsole.MarkupLine($"[yellow]Warning: Unknown option '{arg}' (use --help for available options)[/]");
                    }
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Gets the next argument value from the args array
    /// </summary>
    private static string? GetNextArgument(string[] args, ref int index)
    {
        if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
        {
            index++;
            return args[index];
        }

        return null;
    }
}
