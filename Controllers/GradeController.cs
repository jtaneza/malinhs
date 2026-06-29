using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Controllers
{
    public class GradeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Helpers ───────────────────────────────────────────────

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
        public async Task<IActionResult> Index(int? sectionId, int quarter = 1)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            if (role == "Student")
                return RedirectToAction("StudentView");

            if (quarter < 1 || quarter > 3) quarter = 1;

            var teacherId = HttpContext.Session.GetInt32("UserId");
            var teacher   = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                return View(new GradeIndexVM { SectionName = "No Section Assigned" });

            var available = await GetTeacherSectionsAsync(teacher);
            if (!available.Any())
                return View(new GradeIndexVM { SectionName = "No Section Assigned", AvailableSections = available });

            SectionSwitcherItemVM selected;
            if (sectionId.HasValue && available.Any(s => s.SectionId == sectionId.Value))
                selected = available.First(s => s.SectionId == sectionId.Value);
            else
                selected = available.First();

            var sectionSubjects = await _context.SectionSubjects
                .Where(ss => ss.SectionId == selected.SectionId && ss.TeacherId == teacher.TeacherId)
                .Include(ss => ss.Subject)
                .ToListAsync();

            if (selected.IsAdviser && !sectionSubjects.Any())
            {
                sectionSubjects = await _context.SectionSubjects
                    .Where(ss => ss.SectionId == selected.SectionId)
                    .Include(ss => ss.Subject)
                    .ToListAsync();
            }

            var students = await _context.Students
                .Where(st => st.SectionId == selected.SectionId && st.IsActive)
                .OrderBy(st => st.LastName)
                .ToListAsync();

            var allGrades = await _context.Grades
                .Where(g => g.SectionId == selected.SectionId)
                .ToListAsync();

            var subjectGroups = sectionSubjects.Select(ss =>
            {
                var rows = students.Select(st => new GradeStudentRowVM
                {
                    StudentId = st.StudentId,
                    FullName  = $"{st.LastName}, {st.FirstName}",
                    LRN       = st.LRN ?? "—",
                    Q1 = allGrades.FirstOrDefault(gr => gr.StudentId == st.StudentId && gr.SubjectId == ss.SubjectId && gr.Quarter == 1)?.GradeValue,
                    Q2 = allGrades.FirstOrDefault(gr => gr.StudentId == st.StudentId && gr.SubjectId == ss.SubjectId && gr.Quarter == 2)?.GradeValue,
                    Q3 = allGrades.FirstOrDefault(gr => gr.StudentId == st.StudentId && gr.SubjectId == ss.SubjectId && gr.Quarter == 3)?.GradeValue,
                }).ToList();

                bool isFinalized = allGrades.Any(gr =>
                    gr.SubjectId == ss.SubjectId && gr.Quarter == quarter && gr.IsFinalized);

                return new GradeSubjectGroupVM
                {
                    SubjectId   = ss.SubjectId,
                    SubjectName = ss.Subject?.Name ?? "—",
                    IsFinalized = isFinalized,
                    Students    = rows
                };
            }).OrderBy(g => g.SubjectName).ToList();

            var vm = new GradeIndexVM
            {
                SectionId         = selected.SectionId,
                SectionName       = selected.SectionName,
                GradeLevel        = selected.GradeLevel,
                IsAdviser         = selected.IsAdviser,
                Quarter           = quarter,
                AvailableSections = available,
                SubjectGroups     = subjectGroups
            };

            return View(vm);
        }

        // ── ENTER GRADES (GET) ────────────────────────────────────
        public async Task<IActionResult> Enter(int sectionId, int subjectId, int quarter)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            if (quarter < 1 || quarter > 3) quarter = 1;

            var teacherId = HttpContext.Session.GetInt32("UserId");
            var teacher   = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == teacherId);
            if (teacher == null) return RedirectToAction("Index");

            bool hasAccess = await _context.SectionSubjects
                .AnyAsync(ss => ss.SectionId == sectionId && ss.SubjectId == subjectId && ss.TeacherId == teacher.TeacherId);

            if (!hasAccess)
            {
                bool isAdviser = await _context.Sections
                    .AnyAsync(s => s.SectionId == sectionId && s.Adviser == teacher.FullName);
                if (!isAdviser)
                    return RedirectToAction("Index", new { sectionId });
            }

            var section  = await _context.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId);
            var subject  = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectId == subjectId);
            var students = await _context.Students
                .Where(st => st.SectionId == sectionId && st.IsActive)
                .OrderBy(st => st.LastName)
                .ToListAsync();

            var existing = await _context.Grades
                .Where(g => g.SectionId == sectionId && g.SubjectId == subjectId && g.Quarter == quarter)
                .ToListAsync();

            var vm = new GradeEntryVM
            {
                SectionId   = sectionId,
                SectionName = section?.SectionName ?? "—",
                SubjectId   = subjectId,
                SubjectName = subject?.Name ?? "—",
                Quarter     = quarter,
                Students    = students.Select(st =>
                {
                    var g = existing.FirstOrDefault(gr => gr.StudentId == st.StudentId);
                    return new GradeEntryRowVM
                    {
                        StudentId   = st.StudentId,
                        FullName    = $"{st.LastName}, {st.FirstName}",
                        LRN         = st.LRN ?? "—",
                        Score       = g?.GradeValue,
                        GradeId     = g?.GradeId,
                        IsFinalized = g?.IsFinalized ?? false
                    };
                }).ToList()
            };

            return View(vm);
        }

        // ── ENTER GRADES (POST) ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enter(GradeEntryVM vm)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            foreach (var row in vm.Students)
            {
                if (row.Score == null) continue;

                if (row.GradeId.HasValue)
                {
                    var existing = await _context.Grades.FindAsync(row.GradeId.Value);
                    if (existing != null && !existing.IsFinalized)
                        existing.GradeValue = row.Score.Value;
                }
                else
                {
                    bool alreadyExists = await _context.Grades.AnyAsync(g =>
                        g.StudentId == row.StudentId &&
                        g.SectionId == vm.SectionId &&
                        g.SubjectId == vm.SubjectId &&
                        g.Quarter   == vm.Quarter);

                    if (!alreadyExists)
                    {
                        _context.Grades.Add(new Grade
                        {
                            StudentId   = row.StudentId,
                            SectionId   = vm.SectionId,
                            SubjectId   = vm.SubjectId,
                            Quarter     = vm.Quarter,
                            GradeValue  = row.Score.Value,
                            IsFinalized = false
                        });
                    }
                    else
                    {
                        var existing = await _context.Grades.FirstOrDefaultAsync(g =>
                            g.StudentId == row.StudentId &&
                            g.SectionId == vm.SectionId &&
                            g.SubjectId == vm.SubjectId &&
                            g.Quarter   == vm.Quarter);
                        if (existing != null && !existing.IsFinalized)
                            existing.GradeValue = row.Score.Value;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Grades saved for {vm.SubjectName} – Term {vm.Quarter}.";
            return RedirectToAction("Index", new { sectionId = vm.SectionId, quarter = vm.Quarter });
        }

        // ── EDIT SINGLE GRADE (GET) ───────────────────────────────
        public async Task<IActionResult> Edit(int gradeId, int sectionId, int subjectId, int quarter)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(g => g.GradeId == gradeId);

            if (grade == null)
            {
                TempData["Error"] = "Grade record not found.";
                return RedirectToAction("Index", new { sectionId, quarter });
            }

            if (grade.IsFinalized)
            {
                TempData["Error"] = "This grade has been finalized and cannot be edited.";
                return RedirectToAction("Enter", new { sectionId, subjectId, quarter });
            }

            var vm = new GradeEditVM
            {
                GradeId     = grade.GradeId,
                StudentName = $"{grade.Student?.LastName}, {grade.Student?.FirstName}",
                SubjectName = grade.Subject?.Name ?? "—",
                Quarter     = grade.Quarter,
                OldScore    = grade.GradeValue,
                NewScore    = grade.GradeValue,
                SectionId   = sectionId,
                SubjectId   = subjectId
            };

            return View(vm);
        }

        // ── EDIT SINGLE GRADE (POST) ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GradeEditVM vm)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            if (vm.NewScore < 0 || vm.NewScore > 100)
            {
                ModelState.AddModelError("NewScore", "Score must be between 0 and 100.");
                return View(vm);
            }

            var grade = await _context.Grades.FindAsync(vm.GradeId);

            if (grade == null)
            {
                TempData["Error"] = "Grade record not found.";
                return RedirectToAction("Index", new { sectionId = vm.SectionId, quarter = vm.Quarter });
            }

            if (grade.IsFinalized)
            {
                TempData["Error"] = "This grade has been finalized and cannot be edited.";
                return RedirectToAction("Enter", new { sectionId = vm.SectionId, subjectId = vm.SubjectId, quarter = vm.Quarter });
            }

            grade.GradeValue = vm.NewScore;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Grade updated for {vm.StudentName} – Term {vm.Quarter}.";
            return RedirectToAction("Enter", new
            {
                sectionId = vm.SectionId,
                subjectId = vm.SubjectId,
                quarter   = vm.Quarter
            });
        }

        // ── FINALIZE GRADES (POST) ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(int sectionId, int subjectId, int quarter)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var records = await _context.Grades
                .Where(g => g.SectionId == sectionId && g.SubjectId == subjectId && g.Quarter == quarter)
                .ToListAsync();

            foreach (var r in records)
                r.IsFinalized = true;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Grades finalized and locked.";
            return RedirectToAction("Index", new { sectionId, quarter });
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

            var sectionSubjects = await _context.SectionSubjects
                .Where(ss => ss.SectionId == student.SectionId)
                .Include(ss => ss.Subject)
                .ToListAsync();

            var grades = await _context.Grades
                .Where(g => g.StudentId == studentId)
                .ToListAsync();

            var subjects = sectionSubjects.Select(ss => new StudentSubjectGradeVM
            {
                SubjectName = ss.Subject?.Name ?? "—",
                Q1 = grades.FirstOrDefault(g => g.SubjectId == ss.SubjectId && g.Quarter == 1)?.GradeValue,
                Q2 = grades.FirstOrDefault(g => g.SubjectId == ss.SubjectId && g.Quarter == 2)?.GradeValue,
                Q3 = grades.FirstOrDefault(g => g.SubjectId == ss.SubjectId && g.Quarter == 3)?.GradeValue,
            }).OrderBy(s => s.SubjectName).ToList();

            var vm = new StudentGradeReportVM
            {
                StudentName = $"{student.FirstName} {student.LastName}",
                SectionName = student.Section?.SectionName ?? "—",
                GradeLevel  = student.Section?.GradeLevel ?? "—",
                Subjects    = subjects
            };

            return View("StudentReport", vm);
        }
    }
}