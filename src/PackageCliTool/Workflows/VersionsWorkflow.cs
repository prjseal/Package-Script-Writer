using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageCliTool.Models;
using PackageCliTool.UI;
using PSW.Shared.Configuration;
using Spectre.Console;

namespace PackageCliTool.Workflows;

/// <summary>
/// Orchestrates versions-related workflows
/// </summary>
public class VersionsWorkflow
{
    private readonly PSWConfig _pswConfig;
    private readonly ILogger? _logger;

    public VersionsWorkflow(
        IOptions<PSWConfig> pswConfig,
        ILogger? logger = null)
    {
        _pswConfig = pswConfig.Value;
        _logger = logger;
    }

    /// <summary>
    /// Runs the versions command based on options
    /// </summary>
    public Task RunAsync(CommandLineOptions options)
    {
        _logger?.LogInformation("Displaying Umbraco versions table");

        ConsoleDisplay.DisplayUmbracoVersions(_pswConfig);

        return Task.CompletedTask;
    }
}
