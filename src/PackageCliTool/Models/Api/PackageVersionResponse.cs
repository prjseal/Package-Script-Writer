using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Response model for package versions from the API
/// </summary>
public class PackageVersionResponse
{
    [JsonPropertyName("versions")]
    public List<string> Versions { get; set; } = new();
}
