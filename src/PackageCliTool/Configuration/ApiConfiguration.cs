namespace PackageCliTool.Configuration;

/// <summary>
/// Configuration constants for the Package Script Writer API
/// </summary>
public static class ApiConfiguration
{
    /// <summary>
    /// Base URL for the Package Script Writer API
    /// </summary>
    public const string ApiBaseUrl = "https://psw.codeshare.co.uk";

    /// <summary>
    /// Endpoint for fetching package versions
    /// </summary>
    public const string GetVersionsEndpoint = "/api/scriptgeneratorapi/getpackageversions";

    /// <summary>
    /// Endpoint for generating installation scripts
    /// </summary>
    public const string GenerateScriptEndpoint = "/api/scriptgeneratorapi/generatescript";

    /// <summary>
    /// Endpoint for fetching all available packages
    /// </summary>
    public const string GetAllPackagesEndpoint = "/api/scriptgeneratorapi/getallpackages";

    /// <summary>
    /// Current version of the CLI tool
    /// </summary>
    public const string Version = "1.0.0-beta";

    /// <summary>
    /// Popular Umbraco packages for quick selection
    /// </summary>
    public static readonly List<string> PopularPackages = new()
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
}
