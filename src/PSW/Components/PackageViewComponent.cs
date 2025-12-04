using Microsoft.AspNetCore.Mvc;

using PSW.Models;

namespace PSW.Components;

[ViewComponent(Name = "Package")]
public class PackageViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(PagedPackagesPackage package, List<string> pickedPackageIds, int packageId)
    {
        var model = new PackageViewModel(package, pickedPackageIds, packageId);
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml", model);
    }
}