using Spectre.Console;

namespace PackageCliTool.Models;

/// <summary>
/// Command-line options parser and container
/// </summary>
public class CommandLineOptions
{
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }
    public bool UseDefault { get; set; }
    public string? Packages { get; set; }
    public string? TemplatePackageName { get; set; }
    public string? TemplateVersion { get; set; }
    public string? ProjectName { get; set; }
    public bool? CreateSolution { get; set; }
    public string? SolutionName { get; set; }
    public bool? IncludeStarterKit { get; set; }
    public string? StarterKitPackage { get; set; }
    public string? StarterKitVersion { get; set; }
    public bool? IncludeDockerfile { get; set; }
    public bool? IncludeDockerCompose { get; set; }
    public bool? UseUnattended { get; set; }
    public string? DatabaseType { get; set; }
    public string? ConnectionString { get; set; }
    public string? AdminName { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
    public bool? OnelinerOutput { get; set; }
    public bool? RemoveComments { get; set; }
    public bool? IncludePrerelease { get; set; }
    public bool AutoRun { get; set; }
    public string? RunDirectory { get; set; }
    public bool VerboseMode { get; set; }

    // Template-related options
    public string? TemplateCommand { get; set; }  // save, load, list, show, delete, export, import
    public string? TemplateName { get; set; }
    public string? TemplateDescription { get; set; }
    public string? TemplateFile { get; set; }
    public List<string> TemplateTags { get; set; } = new();
    public Dictionary<string, string> TemplateOverrides { get; set; } = new();

    // History-related options
    public string? HistoryCommand { get; set; }  // list, show, rerun, delete, clear, stats
    public string? HistoryId { get; set; }  // ID or index of history entry
    public int HistoryLimit { get; set; } = 10;  // Number of entries to show

    // Cache-related options
    public bool NoCache { get; set; }  // Disable caching
    public bool ClearCache { get; set; }  // Clear all cache
    public bool UpdatePackageCache { get; set; }  // Force update package cache from marketplace

    // Versions-related options
    public bool ShowVersionsTable { get; set; }  // Show Umbraco versions table

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
                    var tArg = GetNextArgument(args, ref i);
                    if (!string.IsNullOrWhiteSpace(tArg))
                    {
                        // -t flag: if contains pipe, split into name|version. Otherwise, treat as version only
                        if (tArg.Contains('|'))
                        {
                            var parts = tArg.Split('|', 2, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                options.TemplatePackageName = parts[0].Trim();
                                options.TemplateVersion = parts[1].Trim();
                            }
                            else
                            {
                                // Invalid format, just set the whole thing as template name
                                options.TemplatePackageName = tArg;
                            }
                        }
                        else
                        {
                            // No pipe, treat as template version only
                            options.TemplateVersion = tArg;
                        }
                    }
                    break;

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

                case "--set":
                    // Format: --set key=value
                    var setValue = GetNextArgument(args, ref i);
                    if (!string.IsNullOrWhiteSpace(setValue) && setValue.Contains('='))
                    {
                        var parts = setValue.Split('=', 2);
                        options.TemplateOverrides[parts[0].Trim()] = parts[1].Trim();
                    }
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

                case "--no-cache":
                    options.NoCache = true;
                    break;

                case "--clear-cache":
                    options.ClearCache = true;
                    break;

                case "--update-packages":
                    options.UpdatePackageCache = true;
                    break;

                case "versions":
                    options.ShowVersionsTable = true;
                    break;

                default:
                    // Check if this is a template or history subcommand without explicit prefix
                    if (!arg.StartsWith("-") && i == 0)
                    {
                        var templateCommands = new[] { "save", "load", "export", "import", "validate" };
                        var historyCommands = new[] { "list", "show", "delete", "rerun", "clear", "stats" };

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
