using Microsoft.AspNetCore.Mvc;

using PSW.Shared.Models;

namespace PSW.Components;

[ViewComponent(Name = "TabNavigation")]
public class TabNavigationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(bool hasQueryString, int numberOfPackages)
    {
        var model = new TabNavigationViewModel(hasQueryString, numberOfPackages);
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml", model);
    }
}