using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PackageCliTool.Validation;

/// <summary>
/// Validates script commands against an allowlist to prevent execution of dangerous commands
/// </summary>
public class CommandValidator
{
    private readonly ILogger? _logger;
    private readonly List<Regex> _allowedPatterns;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandValidator"/> class
    /// </summary>
    /// <param name="logger">Optional logger instance</param>
    public CommandValidator(ILogger? logger = null)
    {
        _logger = logger;
        _allowedPatterns = InitializeAllowedPatterns();
    }

    /// <summary>
    /// Validates that all commands in the script are allowed
    /// </summary>
    /// <param name="scriptContent">The script content to validate</param>
    /// <param name="isWindows">Whether the script is for Windows (affects syntax)</param>
    /// <returns>A tuple indicating if validation passed and any error messages</returns>
    public (bool IsValid, List<string> Errors) ValidateScript(string scriptContent, bool isWindows)
    {
        var errors = new List<string>();
        var lines = scriptContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            // Handle command chaining with && (one-liner format)
            // Example: dotnet new install ... && dotnet new umbraco ... && dotnet run
            if (line.Contains("&&"))
            {
                var chainedCommands = line.Split(new[] { "&&" }, StringSplitOptions.None);
                _logger?.LogDebug("Line {LineNumber} contains {Count} chained commands", lineNumber, chainedCommands.Length);

                foreach (var chainedCommand in chainedCommands)
                {
                    var trimmedCommand = chainedCommand.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedCommand))
                    {
                        continue;
                    }

                    if (!IsCommandAllowed(trimmedCommand, isWindows))
                    {
                        var error = $"Line {lineNumber}: Command not allowed in chain: '{trimmedCommand}'";
                        errors.Add(error);
                        _logger?.LogWarning("Blocked dangerous command in chain at line {LineNumber}: {Command}", lineNumber, trimmedCommand);
                    }
                }
            }
            else
            {
                // Single command validation
                if (!IsCommandAllowed(line, isWindows))
                {
                    var error = $"Line {lineNumber}: Command not allowed: '{line}'";
                    errors.Add(error);
                    _logger?.LogWarning("Blocked dangerous command at line {LineNumber}: {Command}", lineNumber, line);
                }
            }
        }

        var isValid = errors.Count == 0;
        if (isValid)
        {
            _logger?.LogInformation("Script validation passed - all commands are allowed");
        }
        else
        {
            _logger?.LogWarning("Script validation failed with {ErrorCount} errors", errors.Count);
        }

        return (isValid, errors);
    }

    /// <summary>
    /// Checks if a single command matches any allowed pattern
    /// </summary>
    private bool IsCommandAllowed(string command, bool isWindows)
    {
        // Windows-specific commands
        if (isWindows)
        {
            // @echo off command
            if (command.Equals("@echo off", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // PowerShell environment variables: $env:VARIABLE=value
            if (Regex.IsMatch(command, @"^\$env:\w+\s*=", RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        // Check against all allowed patterns
        foreach (var pattern in _allowedPatterns)
        {
            if (pattern.IsMatch(command))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Initializes the list of allowed command patterns
    /// </summary>
    private List<Regex> InitializeAllowedPatterns()
    {
        var patterns = new List<Regex>();

        // dotnet new install (template installation)
        // Examples: dotnet new install Umbraco.Templates::14.3.0 --force
        patterns.Add(new Regex(@"^dotnet\s+new\s+install\s+[\w\.\-:]+(\s+(--force|--interactive))*\s*$", RegexOptions.IgnoreCase));

        // dotnet new -i (legacy template installation)
        // Examples: dotnet new -i Umbraco.Templates::10.0.0
        patterns.Add(new Regex(@"^dotnet\s+new\s+-i\s+[\w\.\-:]+\s*$", RegexOptions.IgnoreCase));

        // dotnet new sln (solution creation)
        // Examples: dotnet new sln --name "MySolution"
        patterns.Add(new Regex(@"^dotnet\s+new\s+sln(\s+--name\s+""[^""]+"")*\s*$", RegexOptions.IgnoreCase));

        // dotnet new <template> (project creation from templates)
        // Examples:
        //   dotnet new umbraco --force -n "MyProject"
        //   dotnet new umbraco --force -n "MyProject" --add-docker --friendly-name "Admin" --email "admin@example.com" --password "Pass123!" --development-database-type SQLite
        //   dotnet new umbraco-compose -P "MyProject"
        // Pattern allows any combination of flags (with or without values)
        patterns.Add(new Regex(@"^dotnet\s+new\s+[\w\-]+(\s+(--[\w\-]+|-[a-zA-Z])(\s+(""[^""]+""|\S+))?)*\s*$", RegexOptions.IgnoreCase));

        // dotnet sln add (add project to solution)
        // Examples: dotnet sln add "MyProject"
        patterns.Add(new Regex(@"^dotnet\s+sln\s+add\s+(""[^""]+""|\S+)\s*$", RegexOptions.IgnoreCase));

        // dotnet add package (add NuGet package)
        // Examples:
        //   dotnet add package Umbraco.Community.BlockPreview --version 1.6.0
        //   dotnet add "MyProject" package uSync --prerelease
        patterns.Add(new Regex(@"^dotnet\s+add(\s+(""[^""]+""|\S+))?\s+package\s+[\w\.\-]+(\s+(--version|--prerelease)\s+[\w\.\-]+)*\s*$", RegexOptions.IgnoreCase));

        // dotnet run (run the project)
        // Examples:
        //   dotnet run
        //   dotnet run --project "MyProject"
        //   dotnet run --urls "http://localhost:5000"
        patterns.Add(new Regex(@"^dotnet\s+run(\s+(--project|--urls)\s+(""[^""]+""|\S+))*\s*$", RegexOptions.IgnoreCase));

        // dotnet build (build the project - might be needed)
        patterns.Add(new Regex(@"^dotnet\s+build(\s+(""[^""]+""|\S+))*\s*$", RegexOptions.IgnoreCase));

        // dotnet restore (restore NuGet packages - might be needed)
        patterns.Add(new Regex(@"^dotnet\s+restore(\s+(""[^""]+""|\S+))*\s*$", RegexOptions.IgnoreCase));

        _logger?.LogDebug("Initialized {PatternCount} allowed command patterns", patterns.Count);

        return patterns;
    }
}
