using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers;

public class SectionController : Controller
{
    private readonly ISectionService    _sectionService;
    private readonly IGradeLevelService _gradeLevelService;
    private readonly ISubjectService    _subjectService;
    private readonly ITeacherService    _teacherService;
    private readonly ApplicationDbContext _context;

    public SectionController(
        ISectionService    sectionService,
        IGradeLevelService gradeLevelService,
        ISubjectService    subjectService,
        ITeacherService    teacherService,
        ApplicationDbContext context)
    {
        _sectionService    = sectionService;
        _gradeLevelService = gradeLevelService;
        _subjectService    = subjectService;
        _teacherService    = teacherService;
        _context           = context;
    }

    public IActionResult Index()
    {
        ViewBag.GradeLevels = _gradeLevelService.GetAll();
        ViewBag.Subjects    = _subjectService.GetAll();
        ViewBag.Teachers    = _teacherService.GetAllTeachers();
        return View(_sectionService.GetAll());
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.GradeLevels = _gradeLevelService.GetAll();
        ViewBag.Teachers    = _teacherService.GetAllTeachers();
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
        var section = _context.Sections
            .Include(s => s.SectionSubjects)
                .ThenInclude(ss => ss.Subject)
            .Include(s => s.SectionSubjects)
                .ThenInclude(ss => ss.Teacher)
            .FirstOrDefault(s => s.SectionId == id);

        if (section == null) return NotFound();

        ViewBag.GradeLevels = _gradeLevelService.GetAll();
        ViewBag.Teachers    = _teacherService.GetAllTeachers();
        ViewBag.Subjects    = _subjectService.GetAll();
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

    // ── ASSIGN SUBJECT ───────────────────────────────────────────
    [HttpPost]
    public IActionResult AssignSubject(int sectionId, int subjectId, int teacherId,
                                       string? time, string? roomNumber)
    {
        bool exists = _context.SectionSubjects.Any(ss =>
            ss.SectionId == sectionId && ss.SubjectId == subjectId);

        if (!exists)
        {
            _context.SectionSubjects.Add(new SectionSubject
            {
                SectionId  = sectionId,
                SubjectId  = subjectId,
                TeacherId  = teacherId,
                Time       = time,
                RoomNumber = roomNumber
            });
            _context.SaveChanges();
            TempData["Success"] = "Subject assigned successfully.";
        }
        else
        {
            TempData["Warning"] = "This subject is already assigned to this section.";
        }

        return RedirectToAction(nameof(Edit), new { id = sectionId });
    }

    // ── EDIT SUBJECT ─────────────────────────────────────────────
    [HttpPost]
    public IActionResult EditSubject(int sectionSubjectId, int sectionId, int teacherId,
                                      string? time, string? roomNumber)
    {
        var record = _context.SectionSubjects.Find(sectionSubjectId);
        if (record != null)
        {
            record.TeacherId  = teacherId;
            record.Time       = time;
            record.RoomNumber = roomNumber;
            _context.SaveChanges();
            TempData["Success"] = "Subject updated successfully.";
        }
        return RedirectToAction(nameof(Edit), new { id = sectionId });
    }

    // ── REMOVE SUBJECT ───────────────────────────────────────────
    [HttpPost]
    public IActionResult RemoveSubject(int sectionSubjectId, int sectionId)
    {
        var record = _context.SectionSubjects.Find(sectionSubjectId);
        if (record != null)
        {
            _context.SectionSubjects.Remove(record);
            _context.SaveChanges();
            TempData["Success"] = "Subject removed.";
        }
        return RedirectToAction(nameof(Edit), new { id = sectionId });
    }
}