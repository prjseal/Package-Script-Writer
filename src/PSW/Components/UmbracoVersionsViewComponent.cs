using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using PSW.Shared.Configuration;
using PSW.Shared.Models;

namespace PSW.Components;

[ViewComponent(Name = "UmbracoVersions")]
public class UmbracoVersionsViewComponent : ViewComponent
{
    private readonly PSWConfig _pswConfig;

    public UmbracoVersionsViewComponent(IOptions<PSWConfig> pswConfig)
    {
        _pswConfig = pswConfig.Value;
    }

    public IViewComponentResult Invoke()
    {
        var model = new UmbracoVersionsViewModel(_pswConfig.UmbracoVersions);
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml", model);
    }
}