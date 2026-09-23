using System.Text.Json.Serialization;

namespace PSW.Shared.Models.NuGet;

/// <summary>
/// Subset of the NuGet registration API response (https://learn.microsoft.com/nuget/api/registration-base-url-resource).
/// Packages with many versions return pages without inlined items; those pages are fetched via their @id.
/// </summary>
public class NuGetRegistrationIndex
{
    [JsonPropertyName("items")]
    public List<NuGetRegistrationPage> Items { get; set; } = new();
}

public class NuGetRegistrationPage
{
    [JsonPropertyName("@id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("items")]
    public List<NuGetRegistrationLeaf>? Items { get; set; }
}

public class NuGetRegistrationLeaf
{
    [JsonPropertyName("catalogEntry")]
    public NuGetCatalogEntry CatalogEntry { get; set; } = new();
}

public class NuGetCatalogEntry
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("listed")]
    public bool Listed { get; set; } = true;

    [JsonPropertyName("dependencyGroups")]
    public List<NuGetDependencyGroup>? DependencyGroups { get; set; }
}

public class NuGetDependencyGroup
{
    [JsonPropertyName("dependencies")]
    public List<NuGetDependency>? Dependencies { get; set; }
}

public class NuGetDependency
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("range")]
    public string? Range { get; set; }
}
