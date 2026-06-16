using Microsoft.AspNetCore.Mvc;

namespace MalikongkongNHS.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}