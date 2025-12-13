using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Response model for package versions from the API
/// </summary>
public class PackageVersionResponse
{
    /// <summary>Gets or sets the list of available package versions</summary>
    [JsonPropertyName("versions")]
    public List<string> Versions { get; set; } = new();
}
