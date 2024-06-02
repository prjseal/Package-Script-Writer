using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "PopularScripts")]
public class PopularScriptsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml");
    }
}