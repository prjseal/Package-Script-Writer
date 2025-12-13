using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PSW.Shared.Models;

public class UmbracoMarketplaceResponse
{
    public PagedPackagesPackage[] Results { get; set; } = Array.Empty<PagedPackagesPackage>();
    public int TotalResults { get; set; }
}


public class PagedPackagesPackage
{
    public string Authors { get; set; } = string.Empty;
    [JsonPropertyName("Description")]
    public string Summary { get; set; } = string.Empty;
    public string IconDominatorColor { get; set; } = string.Empty;
#pragma warning disable CA1056 // URI-like properties should not be strings
    public string IconUrl { get; set; } = string.Empty;
#pragma warning restore CA1056 // URI-like properties should not be strings
    [JsonPropertyName("FullIconUrl")]
    public string Image => $"https://marketplace.umbraco.com/{IconUrl}";
    public Guid Id { get; set; }
    [JsonPropertyName("PackageUrl")]
#pragma warning disable CA1056 // URI-like properties should not be strings
    public string Url => $"https://marketplace.umbraco.com/package/{PackageId.ToLower()}";
#pragma warning restore CA1056 // URI-like properties should not be strings
    public bool IsHQ { get; set; }
    public bool IsHQSupported { get; set; }
    public bool IsPromoted { get; set; }
    public bool IsPartner { get; set; }
    public string LatestVersionNumber { get; set; } = string.Empty;
    public string[] LicenseTypes { get; set; } = Array.Empty<string>();
    public string MinimumUmbracoVersionNumber { get; set; } = string.Empty;
    [JsonPropertyName("NumberOfNuGetDownloads")]
    public int Downloads { get; set; }
    [JsonPropertyName("PackageId")]
    public string PackageId { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;
    public int[] UmbracoMajorVersionsSupported { get; set; } = Array.Empty<int>();
    public List<string> PackageVersions { get; set; } = new();
    [Display(Name = "Version")]
    public string SelectedVersion { get; set; } = string.Empty;
    public Category Category { get; set; } = new();
}

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}