using System.Text.Json.Serialization;

namespace PackageCliTool.Models;

/// <summary>
/// Represents a package from the Umbraco marketplace
/// </summary>
public class PagedPackagesPackage
{
    public string authors { get; set; }
    public string Description { get; set; }
    public object iconDominatorColor { get; set; }
    public string iconUrl { get; set; }
    public string FullIconUrl { get; set; }
    public string id { get; set; }
    public string PackageUrl { get; set; }
    public bool isHQ { get; set; }
    public bool isHQSupported { get; set; }
    public bool isPromoted { get; set; }
    public bool isPartner { get; set; }
    public string latestVersionNumber { get; set; }
    public string[] licenseTypes { get; set; }
    public string minimumUmbracoVersionNumber { get; set; }
    public int NumberOfNuGetDownloads { get; set; }
    public string PackageId { get; set; }
    public string[] tags { get; set; }
    public string Title { get; set; }
    public int[] umbracoMajorVersionsSupported { get; set; }
    public object packageVersions { get; set; }
    public object selectedVersion { get; set; }
    public Category category { get; set; }
}
