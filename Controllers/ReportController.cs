using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Shared: resolve teacher's sections ────────────────────
        private async Task<List<Section>> GetSectionsAsync(string? role, int? teacherId)
        {
            if (role == "Admin")
                return await _context.Sections.Where(s => s.IsActive).ToListAsync();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == teacherId);
            if (teacher == null) return new();

            var advisoryIds = await _context.Sections
                .Where(s => s.IsActive && s.Adviser == teacher.FullName)
                .Select(s => s.SectionId).ToListAsync();

            var subjectIds = await _context.SectionSubjects
                .Where(ss => ss.TeacherId == teacher.TeacherId)
                .Select(ss => ss.SectionId).Distinct().ToListAsync();

            var allIds = advisoryIds.Union(subjectIds).Distinct().ToList();
            return await _context.Sections
                .Where(s => s.IsActive && allIds.Contains(s.SectionId))
                .ToListAsync();
        }

        // ── INDEX ─────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            int?     sectionId,
            string   tab      = "attendance",
            string?  dateFrom = null,
            string?  dateTo   = null)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var role      = HttpContext.Session.GetString("Role");
            var teacherId = HttpContext.Session.GetInt32("UserId");

            if (role != "Teacher" && role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var sections = await GetSectionsAsync(role, teacherId);
            if (!sectionId.HasValue && sections.Any())
                sectionId = sections.First().SectionId;

            var selectedSection = sections.FirstOrDefault(s => s.SectionId == sectionId);

            // Parse date range
            DateTime? from = ParseDate(dateFrom);
            DateTime? to   = ParseDate(dateTo);

            // ── Students ─────────────────────────────────────────
            var students = sectionId.HasValue
                ? await _context.Students
                    .Where(s => s.SectionId == sectionId && s.IsActive)
                    .OrderBy(s => s.LastName)
                    .ToListAsync()
                : new();

            var studentIds = students.Select(s => s.StudentId).ToList();

            // ── Attendance (date-filtered for attendance tab) ─────
            IQueryable<Attendance> attQuery = _context.Attendances
                .Where(a => studentIds.Contains(a.StudentId));

            if (from.HasValue) attQuery = attQuery.Where(a => a.Date >= from.Value);
            if (to.HasValue)   attQuery = attQuery.Where(a => a.Date <= to.Value.AddDays(1).AddTicks(-1));

            var attendances = studentIds.Any() ? await attQuery.ToListAsync() : new();

            // ── Grades (not date-filtered) ────────────────────────
            var grades = studentIds.Any()
                ? await _context.Grades
                    .Where(g => studentIds.Contains(g.StudentId))
                    .ToListAsync()
                : new();

            // ── Build rows ────────────────────────────────────────
            var attendanceRows = students.Select(s =>
            {
                var recs    = attendances.Where(a => a.StudentId == s.StudentId).ToList();
                int total   = recs.Count;
                int present = recs.Count(a => a.Status == "Present" || a.Status == "Late" || a.Status == "Excused");
                int absent  = recs.Count(a => a.Status == "Absent");
                int late    = recs.Count(a => a.Status == "Late");
                double rate = total > 0 ? Math.Round(present * 100.0 / total, 1) : 0;
                return new AttendanceReportRow
                {
                    StudentId = s.StudentId, FullName = s.FirstName + " " + s.LastName,
                    TotalDays = total, Present = present, Absent = absent, Late = late, Rate = rate
                };
            }).OrderByDescending(r => r.Rate).ToList();

            var gradeRows = students.Select(s =>
            {
                var sg  = grades.Where(g => g.StudentId == s.StudentId).ToList();
                double? q1  = Avg(sg.Where(g => g.Quarter == 1));
                double? q2  = Avg(sg.Where(g => g.Quarter == 2));
                double? q3  = Avg(sg.Where(g => g.Quarter == 3));
                double? avg = sg.Any() ? Math.Round(sg.Average(g => (double)g.GradeValue), 1) : null;
                return new GradeReportRow
                {
                    StudentId = s.StudentId, FullName = s.FirstName + " " + s.LastName,
                    Q1 = q1, Q2 = q2, Q3 = q3, Average = avg
                };
            }).OrderByDescending(r => r.Average).ToList();

            var vm = new TeacherReportVM
            {
                Sections           = sections,
                SelectedSection    = selectedSection,
                SelectedTab        = tab,
                AttendanceRows     = attendanceRows,
                GradeRows          = gradeRows,
                TotalStudents      = students.Count,
                DateFrom           = dateFrom,
                DateTo             = dateTo,
                ClassAvgGrade      = gradeRows.Where(r => r.Average.HasValue).Any()
                                     ? Math.Round(gradeRows.Where(r => r.Average.HasValue).Average(r => r.Average!.Value), 1)
                                     : (double?)null,
                ClassAvgAttendance = attendanceRows.Any()
                                     ? Math.Round(attendanceRows.Average(r => r.Rate), 1)
                                     : (double?)null
            };

            return View(vm);
        }

        // ── DOWNLOAD CSV ──────────────────────────────────────────────
public async Task<IActionResult> Download(
    int?    sectionId,
    string  tab      = "attendance",
    string? dateFrom = null,
    string? dateTo   = null)
{
    if (HttpContext.Session.GetString("Username") == null)
        return RedirectToAction("Login", "Account");

    var role      = HttpContext.Session.GetString("Role");
    var teacherId = HttpContext.Session.GetInt32("UserId");

    var sections = await GetSectionsAsync(role, teacherId);
    var section  = sections.FirstOrDefault(s => s.SectionId == sectionId)
                ?? sections.FirstOrDefault();

    if (section == null) return NotFound();

    var students = await _context.Students
        .Where(s => s.SectionId == section.SectionId && s.IsActive)
        .OrderBy(s => s.LastName)
        .ToListAsync();

    var studentIds = students.Select(s => s.StudentId).ToList();
    DateTime? from = ParseDate(dateFrom);
    DateTime? to   = ParseDate(dateTo);

    var sb = new System.Text.StringBuilder();

    if (tab == "attendance")
    {
        // Build attendance data
        IQueryable<Attendance> attQ = _context.Attendances
            .Where(a => studentIds.Contains(a.StudentId));
        if (from.HasValue) attQ = attQ.Where(a => a.Date >= from.Value);
        if (to.HasValue)   attQ = attQ.Where(a => a.Date <= to.Value.AddDays(1).AddTicks(-1));
        var attendances = studentIds.Any() ? await attQ.ToListAsync() : new();

        // Header
        sb.AppendLine($"Attendance Report - {section.SectionName}");
        sb.AppendLine($"Period: {(from.HasValue ? from.Value.ToString("MMMM dd yyyy") : "All")} to {(to.HasValue ? to.Value.ToString("MMMM dd yyyy") : "Present")}");
        sb.AppendLine($"Generated: {DateTime.Now:MMMM dd yyyy h:mm tt}");
        sb.AppendLine();
        sb.AppendLine("#,Student Name,Student ID,Total Days,Present,Absent,Late,Attendance Rate,Status");

        int i = 1;
        foreach (var s in students)
        {
            var recs    = attendances.Where(a => a.StudentId == s.StudentId).ToList();
            int total   = recs.Count;
            int present = recs.Count(a => a.Status == "Present" || a.Status == "Late" || a.Status == "Excused");
            int absent  = recs.Count(a => a.Status == "Absent");
            int late    = recs.Count(a => a.Status == "Late");
            double rate = total > 0 ? Math.Round(present * 100.0 / total, 1) : 0;
            string status = rate >= 90 ? "Good" : rate >= 75 ? "Fair" : "At Risk";

            sb.AppendLine($"{i++},\"{s.FirstName} {s.LastName}\",{s.StudentId},{total},{present},{absent},{late},{rate}%,{status}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var filename = $"Attendance_{section.SectionName}_{DateTime.Now:yyyyMMdd}.csv";
        return File(bytes, "text/csv", filename);
    }
    else // grades
    {
        var grades = studentIds.Any()
            ? await _context.Grades.Where(g => studentIds.Contains(g.StudentId)).ToListAsync()
            : new();

        // Header
        sb.AppendLine($"Grade Report - {section.SectionName}");
        sb.AppendLine($"Generated: {DateTime.Now:MMMM dd yyyy h:mm tt}");
        sb.AppendLine();
        sb.AppendLine("#,Student Name,Student ID,Quarter 1,Quarter 2,Quarter 3,Average,Remark");

        int i = 1;
        foreach (var s in students)
        {
            var sg      = grades.Where(g => g.StudentId == s.StudentId).ToList();
            double? q1  = Avg(sg.Where(g => g.Quarter == 1));
            double? q2  = Avg(sg.Where(g => g.Quarter == 2));
            double? q3  = Avg(sg.Where(g => g.Quarter == 3));
            double? avg = sg.Any() ? Math.Round(sg.Average(g => (double)g.GradeValue), 1) : null;

            string remark = !avg.HasValue    ? "No Data"
                          : avg >= 85        ? "Outstanding"
                          : avg >= 75        ? "Satisfactory"
                          :                    "Needs Improvement";

            sb.AppendLine($"{i++},\"{s.FirstName} {s.LastName}\",{s.StudentId},{q1?.ToString() ?? "—"},{q2?.ToString() ?? "—"},{q3?.ToString() ?? "—"},{avg?.ToString() ?? "—"},{remark}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var filename = $"Grades_{section.SectionName}_{DateTime.Now:yyyyMMdd}.csv";
        return File(bytes, "text/csv", filename);
    }
}

        // ── PRINT VIEW (PDF via browser print dialog) ────────────
        public async Task<IActionResult> PrintView(
            int?    sectionId,
            string  tab      = "attendance",
            string? dateFrom = null,
            string? dateTo   = null)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var role      = HttpContext.Session.GetString("Role");
            var teacherId = HttpContext.Session.GetInt32("UserId");

            var sections = await GetSectionsAsync(role, teacherId);
            var section  = sections.FirstOrDefault(s => s.SectionId == sectionId)
                        ?? sections.FirstOrDefault();

            if (section == null) return NotFound();

            var students = await _context.Students
                .Where(s => s.SectionId == section.SectionId && s.IsActive)
                .OrderBy(s => s.LastName).ToListAsync();

            var studentIds = students.Select(s => s.StudentId).ToList();
            DateTime? from = ParseDate(dateFrom);
            DateTime? to   = ParseDate(dateTo);

            // Attendance rows
            IQueryable<Attendance> attQ = _context.Attendances
                .Where(a => studentIds.Contains(a.StudentId));
            if (from.HasValue) attQ = attQ.Where(a => a.Date >= from.Value);
            if (to.HasValue)   attQ = attQ.Where(a => a.Date <= to.Value.AddDays(1).AddTicks(-1));
            var attendances = studentIds.Any() ? await attQ.ToListAsync() : new();

            var attendanceRows = students.Select(s => {
                var recs    = attendances.Where(a => a.StudentId == s.StudentId).ToList();
                int total   = recs.Count;
                int present = recs.Count(a => a.Status == "Present" || a.Status == "Late" || a.Status == "Excused");
                int absent  = recs.Count(a => a.Status == "Absent");
                int late    = recs.Count(a => a.Status == "Late");
                double rate = total > 0 ? Math.Round(present * 100.0 / total, 1) : 0;
                return new AttendanceReportRow {
                    StudentId = s.StudentId, FullName = s.FirstName + " " + s.LastName,
                    TotalDays = total, Present = present, Absent = absent, Late = late, Rate = rate
                };
            }).OrderByDescending(r => r.Rate).ToList();

            // Grade rows
            var grades = studentIds.Any()
                ? await _context.Grades.Where(g => studentIds.Contains(g.StudentId)).ToListAsync()
                : new();

            var gradeRows = students.Select(s => {
                var sg  = grades.Where(g => g.StudentId == s.StudentId).ToList();
                double? q1  = Avg(sg.Where(g => g.Quarter == 1));
                double? q2  = Avg(sg.Where(g => g.Quarter == 2));
                double? q3  = Avg(sg.Where(g => g.Quarter == 3));
                double? avg = sg.Any() ? Math.Round(sg.Average(g => (double)g.GradeValue), 1) : null;
                return new GradeReportRow {
                    StudentId = s.StudentId, FullName = s.FirstName + " " + s.LastName,
                    Q1 = q1, Q2 = q2, Q3 = q3, Average = avg
                };
            }).OrderByDescending(r => r.Average).ToList();

            string period = (from.HasValue || to.HasValue)
                ? $"{(from.HasValue ? from.Value.ToString("MMMM dd, yyyy") : "All")} – {(to.HasValue ? to.Value.ToString("MMMM dd, yyyy") : "Present")}"
                : "All Dates";

            var vm = new PrintReportVM
            {
                SectionName     = section.SectionName,
                GradeLevel      = section.GradeLevel ?? "",
                Tab             = tab,
                Period          = period,
                GeneratedAt     = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"),
                AttendanceRows  = attendanceRows,
                GradeRows       = gradeRows,
                TotalStudents   = students.Count,
                ClassAvgAttendance = attendanceRows.Any()
                                  ? Math.Round(attendanceRows.Average(r => r.Rate), 1) : (double?)null,
                ClassAvgGrade   = gradeRows.Where(r => r.Average.HasValue).Any()
                                  ? Math.Round(gradeRows.Where(r => r.Average.HasValue).Average(r => r.Average!.Value), 1) : (double?)null
            };

            // Render without the main layout
            return View(vm);
        }

        // ── Helper ────────────────────────────────────────────────
        private static DateTime? ParseDate(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return null;
            return DateTime.TryParse(val, out var d) ? d : null;
        }

        private static double? Avg(IEnumerable<Grade> grades)
        {
            var list = grades.ToList();
            return list.Any() ? Math.Round(list.Average(g => (double)g.GradeValue), 1) : null;
        }
    }

    // ── ViewModels ────────────────────────────────────────────────

    public class TeacherReportVM
    {
        public List<Section>             Sections           { get; set; } = new();
        public Section?                  SelectedSection    { get; set; }
        public string                    SelectedTab        { get; set; } = "attendance";
        public List<AttendanceReportRow> AttendanceRows     { get; set; } = new();
        public List<GradeReportRow>      GradeRows          { get; set; } = new();
        public int                       TotalStudents      { get; set; }
        public double?                   ClassAvgGrade      { get; set; }
        public double?                   ClassAvgAttendance { get; set; }
        public string?                   DateFrom           { get; set; }
        public string?                   DateTo             { get; set; }
    }

    public class AttendanceReportRow
    {
        public int    StudentId { get; set; }
        public string FullName  { get; set; } = "";
        public int    TotalDays { get; set; }
        public int    Present   { get; set; }
        public int    Absent    { get; set; }
        public int    Late      { get; set; }
        public double Rate      { get; set; }
    }

    public class GradeReportRow
    {
        public int     StudentId { get; set; }
        public string  FullName  { get; set; } = "";
        public double? Q1        { get; set; }
        public double? Q2        { get; set; }
        public double? Q3        { get; set; }
        public double? Average   { get; set; }
    }

    public class PrintReportVM
    {
        public string                    SectionName        { get; set; } = "";
        public string                    GradeLevel         { get; set; } = "";
        public string                    Tab                { get; set; } = "attendance";
        public string                    Period             { get; set; } = "";
        public string                    GeneratedAt        { get; set; } = "";
        public int                       TotalStudents      { get; set; }
        public double?                   ClassAvgAttendance { get; set; }
        public double?                   ClassAvgGrade      { get; set; }
        public List<AttendanceReportRow> AttendanceRows     { get; set; } = new();
        public List<GradeReportRow>      GradeRows          { get; set; } = new();
    }
}
