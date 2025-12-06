using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Request model for fetching package versions from the API
/// </summary>
public class PackageVersionRequest
{
    [JsonPropertyName("packageId")]
    public string PackageId { get; set; } = string.Empty;

    [JsonPropertyName("includePrerelease")]
    public bool IncludePrerelease { get; set; }
}
