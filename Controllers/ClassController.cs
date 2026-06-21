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

            // Get sections where this teacher is the adviser
            var sections = await _context.Sections
                .Where(s => s.IsActive && s.Adviser == teacher.FullName)
                .Include(s => s.Students.Where(st => st.IsActive))
                .ToListAsync();

            var vm = sections.Select(s => new TeacherClassVM
            {
                SectionId   = s.SectionId,
                SectionName = s.SectionName,
                GradeLevel  = s.GradeLevel ?? "—",
                Capacity    = s.Capacity ?? 0,
                Students    = s.Students.Select(st => new StudentListItemVM
                {
                    StudentId   = st.StudentId,
                    StudentNo   = st.LRN ?? "—",
                    FullName    = $"{st.LastName}, {st.FirstName} {st.MiddleName}".Trim(),
                    Gender      = st.Gender ?? "—",
                    ContactNumber = st.ContactNumber ?? "—",
                    GuardianName  = st.GuardianName ?? "—"
                }).OrderBy(st => st.FullName).ToList()
            }).ToList();

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

            var section = await _context.Sections
                .Include(s => s.Students.Where(st => st.IsActive))
                .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.Adviser == teacher.FullName);

            if (section == null)
                return RedirectToAction("Index");

            var vm = new TeacherClassVM
            {
                SectionId   = section.SectionId,
                SectionName = section.SectionName,
                GradeLevel  = section.GradeLevel ?? "—",
                Capacity    = section.Capacity ?? 0,
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