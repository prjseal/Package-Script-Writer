using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "SourceCode")]
public class SourceCodeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml");
    }
}