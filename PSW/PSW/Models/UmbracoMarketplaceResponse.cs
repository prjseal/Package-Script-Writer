using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PSW.Models
{
    public class UmbracoMarketplaceResponse
    {
        public PagedPackagesPackage[] Results { get; set; }
        public int TotalResults { get; set; }
    }


    public class PagedPackagesPackage
    {
        public string Authors { get; set; }
        [JsonPropertyName("Description")]
        public string Summary { get; set; }
        public string IconDominatorColor { get; set; }
        public string IconUrl { get; set; }
        [JsonPropertyName("FullIconUrl")]
        public string Image => $"https://marketplace.umbraco.com/{IconUrl}";
        public Guid Id { get; set; }
        [JsonPropertyName("PackageUrl")]
        public string Url => $"https://marketplace.umbraco.com/package/{NuGetPackageId}";
        public bool IsHQ { get; set; }
        public bool IsHQSupported { get; set; }
        public bool IsPromoted { get; set; }
        public bool IsPartner { get; set; }
        public string LatestVersionNumber { get; set; }
        public string[] LicenseTypes { get; set; }
        public string MinimumUmbracoVersionNumber { get; set; }
        [JsonPropertyName("NumberOfNuGetDownloads")]
        public int Downloads { get; set; }
        [JsonPropertyName("PackageId")]
        public string NuGetPackageId { get; set; }
        public string[] Tags { get; set; }
        [JsonPropertyName("Title")]
        public string Name { get; set; }
        public int[] UmbracoMajorVersionsSupported { get; set; }
        public List<string> PackageVersions { get; set; }
        [Display(Name = "Version")]
        public string SelectedVersion { get; set; }
    }
}
