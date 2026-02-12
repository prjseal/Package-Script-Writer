using Microsoft.Extensions.Logging;
using PackageCliTool.Models;
using PackageCliTool.UI;
using Spectre.Console;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PackageCliTool.Workflows;

/// <summary>
/// Handles the list-options command, which outputs valid values for CLI options.
/// Designed primarily for AI agents and tooling to discover valid inputs.
/// </summary>
public class ListOptionsWorkflow
{
    private readonly ILogger? _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Valid database types accepted by --database-type</summary>
    public static readonly string[] DatabaseTypes = { "SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE" };

    /// <summary>Available starter kits accepted by --starter-kit</summary>
    public static readonly string[] StarterKits = { "clean", "Articulate", "Portfolio", "LittleNorth.Igloo", "Umbraco.BlockGrid.Example.Website", "Umbraco.TheStarterKit", "uSkinnedSiteBuilder" };

    public ListOptionsWorkflow(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Runs the list-options command
    /// </summary>
    public void Run(CommandLineOptions options)
    {
        _logger?.LogInformation("Listing options: {Category}", options.ListOptionsCommand);

        var category = options.ListOptionsCommand?.ToLower() ?? "";

        if (options.OutputFormat == OutputFormat.Json)
        {
            WriteJson(category);
            return;
        }

        switch (category)
        {
            case "database-types":
                WriteDatabaseTypes(options.OutputFormat);
                break;
            case "starter-kits":
                WriteStarterKits(options.OutputFormat);
                break;
            case "defaults":
                WriteDefaults(options.OutputFormat);
                break;
            case "":
                WriteAll(options.OutputFormat);
                break;
            default:
                if (options.OutputFormat == OutputFormat.Plain)
                {
                    Console.Error.WriteLine($"Unknown category: {category}");
                    Console.Error.WriteLine("Valid categories: database-types, starter-kits, defaults");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Unknown category:[/] {category}");
                    AnsiConsole.MarkupLine("[dim]Valid categories: database-types, starter-kits, defaults[/]");
                }
                Environment.ExitCode = ExitCodes.ValidationError;
                break;
        }
    }

    private void WriteJson(string category)
    {
        object result = category switch
        {
            "database-types" => new { databaseTypes = DatabaseTypes },
            "starter-kits" => new { starterKits = StarterKits },
            "defaults" => GetDefaultsObject(),
            _ => new
            {
                databaseTypes = DatabaseTypes,
                starterKits = StarterKits,
                defaults = GetDefaultsObject()
            }
        };

        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
    }

    private static object GetDefaultsObject() => new
    {
        projectName = "MyProject",
        solutionName = "MySolution",
        templatePackage = "Umbraco.Templates",
        databaseType = "SQLite",
        adminEmail = "admin@example.com",
        adminPassword = "1234567890",
        adminName = "Administrator",
        starterKit = "clean",
        createSolution = true,
        includeStarterKit = true,
        useUnattendedInstall = true
    };

    private void WriteDatabaseTypes(OutputFormat format)
    {
        if (format == OutputFormat.Plain)
        {
            foreach (var type in DatabaseTypes)
                Console.WriteLine(type);
            return;
        }

        AnsiConsole.MarkupLine("[bold yellow]Valid Database Types:[/]");
        foreach (var type in DatabaseTypes)
            AnsiConsole.MarkupLine($"  [green]{type}[/]");
        AnsiConsole.MarkupLine("\n[dim]Usage: psw --database-type SQLite[/]");
    }

    private void WriteStarterKits(OutputFormat format)
    {
        if (format == OutputFormat.Plain)
        {
            foreach (var kit in StarterKits)
                Console.WriteLine(kit);
            return;
        }

        AnsiConsole.MarkupLine("[bold yellow]Available Starter Kits:[/]");
        foreach (var kit in StarterKits)
            AnsiConsole.MarkupLine($"  [green]{kit}[/]");
        AnsiConsole.MarkupLine("\n[dim]Usage: psw --starter-kit clean[/]");
    }

    private void WriteDefaults(OutputFormat format)
    {
        if (format == OutputFormat.Plain)
        {
            Console.WriteLine("projectName=MyProject");
            Console.WriteLine("solutionName=MySolution");
            Console.WriteLine("templatePackage=Umbraco.Templates");
            Console.WriteLine("databaseType=SQLite");
            Console.WriteLine("adminEmail=admin@example.com");
            Console.WriteLine("adminPassword=1234567890");
            Console.WriteLine("adminName=Administrator");
            Console.WriteLine("starterKit=clean");
            Console.WriteLine("createSolution=true");
            Console.WriteLine("includeStarterKit=true");
            Console.WriteLine("useUnattendedInstall=true");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .Title("[bold blue]Default Values[/]");

        table.AddColumn("[bold]Option[/]");
        table.AddColumn("[bold]Default[/]");

        table.AddRow("--project-name", "MyProject");
        table.AddRow("--solution", "MySolution");
        table.AddRow("--template-package", "Umbraco.Templates");
        table.AddRow("--database-type", "SQLite");
        table.AddRow("--admin-email", "admin@example.com");
        table.AddRow("--admin-password", "1234567890");
        table.AddRow("--admin-name", "Administrator");
        table.AddRow("--starter-kit", "clean");

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[dim]These are the defaults used with --default flag.[/]");
    }

    private void WriteAll(OutputFormat format)
    {
        if (format == OutputFormat.Plain)
        {
            Console.WriteLine("# Database Types");
            WriteDatabaseTypes(format);
            Console.WriteLine();
            Console.WriteLine("# Starter Kits");
            WriteStarterKits(format);
            Console.WriteLine();
            Console.WriteLine("# Defaults");
            WriteDefaults(format);
            return;
        }

        AnsiConsole.MarkupLine("[bold blue]Available Option Categories[/]\n");
        WriteDatabaseTypes(format);
        AnsiConsole.WriteLine();
        WriteStarterKits(format);
        AnsiConsole.WriteLine();
        WriteDefaults(format);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Tip: Use psw list-options <category> to show a specific category[/]");
        AnsiConsole.MarkupLine("[dim]     Use psw list-options --output json for machine-readable output[/]");
    }
}
