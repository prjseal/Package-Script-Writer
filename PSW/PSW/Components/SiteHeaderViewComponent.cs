using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "SiteHeader")]
public class SiteHeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml");
    }
}