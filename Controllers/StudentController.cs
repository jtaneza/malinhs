using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IAuditService   _audit;

        public StudentController(IStudentService studentService, IAuditService audit)
        {
            _studentService = studentService;
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
            var students = _studentService.GetAllStudents();
            return View(students);
        }

        // =========================
        // CREATE
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Sections = _studentService.GetSections();
            return View(new Student());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sections = _studentService.GetSections();
                return View(student);
            }
            _studentService.Add(student);
            _audit.Log(Who, Role, "Create", "Student",
                $"Added student: {student.FirstName} {student.LastName} (LRN: {student.LRN})", IP);
            TempData["Success"] = "Student added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT (Admin)
        // =========================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _studentService.GetById(id);
            if (student == null) return NotFound();
            ViewBag.Sections = _studentService.GetSections();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student student)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sections = _studentService.GetSections();
                return View(student);
            }
            _studentService.Update(student);
            _audit.Log(Who, Role, "Update", "Student",
                $"Updated student: {student.FirstName} {student.LastName} (ID: {student.StudentId})", IP);
            TempData["Success"] = "Student updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var student = _studentService.GetById(id);
            _studentService.Delete(id);
            _audit.Log(Who, Role, "Delete", "Student",
                $"Deleted student ID: {id}" + (student != null ? $" ({student.FirstName} {student.LastName})" : ""), IP);
            TempData["Success"] = "Student deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // TOGGLE STATUS
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var student = _studentService.GetById(id);
            _studentService.ToggleStatus(id);
            _audit.Log(Who, Role, "Update", "Student",
                $"Toggled status for student ID: {id}" + (student != null ? $" ({student.FirstName} {student.LastName})" : ""), IP);
            TempData["Success"] = "Student status updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // STUDENT PORTAL — PROFILE
        // =========================

        [HttpGet]
        public IActionResult Profile()
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null)
                return RedirectToAction("Login", "Account");

            var student = _studentService.GetById(studentId.Value);
            if (student == null) return NotFound();

            return View(student);
        }

        // =========================
        // STUDENT PORTAL — UPDATE PROFILE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(int StudentId, string? ContactNumber, string? Address, string? GuardianName, string? Email)
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null || studentId.Value != StudentId)
                return RedirectToAction("Login", "Account");

            var student = _studentService.GetById(StudentId);
            if (student == null) return NotFound();

            student.ContactNumber = ContactNumber;
            student.Address       = Address;
            student.GuardianName  = GuardianName;
            student.Email         = Email;

            _studentService.Update(student);
            _audit.Log(Who, Role, "Update", "Student",
                $"Student ID {StudentId} updated their profile.", IP);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        // =========================
        // CHANGE PASSWORD
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(int StudentId, string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var student = _studentService.GetById(StudentId);
            if (student == null) return NotFound();

            if (student.Password != CurrentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Profile));
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction(nameof(Profile));
            }

            student.Password = NewPassword;
            _studentService.Update(student);
            _audit.Log(Who, Role, "Update", "Student",
                $"Student ID {StudentId} changed their password.", IP);

            TempData["Success"] = "Password updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
    }
}