using Microsoft.AspNetCore.Mvc;

using PSW.Models;

namespace PSW.Components;

[ViewComponent(Name = "Options")]
public class OptionsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(PackagesViewModel viewModel)
    {
        return View($"{ViewComponentContext.ViewComponentDescriptor.FullName}.cshtml", viewModel);
    }
}