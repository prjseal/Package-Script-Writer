using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

using PSW.Shared.Models.NuGet;
using PSW.Shared.Models;
using static PSW.Shared.Enums.GlobalEnums;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;

namespace PSW.Shared.Services;

public class MarketplacePackageService : IPackageService
{
    private readonly IHttpClientFactory _clientFactory;

    public MarketplacePackageService(IHttpClientFactory httpClientFactory)
    {
        _clientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public async Task<List<string>> GetNugetPackageVersionsAsync(string packageUrl)
    {
        try
        {
            var client = _clientFactory.CreateClient();
            var result = await client.GetAsync(packageUrl);
            if (result.IsSuccessStatusCode)
            {
                var data = await result.Content.ReadAsStringAsync();
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

    public async Task<List<string>> GetPackageVersionsAsync(string packageUrl)
    {
        var allVersions = new List<string>();

        var url = $"{packageUrl}/atom.xml";

        // Use HttpClient to fetch XML asynchronously
        var client = _clientFactory.CreateClient();
        var xmlContent = await client.GetStringAsync(url);

        using var stringReader = new StringReader(xmlContent);
        using var xmlReader = new XmlTextReader(stringReader);

        var serializer = new XmlSerializer(typeof(NugetPackageVersionFeed.feed));

        if (serializer.Deserialize(xmlReader) is NugetPackageVersionFeed.feed packageFeed)
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

    public async Task<List<PagedPackagesPackage>> GetAllPackagesFromUmbracoAsync()
    {
        return await GetFromUmbracoMarketplaceAsync(UmbracoMarketplaceQueryType.Packages);
    }

    public async Task<List<PagedPackagesPackage>> GetAllTemplatesFromUmbracoAsync()
    {
        return await GetFromUmbracoMarketplaceAsync(UmbracoMarketplaceQueryType.Templates);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public async Task<List<PagedPackagesPackage>> GetFromUmbracoMarketplaceAsync(UmbracoMarketplaceQueryType umbracoMarketplaceQueryType)
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
                var httpClient = _clientFactory.CreateClient();
                var response = await httpClient.GetAsync(url);
                var packages = await response.Content.ReadFromJsonAsync<UmbracoMarketplaceResponse>();
                if (packages != null && carryOn)
                {

                    if (pageIndex == 1)
                    {
                        total = packages.TotalResults;
                    }

                    allPackages.AddRange(packages.Results.Where(x => x != null));
                    totalSoFar = allPackages.Count;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public async Task<List<NuGetSearchResult>> SearchNuGetPackagesAsync(string searchTerm, int take = 20)
    {
        try
        {
            // URL encode the search term
            var encodedSearchTerm = HttpUtility.UrlEncode(searchTerm);

            // NuGet.org search API endpoint
            var url = $"https://azuresearch-usnc.nuget.org/query?q={encodedSearchTerm}&take={take}&prerelease=false";

            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<NuGetSearchResult>();
            }

            var searchResponse = await response.Content.ReadFromJsonAsync<NuGetSearchResponse>();

            if (searchResponse?.Data != null)
            {
                return searchResponse.Data;
            }

            return new List<NuGetSearchResult>();
        }
        catch (Exception)
        {
            return new List<NuGetSearchResult>();
        }
    }
}