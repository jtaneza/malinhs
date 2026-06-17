using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers;

public class SectionController : Controller
{
    private readonly ISectionService _sectionService;
    private readonly IGradeLevelService _gradeLevelService;
    private readonly ISubjectService _subjectService;
    private readonly ITeacherService _teacherService;

    public SectionController(
        ISectionService sectionService,
        IGradeLevelService gradeLevelService,
        ISubjectService subjectService,
        ITeacherService teacherService)
    {
        _sectionService = sectionService;
        _gradeLevelService = gradeLevelService;
        _subjectService = subjectService;
        _teacherService = teacherService;
    }

    public IActionResult Index()
    {
        ViewBag.GradeLevels = _gradeLevelService.GetAll();
        ViewBag.Subjects = _subjectService.GetAll();
        ViewBag.Teachers = _teacherService.GetAllTeachers();
        return View(_sectionService.GetAll());
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.GradeLevels = _gradeLevelService.GetAll();
        ViewBag.Teachers = _teacherService.GetAllTeachers();
        return View(new Section());
    }

    [HttpPost]
    public IActionResult Create(Section section)
    {
        _sectionService.Add(section);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var section = _sectionService.GetById(id);

        if (section == null)
            return NotFound();

        ViewBag.GradeLevels = _gradeLevelService.GetAll();
        ViewBag.Teachers = _teacherService.GetAllTeachers();
        return View(section);
    }

    [HttpPost]
    public IActionResult Edit(Section section)
    {
        _sectionService.Update(section);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        _sectionService.Delete(id);

        return RedirectToAction(nameof(Index));
    }
}