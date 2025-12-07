using System.Text.Json.Serialization;

namespace PackageCliTool.Models;

/// <summary>
/// Represents a package from the Umbraco marketplace
/// </summary>
public class PagedPackagesPackage
{
    [JsonPropertyName("category")]
    public Category Category { get; set; }

    [JsonPropertyName("certifiedToWorkOnUmbracoCloud")]
    public bool CertifiedToWorkOnUmbracoCloud { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }

    [JsonPropertyName("excerpt")]
    public string Excerpt { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("isNuGetFormat")]
    public bool IsNuGetFormat { get; set; }

    [JsonPropertyName("isPromoted")]
    public bool IsPromoted { get; set; }

    [JsonPropertyName("latestVersion")]
    public string LatestVersion { get; set; } = string.Empty;

    [JsonPropertyName("likes")]
    public byte Likes { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nuGetPackageId")]
    public string NuGetPackageId { get; set; } = string.Empty;

    [JsonPropertyName("ownerInfo")]
    public PagedPackagesPackageOwnerInfo? OwnerInfo { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("versionRange")]
    public string VersionRange { get; set; } = string.Empty;

    [JsonPropertyName("packageVersions")]
    public List<string>? PackageVersions { get; set; }

    [JsonPropertyName("selectedVersion")]
    public string? SelectedVersion { get; set; }
}

/// <summary>
/// Represents owner information for a package
/// </summary>
public class PagedPackagesPackageOwnerInfo
{
    [JsonPropertyName("contributors")]
    public string[] Contributors { get; set; } = Array.Empty<string>();

    [JsonPropertyName("karma")]
    public int Karma { get; set; }

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("ownerAvatar")]
    public string OwnerAvatar { get; set; } = string.Empty;
}
