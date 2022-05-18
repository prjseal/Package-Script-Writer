using Microsoft.AspNetCore.Mvc;
using PSW.Models;

namespace PSW.Components;

[ViewComponent(Name = "Packages")]
public class PackagesViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(PackagesViewModel viewModel)
    {
        return View(viewModel);
    }
}