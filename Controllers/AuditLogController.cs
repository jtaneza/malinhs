using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;

namespace MalikongkongNHS.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? search, string? module, string? action,
            string? role, string? from, string? to, int page = 1)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return Forbid();

            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                query = query.Where(l =>
                    l.PerformedBy.ToLower().Contains(q) ||
                    l.Description.ToLower().Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(module) && module != "All")
                query = query.Where(l => l.Module == module);

            if (!string.IsNullOrWhiteSpace(action) && action != "All")
                query = query.Where(l => l.Action == action);

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
                query = query.Where(l => l.Role == role);

            if (DateTime.TryParse(from, out var fromDate))
                query = query.Where(l => l.Timestamp.Date >= fromDate.Date);

            if (DateTime.TryParse(to, out var toDate))
                query = query.Where(l => l.Timestamp.Date <= toDate.Date);

            const int PageSize = 20;
            var total = await query.CountAsync();
            var logs  = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Total    = total;
            ViewBag.Page     = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Pages    = (int)Math.Ceiling(total / (double)PageSize);

            // Filter options
            ViewBag.Search = search;
            ViewBag.Module = module;
            ViewBag.Action = action;
            ViewBag.Role   = role;
            ViewBag.From   = from;
            ViewBag.To     = to;

            ViewBag.Modules = new[] { "All", "Student", "Teacher", "Payment", "Section", "User", "Account" };
            ViewBag.Actions = new[] { "All", "Create", "Update", "Delete", "Login", "Logout" };
            ViewBag.Roles   = new[] { "All", "Admin", "Cashier", "Teacher", "Student" };

            return View(logs);
        }

        // ── CLEAR ALL LOGS ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Forbid();

            _context.AuditLogs.RemoveRange(_context.AuditLogs);
            await _context.SaveChangesAsync();
            TempData["Success"] = "All audit logs cleared.";
            return RedirectToAction(nameof(Index));
        }
    }
}
