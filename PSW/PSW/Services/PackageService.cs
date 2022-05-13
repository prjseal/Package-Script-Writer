using PSW.Models;
using System.Net;
using System.Xml.Serialization;
using static PSW.Models.PackageFeed;

namespace PSW.Services
{
    public class PackageService : IPackageService
    {
        public List<string> GetPackageVersions(string packageUrl)
        {
            List<string> allVersions = new List<string>();

            var url = $"{packageUrl}/atom.xml";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Accept = "application/xml";

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(NugetPackageVersionFeed.feed));
                var baseStream = streamReader.BaseStream;
                if (baseStream == null) return allVersions;

                var packageFeed = (NugetPackageVersionFeed.feed)serializer.Deserialize(baseStream);
                if (packageFeed != null)
                {
                    foreach (var entry in packageFeed.entryField)
                    {
                        var parts = entry.id.Split('/');
                        var partCount = parts.Length;
                        var versionNumber = parts[partCount - 1];
                        allVersions.Add(versionNumber);
                    }
                }
            }
            return allVersions;
        }

        public List<PagedPackagesPackage> GetAllPackagesFromUmbraco()
        {

            int pageIndex = 0;
            var pageSize = 1;
            var carryOn = true;
            List<PagedPackagesPackage> allPackages = new List<PagedPackagesPackage>();

            while (carryOn)
            {
                var url = $"https://our.umbraco.com/webapi/packages/v1?pageIndex={pageIndex}&pageSize={pageSize}&category=&query=&order=Latest&version=9.5.0";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Accept = "application/xml";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PagedPackages));
                    var baseStream = streamReader.BaseStream;
                    if (baseStream == null)
                    {
                        carryOn = false;
                        break;
                    }
                    try
                    {
                        var packageFeed = (PagedPackages)serializer.Deserialize(baseStream);
                        if (packageFeed?.Packages != null)
                        {
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
                }
                pageIndex++;
            }
            return allPackages;
        }
    }
}
