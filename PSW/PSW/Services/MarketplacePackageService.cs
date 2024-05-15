using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

using PSW.Models;
using PSW.Models.NuGet;

using static PSW.Enums.GlobalEnums;

namespace PSW.Services;

public class MarketplacePackageService : IPackageService
{
    private readonly IHttpClientFactory clientFactory;

    public MarketplacePackageService(IHttpClientFactory httpClientFactory)
    {
        clientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    public List<string> GetNugetPackageVersions(string packageUrl)
    {
        try
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
        catch (Exception)
        {
            return new List<string>();
        }
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
        return GetFromUmbracoMarketplace(UmbracoMarketplaceQueryType.Packages);
    }

    public List<PagedPackagesPackage> GetAllTemplatesFromUmbraco()
    {
        return GetFromUmbracoMarketplace(UmbracoMarketplaceQueryType.Templates);
    }

    public List<PagedPackagesPackage> GetFromUmbracoMarketplace(UmbracoMarketplaceQueryType umbracoMarketplaceQueryType)
    {
        int pageIndex = 1;
        var pageSize = 50;
        var carryOn = true;
        var allPackages = new List<PagedPackagesPackage>();
        var total = 0;
        var totalSoFar = 0;
        var isTail = false;
        while (pageIndex == 1 || totalSoFar < total)
        {
            isTail = pageIndex > 1 && (pageSize > total - totalSoFar);
            if (isTail)
            {
                pageSize = total - totalSoFar;
            }

            string url = umbracoMarketplaceQueryType switch
            {
                UmbracoMarketplaceQueryType.Packages => $"https://api.marketplace.umbraco.com/api/v1.0/packages?orderBy=MostDownloads&fields=numberOfNuGetDownloads,authors,packageType,licenseTypes,iconUrl,minimumUmbracoVersionNumber,maximumUmbracoVersionNumber,isCertifiedToWorkOnUmbracoCloud,isPromoted,isPartner,isHQ,isHQSupported,id,title,description,packageId,tags,umbracoMajorVersionsSupported,iconDominantColor,Category&pageSize={pageSize}&pageNumber={pageIndex}",
                UmbracoMarketplaceQueryType.Templates => $"https://api.marketplace.umbraco.com/api/v1.0/packages?packageType=Template&categoryId=b239f1b7-31f6-4665-bf03-2ab985c64ac0&fields=title,packageId&pageSize={pageSize}&pageNumber={pageIndex}",
                _ => throw new Exception("Unable to match to a marketplace query type")
            };

            try
            {
                var httpClient = clientFactory.CreateClient();
                var response = httpClient.GetAsync(url).Result;
                var packages = response.Content.ReadFromJsonAsync<UmbracoMarketplaceResponse>().Result;
                if (packages != null && carryOn)
                {

                    if (pageIndex == 1)
                    {
                        total = packages.TotalResults;
                    }

                    allPackages.AddRange(packages.Results.Where(x => x != null));
                    totalSoFar = allPackages.Count();
                    if (isTail)
                    {
                        break;
                    }
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