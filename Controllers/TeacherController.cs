using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Services.Interfaces;
using MalikongkongNHS.Models.Entities;

public class TeacherController : Controller
{
    private readonly ITeacherService _teacherService;
    private readonly ISubjectService _subjectService;

    public TeacherController(ITeacherService teacherService, ISubjectService subjectService)
    {
        _teacherService = teacherService;
        _subjectService = subjectService;
    }

    // =========================
    // LIST
    // =========================

    public IActionResult Index()
    {
        var teachers = _teacherService.GetAllTeachers();
        ViewBag.Subjects = _subjectService.GetAll();
        return View(teachers);
    }

    // =========================
    // CREATE
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Teacher teacher)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(Index));
        }

        _teacherService.Add(teacher);
        TempData["Success"] = "Teacher added successfully!";
        return RedirectToAction(nameof(Index));
    }

    // =========================
    // GET TEACHER (for Edit modal via AJAX)
    // =========================

    [HttpGet]
    public IActionResult GetTeacher(int id)
    {
        var teacher = _teacherService.GetById(id);
        if (teacher == null)
            return NotFound();

        return Json(new
        {
            teacherId = teacher.TeacherId,
            fullName = teacher.FullName,
            subjectId = teacher.SubjectId,
            email = teacher.Email
        });
    }

    // =========================
    // EDIT
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Teacher teacher)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(Index));
        }

        _teacherService.Update(teacher);
        TempData["Success"] = "Teacher updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // =========================
    // DELETE
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _teacherService.Delete(id);
        TempData["Success"] = "Teacher deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}