using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int?   UserId => HttpContext.Session.GetInt32("UserId");
        private string Role   => HttpContext.Session.GetString("Role") ?? "";

        // =========================
        // GET NOTIFICATIONS (JSON)
        // =========================
        [HttpGet]
        public IActionResult GetNotifications()
        {
            if (UserId == null) return Unauthorized();

            // Auto-generate smart notifications from real data
            GenerateSmartNotifications();

            var items = _context.Notifications
                .Where(n => (n.UserId == UserId && n.Role == Role) || n.Role == "All")
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new {
                    id        = n.Id,
                    title     = n.Title,
                    message   = n.Message,
                    type      = n.Type,
                    isRead    = n.IsRead,
                    createdAt = n.CreatedAt
                })
                .ToList();

            return Json(items);
        }

        // =========================
        // GET UNREAD COUNT (JSON)
        // =========================
        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            if (UserId == null) return Json(0);

            var count = _context.Notifications
                .Count(n => !n.IsRead &&
                       ((n.UserId == UserId && n.Role == Role) || n.Role == "All"));

            return Json(count);
        }

        // =========================
        // MARK ONE AS READ
        // =========================
        [HttpPost]
        public IActionResult MarkRead(int id)
        {
            var notif = _context.Notifications.Find(id);
            if (notif != null && (notif.UserId == UserId || notif.Role == "All"))
            {
                notif.IsRead = true;
                _context.SaveChanges();
            }
            return Ok();
        }

        // =========================
        // MARK ALL AS READ
        // =========================
        [HttpPost]
        public IActionResult MarkAllRead()
        {
            if (UserId == null) return Unauthorized();

            var items = _context.Notifications
                .Where(n => !n.IsRead &&
                       ((n.UserId == UserId && n.Role == Role) || n.Role == "All"))
                .ToList();

            items.ForEach(n => n.IsRead = true);
            _context.SaveChanges();
            return Ok();
        }

        // =========================
        // SMART NOTIFICATION GENERATOR
        // Checks real DB data and creates notifications if not already created
        // =========================
        private void GenerateSmartNotifications()
        {
            if (UserId == null) return;

            if (Role == "Student")
                GenerateStudentNotifications(UserId.Value);
            else if (Role == "Teacher")
                GenerateTeacherNotifications(UserId.Value);
        }

        private void GenerateStudentNotifications(int studentId)
        {
            var today = DateTime.Today;

            // 1. Absent today?
            var todayAttendance = _context.Attendances
                .Where(a => a.StudentId == studentId && a.Date.Date == today)
                .FirstOrDefault();

            if (todayAttendance != null && todayAttendance.Status == "Absent")
            {
                var key = $"absent_{studentId}_{today:yyyyMMdd}";
                if (!NotifExists(studentId, key))
                {
                    AddNotif(studentId, "Student", "Absent Today",
                        $"You were marked absent on {today:MMMM dd, yyyy}.",
                        "attendance", key);
                }
            }

            // 2. Late this week?
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var lateCount = _context.Attendances
                .Count(a => a.StudentId == studentId
                         && a.Date >= weekStart
                         && a.Status == "Late");

            if (lateCount >= 2)
            {
                var key = $"late_week_{studentId}_{weekStart:yyyyMMdd}";
                if (!NotifExists(studentId, key))
                {
                    AddNotif(studentId, "Student", "Late Arrivals This Week",
                        $"You have been late {lateCount} times this week.",
                        "attendance", key);
                }
            }

            // 3. New grade posted? (check if any grade exists for student)
            var hasGrades = _context.Grades
                .Any(g => g.StudentId == studentId);

            if (hasGrades)
            {
// Around line 156 — both g.Id → g.GradeId
var latestGrade = _context.Grades
    .Where(g => g.StudentId == studentId)
    .OrderByDescending(g => g.GradeId)          // ← fixed
    .FirstOrDefault();

if (latestGrade != null)
{
    var key = $"grade_{studentId}_{latestGrade.GradeId}";  // ← fixed
                    if (!NotifExists(studentId, key))
                    {
                        AddNotif(studentId, "Student", "Grade Posted",
                            $"A new grade has been recorded for your account.",
                            "grade", key);
                    }
                }
            }

            // 4. Welcome notification (first login)
            var welcomeKey = $"welcome_{studentId}";
            if (!NotifExists(studentId, welcomeKey))
            {
                AddNotif(studentId, "Student", "Welcome to Malikongkong NHS SErpS!",
                    "Your student portal is ready. Check your schedule, attendance, and grades.",
                    "system", welcomeKey);
            }
        }

        private void GenerateTeacherNotifications(int teacherId)
        {
            // Welcome notification
            var welcomeKey = $"welcome_teacher_{teacherId}";
            if (!NotifExists(teacherId, welcomeKey))
            {
                AddNotif(teacherId, "Teacher", "Welcome to Malikongkong NHS SErpS!",
                    "Your teacher portal is ready. Manage your classes, attendance, and grades.",
                    "system", welcomeKey);
            }

            // Remind to take attendance today
            var today = DateTime.Today;
            var attendanceKey = $"remind_attend_{teacherId}_{today:yyyyMMdd}";
            if (!NotifExists(teacherId, attendanceKey))
            {
                var teacher = _context.Teachers.Find(teacherId);
                if (teacher != null)
                {
                    var sections = _context.Sections
                        .Where(s => s.IsActive && s.Adviser == teacher.FullName)
                        .ToList();

                    foreach (var sec in sections)
                    {
                        bool taken = _context.Attendances
                            .Any(a => a.SectionId == sec.SectionId && a.Date.Date == today);
                        if (!taken)
                        {
                            AddNotif(teacherId, "Teacher", "Attendance Reminder",
                                $"Attendance for section {sec.SectionName} has not been taken today.",
                                "attendance", attendanceKey);
                            break;
                        }
                    }
                }
            }
        }

        // ── Helpers ──
        private bool NotifExists(int userId, string key)
        {
            // We store the key in the Message field prefixed with [KEY:...]
            return _context.Notifications
                .Any(n => n.UserId == userId && n.Message.StartsWith($"[KEY:{key}]"));
        }

        private void AddNotif(int userId, string role, string title, string message, string type, string key)
        {
            _context.Notifications.Add(new Notification
            {
                UserId    = userId,
                Role      = role,
                Title     = title,
                Message   = $"[KEY:{key}] {message}",
                Type      = type,
                IsRead    = false,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();
        }
    }
}
