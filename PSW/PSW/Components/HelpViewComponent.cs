using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "Help")]
public class HelpViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}