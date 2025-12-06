using Microsoft.AspNetCore.Mvc;

namespace PSW.Controllers;

public class UUIController : Controller
{
    public UUIController()
    {
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}