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

    var vm = new TeacherDashboardVM
    {
        TotalClasses          = sections.Count,
        TotalStudentsHandled  = sections.Sum(s => s.Students.Count(st => st.IsActive)),
        TodayAttendanceTaken  = 0,
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
                .FirstOrDefault(s => s.StudentId == studentId);

            var vm = new StudentDashboardVM
            {
                FullName = student != null
                    ? student.FirstName + " " + student.LastName
                    : "Student",

                AttendanceRate = 0,
                AverageGrade   = 0,
                Remarks        = "No Data Yet"
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