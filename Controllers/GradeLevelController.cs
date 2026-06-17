using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MalikongkongNHS.Controllers;

public class GradeLevelController : Controller
{
    private readonly IGradeLevelService _gradeLevelService;

    public GradeLevelController(IGradeLevelService gradeLevelService)
    {
        _gradeLevelService = gradeLevelService;
    }

    [HttpPost]
    public IActionResult Create(string Name)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            _gradeLevelService.Add(new GradeLevel { Name = Name });
        }

        return RedirectToAction("Index", "Section");
    }
}