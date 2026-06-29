using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Controllers
{
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var teacherId = HttpContext.Session.GetInt32("UserId");

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                return View(new List<TeacherClassVM>());

            // ── 1. Sections where teacher is the adviser ──────────
            var advisorySections = await _context.Sections
                .Where(s => s.IsActive && s.Adviser == teacher.FullName)
                .Select(s => s.SectionId)
                .ToListAsync();

            // ── 2. Sections where teacher has at least one subject assigned ──
            var subjectSectionIds = await _context.SectionSubjects
                .Where(ss => ss.TeacherId == teacher.TeacherId)
                .Select(ss => ss.SectionId)
                .Distinct()
                .ToListAsync();

            // Union of both sets
            var allSectionIds = advisorySections
                .Union(subjectSectionIds)
                .Distinct()
                .ToList();

            if (!allSectionIds.Any())
                return View(new List<TeacherClassVM>());

            // ── 3. Load the sections with their active students ────
            var sections = await _context.Sections
                .Where(s => s.IsActive && allSectionIds.Contains(s.SectionId))
                .Include(s => s.Students.Where(st => st.IsActive))
                .ToListAsync();

            var vm = sections.Select(s => new TeacherClassVM
            {
                SectionId   = s.SectionId,
                SectionName = s.SectionName,
                GradeLevel  = s.GradeLevel ?? "—",
                Adviser     = s.Adviser ?? "—",
                Capacity    = s.Capacity ?? 0,
                IsAdviser   = advisorySections.Contains(s.SectionId),
                Students    = s.Students.Select(st => new StudentListItemVM
                {
                    StudentId     = st.StudentId,
                    StudentNo     = st.LRN ?? "—",
                    FullName      = $"{st.LastName}, {st.FirstName} {st.MiddleName}".Trim(),
                    Gender        = st.Gender ?? "—",
                    ContactNumber = st.ContactNumber ?? "—",
                    GuardianName  = st.GuardianName ?? "—"
                }).OrderBy(st => st.FullName).ToList()
            })
            .OrderByDescending(s => s.IsAdviser)   // advisory class first
            .ThenBy(s => s.SectionName)
            .ToList();

            return View(vm);
        }

        // View students of a specific section
        public async Task<IActionResult> Students(int sectionId)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var teacherId = HttpContext.Session.GetInt32("UserId");

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                return RedirectToAction("Index");

            // Teacher must be adviser OR have a subject in this section
            bool isAdviser = await _context.Sections
                .AnyAsync(s => s.SectionId == sectionId && s.Adviser == teacher.FullName);

            bool isSubjectTeacher = await _context.SectionSubjects
                .AnyAsync(ss => ss.SectionId == sectionId && ss.TeacherId == teacher.TeacherId);

            if (!isAdviser && !isSubjectTeacher)
                return RedirectToAction("Index");

            var section = await _context.Sections
                .Include(s => s.Students.Where(st => st.IsActive))
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
                return RedirectToAction("Index");

            var vm = new TeacherClassVM
            {
                SectionId   = section.SectionId,
                SectionName = section.SectionName,
                GradeLevel  = section.GradeLevel ?? "—",
                Adviser     = section.Adviser ?? "—",
                Capacity    = section.Capacity ?? 0,
                IsAdviser   = isAdviser,
                Students    = section.Students.Select(st => new StudentListItemVM
                {
                    StudentId     = st.StudentId,
                    StudentNo     = st.LRN ?? "—",
                    FullName      = $"{st.LastName}, {st.FirstName} {st.MiddleName}".Trim(),
                    Gender        = st.Gender ?? "—",
                    ContactNumber = st.ContactNumber ?? "—",
                    GuardianName  = st.GuardianName ?? "—"
                }).OrderBy(st => st.FullName).ToList()
            };

            return View(vm);
        }
    }
}