using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── helpers ──────────────────────────────────────────────

        /// <summary>
        /// Returns all sections this teacher is associated with
        /// (advisory OR subject-teacher via SectionSubject).
        /// </summary>
        private async Task<List<SectionSwitcherItemVM>> GetTeacherSectionsAsync(Teacher teacher)
        {
            var advisorySectionIds = await _context.Sections
                .Where(s => s.IsActive && s.Adviser == teacher.FullName)
                .Select(s => s.SectionId)
                .ToListAsync();

            var subjectSectionIds = await _context.SectionSubjects
                .Where(ss => ss.TeacherId == teacher.TeacherId)
                .Select(ss => ss.SectionId)
                .Distinct()
                .ToListAsync();

            var allIds = advisorySectionIds.Union(subjectSectionIds).Distinct().ToList();

            var sections = await _context.Sections
                .Where(s => s.IsActive && allIds.Contains(s.SectionId))
                .OrderByDescending(s => advisorySectionIds.Contains(s.SectionId))
                .ThenBy(s => s.SectionName)
                .ToListAsync();

            return sections.Select(s => new SectionSwitcherItemVM
            {
                SectionId   = s.SectionId,
                SectionName = s.SectionName,
                GradeLevel  = s.GradeLevel ?? "—",
                IsAdviser   = advisorySectionIds.Contains(s.SectionId)
            }).ToList();
        }

        // ── INDEX ─────────────────────────────────────────────────
        public async Task<IActionResult> Index(int? sectionId)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var teacherId = HttpContext.Session.GetInt32("UserId");

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                return View(new AttendanceIndexVM { SectionName = "No Section Assigned" });

            // Get all sections this teacher handles
            var availableSections = await GetTeacherSectionsAsync(teacher);

            if (!availableSections.Any())
                return View(new AttendanceIndexVM
                {
                    SectionName       = "No Section Assigned",
                    AvailableSections = availableSections
                });

            // Resolve which section to show:
            // - Use sectionId from query if valid, else default to first (advisory first)
            SectionSwitcherItemVM selectedInfo;
            if (sectionId.HasValue && availableSections.Any(s => s.SectionId == sectionId.Value))
                selectedInfo = availableSections.First(s => s.SectionId == sectionId.Value);
            else
                selectedInfo = availableSections.First();

            var section = await _context.Sections
                .Include(s => s.Students.Where(st => st.IsActive))
                .FirstOrDefaultAsync(s => s.SectionId == selectedInfo.SectionId);

            if (section == null)
                return View(new AttendanceIndexVM
                {
                    SectionName       = "Section Not Found",
                    AvailableSections = availableSections
                });

            var today = DateTime.Today;

            var allAttendance = await _context.Attendances
                .Where(a => a.SectionId == section.SectionId)
                .Include(a => a.Student)
                .ToListAsync();

            var todayRecords = allAttendance.Where(a => a.Date.Date == today).ToList();

            // Student summaries
            var summaries = section.Students.Select(st =>
            {
                var records     = allAttendance.Where(a => a.StudentId == st.StudentId).ToList();
                var todayRecord = todayRecords.FirstOrDefault(r => r.StudentId == st.StudentId);

                return new StudentAttendanceSummaryVM
                {
                    StudentId         = st.StudentId,
                    FullName          = $"{st.LastName}, {st.FirstName}",
                    LRN               = st.LRN ?? "—",
                    Present           = records.Count(r => r.Status == "Present"),
                    Absent            = records.Count(r => r.Status == "Absent"),
                    Late              = records.Count(r => r.Status == "Late"),
                    Excused           = records.Count(r => r.Status == "Excused"),
                    Total             = records.Count,
                    TodayAttendanceId = todayRecord?.AttendanceId,
                    TodayStatus       = todayRecord?.Status
                };
            }).OrderBy(s => s.FullName).ToList();

            // Recent history (last 7 days)
            var recentHistory = allAttendance
                .GroupBy(a => a.Date.Date)
                .OrderByDescending(g => g.Key)
                .Take(7)
                .Select(g => new AttendanceHistoryDayVM
                {
                    Date = g.Key,
                    Records = g.Select(a => new AttendanceRecordVM
                    {
                        FullName = a.Student != null
                            ? $"{a.Student.LastName}, {a.Student.FirstName}"
                            : "—",
                        LRN    = a.Student?.LRN ?? "—",
                        Status = a.Status
                    }).OrderBy(r => r.FullName).ToList()
                }).ToList();

            var vm = new AttendanceIndexVM
            {
                SectionId            = section.SectionId,
                SectionName          = section.SectionName,
                IsAdviser            = selectedInfo.IsAdviser,
                AttendanceTakenToday = todayRecords.Any(),
                PresentToday         = todayRecords.Count(r => r.Status == "Present"),
                AbsentToday          = todayRecords.Count(r => r.Status == "Absent"),
                LateToday            = todayRecords.Count(r => r.Status == "Late"),
                ExcusedToday         = todayRecords.Count(r => r.Status == "Excused"),
                TotalStudents        = section.Students.Count,
                AvailableSections    = availableSections,
                StudentSummaries     = summaries,
                RecentHistory        = recentHistory
            };

            return View(vm);
        }

        // ── TAKE ATTENDANCE (GET) ─────────────────────────────────
        public async Task<IActionResult> Take(int? sectionId)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var teacherId = HttpContext.Session.GetInt32("UserId");

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                return RedirectToAction("Index");

            var availableSections = await GetTeacherSectionsAsync(teacher);

            // Resolve section
            SectionSwitcherItemVM? selectedInfo = null;
            if (sectionId.HasValue)
                selectedInfo = availableSections.FirstOrDefault(s => s.SectionId == sectionId.Value);
            if (selectedInfo == null)
                selectedInfo = availableSections.FirstOrDefault();

            if (selectedInfo == null)
                return RedirectToAction("Index");

            var section = await _context.Sections
                .Include(s => s.Students.Where(st => st.IsActive))
                .FirstOrDefaultAsync(s => s.SectionId == selectedInfo.SectionId);

            if (section == null)
                return RedirectToAction("Index");

            var today = DateTime.Today;

            var alreadyTaken = await _context.Attendances
                .AnyAsync(a => a.SectionId == section.SectionId && a.Date.Date == today);

            if (alreadyTaken)
            {
                TempData["Warning"] = "Attendance for today has already been submitted for this section.";
                return RedirectToAction("Index", new { sectionId = section.SectionId });
            }

            var vm = new AttendanceTakeVM
            {
                SectionId   = section.SectionId,
                SectionName = section.SectionName,
                Date        = today,
                Students    = section.Students
                    .OrderBy(st => st.LastName)
                    .Select(st => new StudentAttendanceInputVM
                    {
                        StudentId = st.StudentId,
                        FullName  = $"{st.LastName}, {st.FirstName}",
                        LRN       = st.LRN ?? "—",
                        Status    = "Present"
                    }).ToList()
            };

            return View(vm);
        }

        // ── TAKE ATTENDANCE (POST) ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Take(AttendanceTakeVM vm)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var teacherId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var today     = DateTime.Today;

            var alreadyTaken = await _context.Attendances
                .AnyAsync(a => a.SectionId == vm.SectionId && a.Date.Date == today);

            if (alreadyTaken)
            {
                TempData["Warning"] = "Attendance for today has already been submitted.";
                return RedirectToAction("Index", new { sectionId = vm.SectionId });
            }

            var records = vm.Students.Select(s => new Attendance
            {
                StudentId = s.StudentId,
                SectionId = vm.SectionId,
                TeacherId = teacherId,
                Date      = today,
                Status    = s.Status
            }).ToList();

            _context.Attendances.AddRange(records);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Attendance submitted successfully.";
            return RedirectToAction("Index", new { sectionId = vm.SectionId });
        }

        // ── EDIT STATUS (POST) ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int attendanceId, string status, int? returnSectionId)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var record = await _context.Attendances.FindAsync(attendanceId);

            if (record != null)
            {
                record.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Attendance updated successfully.";
            }
            else
            {
                TempData["Error"] = "Attendance record not found.";
            }

            return RedirectToAction("Index", new { sectionId = returnSectionId });
        }

        // ── STUDENT VIEW ──────────────────────────────────────────
        public async Task<IActionResult> StudentView()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var studentId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var student = await _context.Students
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return RedirectToAction("Login", "Account");

            var records = await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            int present  = records.Count(r => r.Status == "Present");
            int absent   = records.Count(r => r.Status == "Absent");
            int late     = records.Count(r => r.Status == "Late");
            int excused  = records.Count(r => r.Status == "Excused");
            int total    = records.Count;

            double rate = total > 0
                ? Math.Round((present + late + excused) * 100.0 / total, 1)
                : 0;

            var vm = new StudentAttendanceVM
            {
                StudentName    = $"{student.FirstName} {student.LastName}",
                SectionName    = student.Section?.SectionName ?? "—",
                TotalDays      = total,
                Present        = present,
                Absent         = absent,
                Late           = late,
                Excused        = excused,
                AttendanceRate = rate,
                Records        = records.Select(r => new StudentAttendanceDayVM
                {
                    Date   = r.Date,
                    Status = r.Status
                }).ToList()
            };

            return View(vm);
        }
    }
}