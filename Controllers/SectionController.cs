using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers;

public class SectionController : Controller
{
    private readonly ISectionService      _sectionService;
    private readonly IGradeLevelService   _gradeLevelService;
    private readonly ISubjectService      _subjectService;
    private readonly ITeacherService      _teacherService;
    private readonly ApplicationDbContext _context;
    private readonly IAuditService        _audit;

    public SectionController(
        ISectionService      sectionService,
        IGradeLevelService   gradeLevelService,
        ISubjectService      subjectService,
        ITeacherService      teacherService,
        ApplicationDbContext context,
        IAuditService        audit)
    {
        _sectionService    = sectionService;
        _gradeLevelService = gradeLevelService;
        _subjectService    = subjectService;
        _teacherService    = teacherService;
        _context           = context;
        _audit             = audit;
    }

    private string Who  => HttpContext.Session.GetString("Username") ?? "Unknown";
    private string Role => HttpContext.Session.GetString("Role")     ?? "Unknown";
    private string IP   => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

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
        _audit.Log(Who, Role, "Create", "Section",
            $"Added section: {section.SectionName}", IP);
        TempData["Success"] = "Section added successfully!";
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
        _audit.Log(Who, Role, "Update", "Section",
            $"Updated section: {section.SectionName} (ID: {section.SectionId})", IP);
        TempData["Success"] = "Section updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var section = _context.Sections.Find(id);
        _sectionService.Delete(id);
        _audit.Log(Who, Role, "Delete", "Section",
            $"Deleted section ID: {id}" + (section != null ? $" ({section.SectionName})" : ""), IP);
        TempData["Success"] = "Section deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

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

           // FIXED
_audit.Log(Who, Role, "Create", "Section",
    $"Assigned subject ID {subjectId} to section ID {sectionId} — Room: {roomNumber}, Time: {time}", IP);

            TempData["Success"] = "Subject assigned successfully.";
        }
        else
        {
            TempData["Warning"] = "This subject is already assigned to this section.";
        }

        return RedirectToAction(nameof(Edit), new { id = sectionId });
    }

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

            var teacher = _teacherService.GetById(teacherId);
            _audit.Log(Who, Role, "Update", "Section",
                $"Updated subject assignment ID {sectionSubjectId} in section ID {sectionId} — Teacher: {teacher?.FullName}, Room: {roomNumber}, Time: {time}", IP);

            TempData["Success"] = "Subject updated successfully.";
        }
        return RedirectToAction(nameof(Edit), new { id = sectionId });
    }

    [HttpPost]
    public IActionResult RemoveSubject(int sectionSubjectId, int sectionId)
    {
        var record = _context.SectionSubjects.Find(sectionSubjectId);
        if (record != null)
        {
            _context.SectionSubjects.Remove(record);
            _context.SaveChanges();
            _audit.Log(Who, Role, "Delete", "Section",
                $"Removed subject assignment ID {sectionSubjectId} from section ID {sectionId}", IP);

            TempData["Success"] = "Subject removed.";
        }
        return RedirectToAction(nameof(Edit), new { id = sectionId });
    }
}