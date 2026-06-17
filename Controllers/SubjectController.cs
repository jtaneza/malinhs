using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MalikongkongNHS.Controllers;

public class SubjectController : Controller
{
    private readonly ISubjectService _subjectService;

    public SubjectController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpPost]
    public IActionResult Create(string Name)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            _subjectService.Add(new Subject { Name = Name });
        }

        return RedirectToAction("Index", "Section");
    }
}
