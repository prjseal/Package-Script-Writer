using System.Text.Json;
using PSW.Models;
using System.Xml;
using System.Xml.Serialization;
using PSW.Models.NuGet;
using static PSW.Models.PackageFeed;

namespace PSW.Services;

public class PackageService : IPackageService
{
    private readonly IHttpClientFactory clientFactory;

    public PackageService(IHttpClientFactory clientFactory)
    {
        this.clientFactory = clientFactory;
    }

    public List<string> GetNugetPackageVersions(string packageUrl)
    {
        var client = clientFactory.CreateClient();
        var result = client.GetAsync(packageUrl).Result;
        if (result.IsSuccessStatusCode)
        {
            var data = result.Content.ReadAsStringAsync().Result;
            var packageVersions = JsonSerializer.Deserialize<PackageVersions>(data);

            if (packageVersions is { Versions: { } })
            {
                return packageVersions.Versions.Reverse().ToList();
            }
        }

        return new List<string>();
    }

    public List<string> GetPackageVersions(string packageUrl)
    {
        var allVersions = new List<string>();

        var url = $"{packageUrl}/atom.xml";

        var XmlReader = new XmlTextReader(url);

        var serializer = new XmlSerializer(typeof(NugetPackageVersionFeed.feed));

        if (serializer.Deserialize(XmlReader) is NugetPackageVersionFeed.feed packageFeed)
        {
            foreach (var entry in packageFeed.entryField)
            {
                var parts = entry.id.Split('/');
                var partCount = parts.Length;
                var versionNumber = parts[partCount - 1];
                allVersions.Add(versionNumber);
            }
        }

        return allVersions;
    }

    public List<PagedPackagesPackage> GetAllPackagesFromUmbraco()
    {

        int pageIndex = 0;
        var pageSize = 200;
        var carryOn = true;
        var allPackages = new List<PagedPackagesPackage>();
        var total = 0;
        var totalSoFar = 0;
        while (carryOn && (pageIndex == 0 || totalSoFar < total))
        {
            totalSoFar = (pageIndex + 1) * pageSize;
            var url = $"https://our.umbraco.com/webapi/packages/v1?pageIndex={pageIndex}&pageSize={pageSize}&category=&query=&order=Latest&version=9.5.0";

            var XmlReader = new XmlTextReader(url);

            var serializer = new XmlSerializer(typeof(PagedPackages));

            try
            {
                PagedPackages? packageFeed = serializer.Deserialize(XmlReader) as PagedPackages;
                if (packageFeed?.Packages != null)
                {
                    if (pageIndex == 0)
                    {
                        total = packageFeed.Total;
                    }
                    allPackages.AddRange(packageFeed.Packages.Where(x => x != null));
                    carryOn = true;
                }
                else
                {
                    carryOn = false;
                }
            }
            catch
            {
                carryOn = false;
                break;
            }

            pageIndex++;
        }

        return allPackages;
    }
}