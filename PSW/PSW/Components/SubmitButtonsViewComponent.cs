using Microsoft.AspNetCore.Mvc;

namespace PSW.Components;

[ViewComponent(Name = "SubmitButtons")]
public class SubmitButtonsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}