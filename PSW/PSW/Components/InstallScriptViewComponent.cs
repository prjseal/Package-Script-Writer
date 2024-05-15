using Microsoft.AspNetCore.Mvc;
using PSW.Models;

namespace PSW.Components;

[ViewComponent(Name = "InstallScript")]
public class InstallScriptViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(bool hasQueryString, PackagesViewModel packagesViewModel)
    {
        var model = new InstallScriptViewModel(hasQueryString, packagesViewModel.OnelinerOutput, packagesViewModel.RemoveComments, packagesViewModel.Output);
        return View(model);
    }
}