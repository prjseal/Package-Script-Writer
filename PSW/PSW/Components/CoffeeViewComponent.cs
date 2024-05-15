using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "Coffee")]
public class CoffeeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}