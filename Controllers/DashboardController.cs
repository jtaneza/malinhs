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
            var today = DateTime.Today;

            // ── Basic counts ─────────────────────────────────────────────
            var vm = new DashboardVM
            {
                TotalStudents = _context.Students.Count(),
                TotalTeachers = _context.Teachers.Count(),
                TotalSections = _context.Sections.Count(),
                TotalUsers    = _context.Users.Count()
            };

            // ── Monthly enrollment trend (last 8 months, cumulative) ──────
            // We derive the count for each past month by taking the current
            // total and subtracting any student-Create audit events that
            // happened AFTER that month — giving a realistic growth curve.
            var eightMonthsAgo = new DateTime(today.AddMonths(-7).Year, today.AddMonths(-7).Month, 1);

            var createTimestamps = _context.AuditLogs
                .Where(a => a.Action == "Create" && a.Module == "Student")
                .Select(a => a.Timestamp)
                .ToList();

            int currentTotal = _context.Students.Count();

            for (int i = 7; i >= 0; i--)
            {
                var month    = today.AddMonths(-i);
                var monthEnd = new DateTime(month.Year, month.Month,
                                   DateTime.DaysInMonth(month.Year, month.Month), 23, 59, 59);

                // Estimated count at end of this month:
                // current total minus students created AFTER this month
                int addedAfter = createTimestamps.Count(t => t > monthEnd);
                int estimated  = Math.Max(0, currentTotal - addedAfter);

                vm.MonthLabels.Add(month.ToString("MMM yyyy"));
                vm.MonthlyEnrollment.Add(estimated);
            }

            // ── Recent activities from AuditLogs ─────────────────────────
            vm.RecentActivities = _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Select(a => new RecentActivityItem
                {
                    Message   = a.PerformedBy + " [" + a.Role + "] " + a.Action + "d " + a.Module + ": " + a.Description,
                    CreatedAt = a.Timestamp
                })
                .ToList();

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