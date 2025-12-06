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
    public bool VerboseMode { get; set; }

    /// <summary>
    /// Checks if any configuration options are set (excluding help/version/default/verbose)
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

                case "--verbose":
                    options.VerboseMode = true;
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
