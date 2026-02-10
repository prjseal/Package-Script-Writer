using Spectre.Console;

namespace PackageCliTool.Models;

/// <summary>
/// Command-line options parser and container
/// </summary>
public class CommandLineOptions
{
    /// <summary>Gets or sets whether to show help information</summary>
    public bool ShowHelp { get; set; }

    /// <summary>Gets or sets whether to show version information</summary>
    public bool ShowVersion { get; set; }

    /// <summary>Gets or sets whether to use default values without prompts</summary>
    public bool UseDefault { get; set; }

    /// <summary>Gets or sets the comma-separated list of packages to install</summary>
    public string? Packages { get; set; }

    /// <summary>Gets or sets the template package name</summary>
    public string? TemplatePackageName { get; set; }

    /// <summary>Gets or sets the template version</summary>
    public string? TemplateVersion { get; set; }

    /// <summary>Gets or sets the project name</summary>
    public string? ProjectName { get; set; }

    /// <summary>Gets or sets whether to create a solution file</summary>
    public bool? CreateSolution { get; set; }

    /// <summary>Gets or sets the solution name</summary>
    public string? SolutionName { get; set; }

    /// <summary>Gets or sets whether to include a starter kit</summary>
    public bool? IncludeStarterKit { get; set; }

    /// <summary>Gets or sets the starter kit package name</summary>
    public string? StarterKitPackage { get; set; }

    /// <summary>Gets or sets the starter kit version</summary>
    public string? StarterKitVersion { get; set; }

    /// <summary>Gets or sets whether to include a Dockerfile</summary>
    public bool? IncludeDockerfile { get; set; }

    /// <summary>Gets or sets whether to include Docker Compose configuration</summary>
    public bool? IncludeDockerCompose { get; set; }

    /// <summary>Gets or sets whether to enable the Content Delivery API</summary>
    public bool? EnableContentDeliveryApi { get; set; }

    /// <summary>Gets or sets whether to use unattended installation</summary>
    public bool? UseUnattended { get; set; }

    /// <summary>Gets or sets the database type</summary>
    public string? DatabaseType { get; set; }

    /// <summary>Gets or sets the database connection string</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Gets or sets the admin user name</summary>
    public string? AdminName { get; set; }

    /// <summary>Gets or sets the admin user email</summary>
    public string? AdminEmail { get; set; }

    /// <summary>Gets or sets the admin user password</summary>
    public string? AdminPassword { get; set; }

    /// <summary>Gets or sets whether to output as a one-liner script</summary>
    public bool? OnelinerOutput { get; set; }

    /// <summary>Gets or sets whether to remove comments from the generated script</summary>
    public bool? RemoveComments { get; set; }

    /// <summary>Gets or sets whether to include prerelease versions</summary>
    public bool? IncludePrerelease { get; set; }

    /// <summary>Gets or sets whether to automatically run the generated script</summary>
    public bool AutoRun { get; set; }

    /// <summary>Gets or sets the directory where the script should be run</summary>
    public string? RunDirectory { get; set; }

    /// <summary>Gets or sets whether to enable verbose logging</summary>
    public bool VerboseMode { get; set; }

    /// <summary>Gets or sets the template command (save, load, list, delete, export, import)</summary>
    public string? TemplateCommand { get; set; }

    /// <summary>Gets or sets the template name</summary>
    public string? TemplateName { get; set; }

    /// <summary>Gets or sets the template description</summary>
    public string? TemplateDescription { get; set; }

    /// <summary>Gets or sets the template file path</summary>
    public string? TemplateFile { get; set; }

    /// <summary>Gets or sets the template tags</summary>
    public List<string> TemplateTags { get; set; } = new();

    /// <summary>Gets or sets the community template name or 'list' to show all available templates</summary>
    public string? CommunityTemplate { get; set; }

    /// <summary>Gets or sets the history command (list, show, rerun, delete, clear)</summary>
    public string? HistoryCommand { get; set; }

    /// <summary>Gets or sets the history entry ID or index</summary>
    public string? HistoryId { get; set; }

    /// <summary>Gets or sets the number of history entries to show</summary>
    public int HistoryLimit { get; set; } = 10;

    /// <summary>Gets or sets whether to clear all cache</summary>
    public bool ClearCache { get; set; }

    /// <summary>Gets or sets whether to show Umbraco versions table</summary>
    public bool ShowVersionsTable { get; set; }

    /// <summary>Gets or sets the output format (default, plain, json)</summary>
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Default;

    /// <summary>Gets or sets whether to output only the raw script text</summary>
    public bool ScriptOnly { get; set; }

    /// <summary>Gets or sets whether to suppress all interactive prompts</summary>
    public bool NonInteractive { get; set; }

    /// <summary>Gets or sets whether to validate inputs without generating a script</summary>
    public bool DryRun { get; set; }

    /// <summary>Gets or sets whether to show help as structured JSON</summary>
    public bool ShowHelpJson { get; set; }

    /// <summary>Gets or sets the list-options subcommand category (e.g., database-types, starter-kits, defaults)</summary>
    public string? ListOptionsCommand { get; set; }

    /// <summary>
    /// Checks if this is a list-options command
    /// </summary>
    public bool IsListOptionsCommand()
    {
        return ListOptionsCommand != null;
    }

    /// <summary>
    /// Checks if this is a template command
    /// </summary>
    public bool IsTemplateCommand()
    {
        return !string.IsNullOrWhiteSpace(TemplateCommand);
    }

    /// <summary>
    /// Checks if this is a history command
    /// </summary>
    public bool IsHistoryCommand()
    {
        return !string.IsNullOrWhiteSpace(HistoryCommand);
    }

    /// <summary>
    /// Checks if this is a versions command
    /// </summary>
    public bool IsVersionsCommand()
    {
        return ShowVersionsTable;
    }

    /// <summary>
    /// Checks if this is a community template command
    /// </summary>
    public bool IsCommunityTemplateCommand()
    {
        return !string.IsNullOrWhiteSpace(CommunityTemplate);
    }

    /// <summary>
    /// Checks if any configuration options are set (excluding help/version/default/verbose/template)
    /// </summary>
    public bool HasAnyOptions()
    {
        return UseDefault ||
               !string.IsNullOrWhiteSpace(Packages) ||
               !string.IsNullOrWhiteSpace(TemplatePackageName) ||
               !string.IsNullOrWhiteSpace(TemplateVersion) ||
               !string.IsNullOrWhiteSpace(ProjectName) ||
               CreateSolution.HasValue ||
               !string.IsNullOrWhiteSpace(SolutionName) ||
               IncludeStarterKit.HasValue ||
               !string.IsNullOrWhiteSpace(StarterKitPackage) ||
               !string.IsNullOrWhiteSpace(StarterKitVersion) ||
               IncludeDockerfile.HasValue ||
               IncludeDockerCompose.HasValue ||
               UseUnattended.HasValue ||
               !string.IsNullOrWhiteSpace(DatabaseType) ||
               !string.IsNullOrWhiteSpace(ConnectionString) ||
               !string.IsNullOrWhiteSpace(AdminName) ||
               !string.IsNullOrWhiteSpace(AdminEmail) ||
               !string.IsNullOrWhiteSpace(AdminPassword) ||
               OnelinerOutput.HasValue ||
               RemoveComments.HasValue ||
               IncludePrerelease.HasValue ||
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
                case "--template-package":
                    var templateArg = GetNextArgument(args, ref i);
                    if (!string.IsNullOrWhiteSpace(templateArg))
                    {
                        // --template-package flag: if contains pipe, split into name|version. Otherwise, treat as name only
                        if (templateArg.Contains('|'))
                        {
                            var parts = templateArg.Split('|', 2, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                options.TemplatePackageName = parts[0].Trim();
                                options.TemplateVersion = parts[1].Trim();
                            }
                            else
                            {
                                // Invalid format, just set the whole thing as template name
                                options.TemplatePackageName = templateArg;
                            }
                        }
                        else
                        {
                            // No pipe, treat as template package name only
                            options.TemplatePackageName = templateArg;
                        }
                    }
                    break;

                case "-n":
                case "--project-name":
                    options.ProjectName = GetNextArgument(args, ref i);
                    break;

                case "-s":
                case "--solution":
                    options.SolutionName = GetNextArgument(args, ref i);
                    options.CreateSolution = !string.IsNullOrWhiteSpace(options.SolutionName);
                    break;

                case "-k":
                case "--starter-kit":
                    var starterKitArg = GetNextArgument(args, ref i);
                    if (!string.IsNullOrWhiteSpace(starterKitArg))
                    {
                        // Check if version is specified with pipe character (e.g., "clean|7.0.3")
                        if (starterKitArg.Contains('|'))
                        {
                            var parts = starterKitArg.Split('|', 2, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                options.StarterKitPackage = parts[0].Trim();
                                options.StarterKitVersion = parts[1].Trim();
                            }
                            else
                            {
                                // Invalid format, just set the whole thing as starter kit package
                                options.StarterKitPackage = starterKitArg;
                            }
                        }
                        else
                        {
                            options.StarterKitPackage = starterKitArg;
                        }
                        options.IncludeStarterKit = true;
                    }
                    break;

                case "--dockerfile":
                    options.IncludeDockerfile = true;
                    break;

                case "--docker-compose":
                    options.IncludeDockerCompose = true;
                    break;

                case "-da":
                case "--delivery-api":
                    options.EnableContentDeliveryApi = true;
                    break;

                case "-u":
                case "--unattended-defaults":
                    options.UseUnattended = true;
                    options.DatabaseType = "SQLite";
                    options.AdminEmail = "admin@example.com";
                    options.AdminPassword = "1234567890";
                    break;

                case "--database-type":
                    options.DatabaseType = GetNextArgument(args, ref i);
                    options.UseUnattended = !string.IsNullOrWhiteSpace(options.DatabaseType);
                    break;

                case "--connection-string":
                    options.ConnectionString = GetNextArgument(args, ref i);
                    options.UseUnattended = !string.IsNullOrWhiteSpace(options.ConnectionString);
                    break;

                case "--admin-name":
                    options.AdminName = GetNextArgument(args, ref i);
                    options.UseUnattended = !string.IsNullOrWhiteSpace(options.AdminName);
                    break;

                case "--admin-email":
                    options.AdminEmail = GetNextArgument(args, ref i);
                    options.UseUnattended = !string.IsNullOrWhiteSpace(options.AdminEmail);
                    break;

                case "--admin-password":
                    options.AdminPassword = GetNextArgument(args, ref i);
                    options.UseUnattended = !string.IsNullOrWhiteSpace(options.AdminPassword);
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

                case "--verbose":
                    options.VerboseMode = true;
                    break;

                case "--template-version":
                    options.TemplateVersion = GetNextArgument(args, ref i);
                    break;

                case "--output":
                    var outputArg = GetNextArgument(args, ref i);
                    if (!string.IsNullOrWhiteSpace(outputArg))
                    {
                        options.OutputFormat = outputArg.ToLower() switch
                        {
                            "json" => OutputFormat.Json,
                            "plain" => OutputFormat.Plain,
                            _ => OutputFormat.Default
                        };
                    }
                    break;

                case "--script-only":
                    options.ScriptOnly = true;
                    break;

                case "--no-interaction":
                case "--non-interactive":
                    options.NonInteractive = true;
                    break;

                case "--dry-run":
                    options.DryRun = true;
                    break;

                case "--help-json":
                    options.ShowHelpJson = true;
                    break;

                // Template commands
                case "template":
                    // Next argument should be the subcommand (save, load, list, etc.)
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        i++;
                        options.TemplateCommand = args[i];

                        // Get template name if next argument is not a flag
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        {
                            i++;
                            options.TemplateName = args[i];
                        }
                    }
                    else
                    {
                        // No subcommand provided - set to empty string to trigger help display
                        options.TemplateCommand = "";
                    }
                    break;

                case "--template-name":
                    options.TemplateName = GetNextArgument(args, ref i);
                    break;

                case "--template-description":
                    options.TemplateDescription = GetNextArgument(args, ref i);
                    break;

                case "--template-file":
                    options.TemplateFile = GetNextArgument(args, ref i);
                    break;

                case "--template-tags":
                    var tags = GetNextArgument(args, ref i);
                    if (!string.IsNullOrWhiteSpace(tags))
                    {
                        options.TemplateTags = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList();
                    }
                    break;

                case "--community-template":
                    options.CommunityTemplate = GetNextArgument(args, ref i);
                    break;

                // History commands
                case "history":
                    // Next argument should be the subcommand (list, show, rerun, etc.)
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        i++;
                        options.HistoryCommand = args[i];

                        // Get history ID/index if next argument is not a flag
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        {
                            i++;
                            options.HistoryId = args[i];
                        }
                    }
                    else
                    {
                        // No subcommand provided - set to empty string to trigger help display
                        options.HistoryCommand = "";
                    }
                    break;

                case "--history-limit":
                    var limitStr = GetNextArgument(args, ref i);
                    if (!string.IsNullOrWhiteSpace(limitStr) && int.TryParse(limitStr, out int limit))
                    {
                        options.HistoryLimit = limit;
                    }
                    break;

                case "--clear-cache":
                    options.ClearCache = true;
                    break;

                case "versions":
                    options.ShowVersionsTable = true;
                    break;

                case "list-options":
                    // Get optional category argument
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        i++;
                        options.ListOptionsCommand = args[i].ToLower();
                    }
                    else
                    {
                        // No category - list all
                        options.ListOptionsCommand = "";
                    }
                    break;

                default:
                    // Check if this is a template or history subcommand without explicit prefix
                    if (!arg.StartsWith("-") && i == 0)
                    {
                        var templateCommands = new[] { "save", "load", "export", "import", "validate" };
                        var historyCommands = new[] { "list", "delete", "rerun", "clear" };

                        // Check if it's a template command (some overlap, so check context)
                        if (templateCommands.Contains(arg.ToLower()))
                        {
                            options.TemplateCommand = arg.ToLower();

                            // Get template name if next argument is not a flag
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                i++;
                                options.TemplateName = args[i];
                            }
                        }
                    }
                    // Unknown argument - ignore or warn
                    else if (arg.StartsWith("-"))
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
