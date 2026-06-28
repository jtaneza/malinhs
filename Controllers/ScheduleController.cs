using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var teacherId = HttpContext.Session.GetInt32("UserId");

            var assignments = await _context.SectionSubjects
                .Include(ss => ss.Subject)
                .Include(ss => ss.Section)
                .Where(ss => ss.TeacherId == teacherId)
                .OrderBy(ss => ss.Time)
                .ToListAsync();

            return View(assignments);
        }
    }
}