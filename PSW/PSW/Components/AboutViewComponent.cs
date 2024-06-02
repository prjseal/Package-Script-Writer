using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "About")]
public class AboutViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml");
    }
}