using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX (ROLE ROUTER) =================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");

            return role switch
            {
                "Teacher" => RedirectToAction("Teacher"),
                "Student" => RedirectToAction("Student"),
                "Cashier" => RedirectToAction("Index", "Payment"),
                _         => AdminDashboard()
            };
        }

        // ================= ADMIN =================
        private IActionResult AdminDashboard()
        {
            var vm = new DashboardVM
            {
                TotalStudents = _context.Students.Count(),
                TotalTeachers = _context.Teachers.Count(),
                TotalSections = _context.Sections.Count(),
                TotalUsers    = _context.Users.Count()
            };

            return View("Index", vm);
        }

        // ================= TEACHER =================
        public IActionResult Teacher()
{
    if (HttpContext.Session.GetString("Username") == null)
        return RedirectToAction("Login", "Account");

    var teacherId = HttpContext.Session.GetInt32("UserId");

    var teacher = _context.Teachers
        .FirstOrDefault(t => t.TeacherId == teacherId);

    var sections = teacher == null
        ? new List<MalikongkongNHS.Models.Entities.Section>()
        : _context.Sections
            .Where(s => s.IsActive && s.Adviser == teacher.FullName)
            .Include(s => s.Students)
            .ToList();

    var sectionIds = sections.Select(s => s.SectionId).ToList();
    var today = DateTime.Today;

    var todayAttendanceCount = sectionIds.Any()
        ? _context.Attendances.Count(a => sectionIds.Contains(a.SectionId) && a.Date.Date == today)
        : 0;

    var vm = new TeacherDashboardVM
    {
        TotalClasses          = sections.Count,
        TotalStudentsHandled  = sections.Sum(s => s.Students.Count(st => st.IsActive)),
        TodayAttendanceTaken  = todayAttendanceCount,
        PendingGrades         = 0
    };

    return View(vm);
}


        // ================= STUDENT =================
        public IActionResult Student()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var studentId = HttpContext.Session.GetInt32("UserId");

            var student = _context.Students
                .Include(s => s.Section)
                    .ThenInclude(sec => sec!.SectionSubjects)
                .FirstOrDefault(s => s.StudentId == studentId);

            // Count subjects from the section's SectionSubjects
            int subjectCount = student?.Section?.SectionSubjects?.Count ?? 0;

            // Real attendance rate
            var attendanceRecords = _context.Attendances
                .Where(a => a.StudentId == studentId)
                .ToList();
            int total   = attendanceRecords.Count;
            int present = attendanceRecords.Count(r => r.Status == "Present" || r.Status == "Late" || r.Status == "Excused");
            double rate = total > 0 ? Math.Round(present * 100.0 / total, 1) : 0;

            var vm = new StudentDashboardVM
            {
                FullName = student != null
                    ? student.FirstName + " " + student.LastName
                    : "Student",
                Subjects       = subjectCount,
                AttendanceRate = rate,
                AverageGrade   = 0,
                Remarks        = string.Empty
            };

            return View(vm);
        }

        // ================= CASHIER =================
        public IActionResult Cashier()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var vm = new CashierDashboardVM
            {
                TotalRevenue       = 0,
                TotalTransactions  = 0,
                PaidStudents       = 0,
                PendingPayments    = _context.Students.Count()
            };

            return View(vm);
        }
    }
}