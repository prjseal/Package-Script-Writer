using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using Spectre.Console;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PackageCliTool.UI;

/// <summary>
/// Provides output methods that respect the chosen OutputFormat.
/// When format is Json or Plain, Spectre.Console markup is suppressed.
/// </summary>
public static class OutputHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Returns true if the format suppresses Spectre.Console rich output
    /// </summary>
    public static bool IsMachineReadable(OutputFormat format) =>
        format is OutputFormat.Json or OutputFormat.Plain;

    /// <summary>
    /// Writes a status message to stderr (visible to humans, ignored by piped output).
    /// Suppressed entirely in Json mode.
    /// </summary>
    public static void Status(string message, OutputFormat format)
    {
        if (format == OutputFormat.Json) return;
        if (format == OutputFormat.Plain)
        {
            Console.Error.WriteLine(message);
            return;
        }
        AnsiConsole.MarkupLine(message);
    }

    /// <summary>
    /// Writes a generated script to the appropriate output stream.
    /// </summary>
    public static void WriteScript(string script, OutputFormat format, ScriptModel? configuration = null, string? title = null)
    {
        switch (format)
        {
            case OutputFormat.Json:
                WriteScriptJson(script, configuration);
                break;
            case OutputFormat.Plain:
                Console.Write(script);
                break;
            default:
                ConsoleDisplay.DisplayGeneratedScript(script, title ?? "Generated Installation Script");
                break;
        }
    }

    /// <summary>
    /// Writes the script as a structured JSON response to stdout
    /// </summary>
    private static void WriteScriptJson(string script, ScriptModel? configuration)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        var response = new
        {
            success = true,
            script,
            configuration = configuration != null ? new
            {
                templateName = configuration.TemplateName,
                templateVersion = configuration.TemplateVersion,
                projectName = configuration.ProjectName,
                solutionName = configuration.SolutionName,
                createSolutionFile = configuration.CreateSolutionFile,
                packages = configuration.Packages,
                includeStarterKit = configuration.IncludeStarterKit,
                starterKitPackage = configuration.StarterKitPackage,
                includeDockerfile = configuration.IncludeDockerfile,
                includeDockerCompose = configuration.IncludeDockerCompose,
                enableContentDeliveryApi = configuration.EnableContentDeliveryApi,
                useUnattendedInstall = configuration.UseUnattendedInstall,
                databaseType = configuration.DatabaseType,
                onelinerOutput = configuration.OnelinerOutput,
                removeComments = configuration.RemoveComments
            } : null,
            metadata = new
            {
                generatedAt = DateTime.UtcNow.ToString("o"),
                cliVersion = version
            }
        };
        Console.WriteLine(JsonSerializer.Serialize(response, JsonOptions));
    }

    /// <summary>
    /// Writes an error as JSON to stdout (for machine-readable error handling)
    /// </summary>
    public static void WriteErrorJson(string error, string? errorCode = null, string? suggestion = null)
    {
        var response = new
        {
            success = false,
            error,
            errorCode,
            suggestion
        };
        Console.WriteLine(JsonSerializer.Serialize(response, JsonOptions));
    }

    /// <summary>
    /// Writes structured JSON output for version information
    /// </summary>
    public static void WriteVersionJson()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        var response = new
        {
            name = "PackageScriptWriter.Cli",
            version,
            runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            platform = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier
        };
        Console.WriteLine(JsonSerializer.Serialize(response, JsonOptions));
    }

    /// <summary>
    /// Writes plain version string (just the number)
    /// </summary>
    public static void WriteVersionPlain()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        Console.WriteLine(version);
    }

    /// <summary>
    /// Writes the full help information as structured JSON
    /// </summary>
    public static void WriteHelpJson()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        var help = new
        {
            name = "psw",
            description = "Package Script Writer - Generate Umbraco CMS installation scripts",
            version,
            usage = new[]
            {
                "psw [options]",
                "psw template <command> [options]",
                "psw history <command> [options]",
                "psw versions",
                "psw list-options [category]"
            },
            options = new object[]
            {
                new { name = "--help", shortName = "-h", type = "flag", description = "Show help information" },
                new { name = "--help-json", shortName = (string?)null, type = "flag", description = "Show help as structured JSON (for AI agents and tooling)" },
                new { name = "--version", shortName = "-v", type = "flag", description = "Show version information" },
                new { name = "--default", shortName = "-d", type = "flag", description = "Generate a default script with minimal configuration" },
                new { name = "--packages", shortName = "-p", type = "string", required = false, description = "Comma-separated list of packages with optional versions", format = "Package1|Version1,Package2|Version2", examples = new[] { "uSync|17.0.0,Umbraco.Forms", "uSync,Diplo.GodMode" } },
                new { name = "--template-package", shortName = "-t", type = "string", required = false, description = "Template package name with optional version", format = "PackageName|Version", examples = new[] { "Umbraco.Templates|17.0.3", "Umbraco.Templates" } },
                new { name = "--template-version", shortName = (string?)null, type = "string", required = false, description = "Template version (alternative to pipe syntax in --template-package)", format = "major.minor.patch", examples = new[] { "17.0.3", "14.0.0" } },
                new { name = "--project-name", shortName = "-n", type = "string", required = false, description = "Project name", @default = "MyProject" },
                new { name = "--solution", shortName = "-s", type = "string", required = false, description = "Solution name (also enables solution file creation)" },
                new { name = "--starter-kit", shortName = "-k", type = "string", required = false, description = "Starter kit package with optional version", format = "PackageName|Version", validValues = new[] { "clean", "Articulate", "Portfolio", "LittleNorth.Igloo", "Umbraco.BlockGrid.Example.Website", "Umbraco.TheStarterKit", "uSkinnedSiteBuilder" } },
                new { name = "--dockerfile", shortName = (string?)null, type = "flag", description = "Include Dockerfile in generated script" },
                new { name = "--docker-compose", shortName = (string?)null, type = "flag", description = "Include Docker Compose file in generated script" },
                new { name = "--delivery-api", shortName = "-da", type = "flag", description = "Enable Content Delivery API" },
                new { name = "--unattended-defaults", shortName = "-u", type = "flag", description = "Use unattended install with defaults (SQLite, admin@example.com, 1234567890)" },
                new { name = "--database-type", shortName = (string?)null, type = "enum", required = false, description = "Database type for unattended install. Implies --unattended-defaults.", validValues = new[] { "SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE" }, note = "SQLServer and SQLAzure require --connection-string" },
                new { name = "--connection-string", shortName = (string?)null, type = "string", required = false, description = "Database connection string (required for SQLServer/SQLAzure)" },
                new { name = "--admin-name", shortName = (string?)null, type = "string", required = false, description = "Admin user friendly name for unattended install" },
                new { name = "--admin-email", shortName = (string?)null, type = "string", required = false, description = "Admin email for unattended install", format = "email" },
                new { name = "--admin-password", shortName = (string?)null, type = "string", required = false, description = "Admin password for unattended install (min 10 characters)" },
                new { name = "--oneliner", shortName = "-o", type = "flag", description = "Output script as one-liner" },
                new { name = "--remove-comments", shortName = "-r", type = "flag", description = "Remove comments from generated script" },
                new { name = "--include-prerelease", shortName = (string?)null, type = "flag", description = "Include prerelease package versions" },
                new { name = "--auto-run", shortName = (string?)null, type = "flag", description = "Automatically run the generated script" },
                new { name = "--no-run", shortName = (string?)null, type = "flag", description = "Skip 'dotnet run' from the generated script" },
                new { name = "--run-dir", shortName = (string?)null, type = "string", required = false, description = "Directory to run script in" },
                new { name = "--save-only", shortName = (string?)null, type = "flag", description = "Save script to file (via --output-file) and exit without interactive prompts" },
                new { name = "--output-file", shortName = (string?)null, type = "string", required = false, description = "Output file path for saving the generated script" },
                new { name = "--community-template", shortName = (string?)null, type = "string", required = false, description = "Load a community template by name, or 'list' to show all" },
                new { name = "--output", shortName = (string?)null, type = "enum", required = false, description = "Output format", validValues = new[] { "json", "plain" }, @default = "default (rich Spectre.Console)" },
                new { name = "--script-only", shortName = (string?)null, type = "flag", description = "Output only the raw script text, no decoration or status messages" },
                new { name = "--no-interaction", shortName = (string?)null, type = "flag", description = "Suppress all interactive prompts. Fails if user input would be required." },
                new { name = "--dry-run", shortName = (string?)null, type = "flag", description = "Validate inputs and show configuration without generating a script" },
                new { name = "--verbose", shortName = (string?)null, type = "flag", description = "Enable verbose logging" },
                new { name = "--clear-cache", shortName = (string?)null, type = "flag", description = "Clear all cached API responses" }
            },
            commands = new object[]
            {
                new
                {
                    name = "template",
                    description = "Manage script configuration templates",
                    subcommands = new object[]
                    {
                        new { name = "save", arguments = new[] { new { name = "name", required = true, description = "Template name" } }, description = "Save current configuration as a template" },
                        new { name = "load", arguments = new[] { new { name = "name", required = true, description = "Template name" } }, description = "Load and execute a template" },
                        new { name = "list", arguments = Array.Empty<object>(), description = "List all available templates" },
                        new { name = "delete", arguments = new[] { new { name = "name", required = true, description = "Template name" } }, description = "Delete a template" },
                        new { name = "export", arguments = new[] { new { name = "name", required = true, description = "Template name" } }, description = "Export template to file" },
                        new { name = "import", arguments = new[] { new { name = "file", required = true, description = "Template file path" } }, description = "Import template from file" },
                        new { name = "validate", arguments = new[] { new { name = "file", required = true, description = "Template file path" } }, description = "Validate template file" }
                    }
                },
                new
                {
                    name = "history",
                    description = "Manage script generation history",
                    subcommands = new object[]
                    {
                        new { name = "list", arguments = Array.Empty<object>(), description = "List recent script generation history" },
                        new { name = "rerun", arguments = new[] { new { name = "number", required = true, description = "History entry number" } }, description = "Regenerate and re-run a script from history" },
                        new { name = "delete", arguments = new[] { new { name = "number", required = true, description = "History entry number" } }, description = "Delete a history entry" },
                        new { name = "clear", arguments = Array.Empty<object>(), description = "Clear all history" }
                    }
                },
                new
                {
                    name = "versions",
                    description = "Display Umbraco versions table with support lifecycle information",
                    subcommands = Array.Empty<object>()
                },
                new
                {
                    name = "list-options",
                    description = "List valid values for CLI options (for AI agents and tooling)",
                    subcommands = new object[]
                    {
                        new { name = "database-types", arguments = Array.Empty<object>(), description = "List valid database types" },
                        new { name = "starter-kits", arguments = Array.Empty<object>(), description = "List available starter kits" },
                        new { name = "defaults", arguments = Array.Empty<object>(), description = "Show default values for all options" }
                    }
                }
            }
        };

        Console.WriteLine(JsonSerializer.Serialize(help, JsonOptions));
    }
}
