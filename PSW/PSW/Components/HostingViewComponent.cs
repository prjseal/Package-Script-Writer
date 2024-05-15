using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "Hosting")]
public class HostingViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml");
    }
}