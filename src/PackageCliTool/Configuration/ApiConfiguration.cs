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
