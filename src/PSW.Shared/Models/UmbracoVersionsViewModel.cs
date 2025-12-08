using PSW.Shared.Configuration;

namespace PSW.Shared.Models;

public class UmbracoVersionsViewModel
{
    public List<UmbracoVersion>? UmbracoVersions { get; set; }

    public UmbracoVersionsViewModel(List<UmbracoVersion>? umbracoVersions)
    {
        UmbracoVersions = umbracoVersions;
    }
}