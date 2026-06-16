using Microsoft.AspNetCore.Mvc;

namespace MalikongkongNHS.Controllers;

public class TeacherController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}