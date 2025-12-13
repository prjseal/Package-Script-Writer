using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Request model for fetching package versions from the API
/// </summary>
public class PackageVersionRequest
{
    /// <summary>Gets or sets the NuGet package identifier</summary>
    [JsonPropertyName("packageId")]
    public string PackageId { get; set; } = string.Empty;

    /// <summary>Gets or sets whether to include prerelease versions</summary>
    [JsonPropertyName("includePrerelease")]
    public bool IncludePrerelease { get; set; }
}
