using Microsoft.AspNetCore.Mvc;
using PSW.Models;

namespace PSW.Components;

[ViewComponent(Name = "TabNavigation")]
public class TabNavigationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(bool hasQueryString, int numberOfPackages)
    {
        var model = new TabNavigationViewModel(hasQueryString, numberOfPackages);
        return View(model);
    }
}