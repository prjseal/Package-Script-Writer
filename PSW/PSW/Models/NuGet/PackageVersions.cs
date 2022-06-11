using System.Text.Json.Serialization;

namespace PSW.Models.NuGet;

public class PackageVersions
{
    [JsonPropertyName("versions")]
    public string[]? Versions { get; set; }
}