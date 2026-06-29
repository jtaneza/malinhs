using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Services.Interfaces;
using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly ISubjectService _subjectService;
        private readonly IAuditService   _audit;

        public TeacherController(ITeacherService teacherService, ISubjectService subjectService, IAuditService audit)
        {
            _teacherService = teacherService;
            _subjectService = subjectService;
            _audit          = audit;
        }

        private string Who  => HttpContext.Session.GetString("Username") ?? "Unknown";
        private string Role => HttpContext.Session.GetString("Role")     ?? "Unknown";
        private string IP   => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

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
            _audit.Log(Who, Role, "Create", "Teacher",
                $"Added teacher: {teacher.FullName} (Email: {teacher.Email})", IP);
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
            if (teacher == null) return NotFound();

            return Json(new
            {
                teacherId   = teacher.TeacherId,
                fullName    = teacher.FullName,
                subjectId   = teacher.SubjectId,
                email       = teacher.Email,
                credentials = teacher.Credentials
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
            _audit.Log(Who, Role, "Update", "Teacher",
                $"Updated teacher: {teacher.FullName} (ID: {teacher.TeacherId})", IP);
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
            var teacher = _teacherService.GetById(id);
            _teacherService.Delete(id);
            _audit.Log(Who, Role, "Delete", "Teacher",
                $"Deleted teacher ID: {id}" + (teacher != null ? $" ({teacher.FullName})" : ""), IP);
            TempData["Success"] = "Teacher deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}