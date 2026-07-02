using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using System.Text;

namespace MalikongkongNHS.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── INDEX ───────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? search, string? module, string? action,
            string? role, string? from, string? to,
            string? userId, int page = 1)
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

            if (!string.IsNullOrWhiteSpace(userId) && int.TryParse(userId, out var uid) && uid > 0)
                query = query.Where(l => l.UserId == uid);

            if (DateTime.TryParse(from, out var fromDate))
                query = query.Where(l => l.Timestamp.Date >= fromDate.Date);

            if (DateTime.TryParse(to, out var toDate))
                query = query.Where(l => l.Timestamp.Date <= toDate.Date);

            // ── Full-DB stat counts (before paging) ──────────────
            var totalFiltered = await query.CountAsync();
            var creates = await query.CountAsync(l => l.Action == "Create");
            var updates = await query.CountAsync(l => l.Action == "Update");
            var deletes = await query.CountAsync(l => l.Action == "Delete");

            const int PageSize = 20;
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Total    = totalFiltered;
            ViewBag.Creates  = creates;
            ViewBag.Updates  = updates;
            ViewBag.Deletes  = deletes;
            ViewBag.Page     = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Pages    = (int)Math.Ceiling(totalFiltered / (double)PageSize);

            // Filter options
            ViewBag.Search = search;
            ViewBag.Module = module;
            ViewBag.Action = action;
            ViewBag.Role   = role;
            ViewBag.From   = from;
            ViewBag.To     = to;
            ViewBag.UserId = userId;

            ViewBag.Modules = new[] { "All", "Student", "Teacher", "Payment", "Section", "User", "Account" };
            ViewBag.Actions = new[] { "All", "Create", "Update", "Delete", "Login", "Logout" };
            ViewBag.Roles   = new[] { "All", "Admin", "Cashier", "Teacher", "Student" };

            return View(logs);
        }

        // ── GET USER ACTIVITY (AJAX) ────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetUserActivity(string username)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Forbid();

            if (string.IsNullOrWhiteSpace(username))
                return Json(new { logs = new object[] { }, totalActions = 0 });

            var logs = await _context.AuditLogs
                .Where(l => l.PerformedBy == username)
                .OrderByDescending(l => l.Timestamp)
                .Take(100)
                .Select(l => new
                {
                    l.Id,
                    Timestamp   = l.Timestamp.ToString("MMM dd, yyyy hh:mm:ss tt"),
                    l.Action,
                    l.Module,
                    l.Description,
                    l.IpAddress,
                    l.Role
                })
                .ToListAsync();

            var summary = new
            {
                TotalActions = await _context.AuditLogs.CountAsync(l => l.PerformedBy == username),
                Creates      = await _context.AuditLogs.CountAsync(l => l.PerformedBy == username && l.Action == "Create"),
                Updates      = await _context.AuditLogs.CountAsync(l => l.PerformedBy == username && l.Action == "Update"),
                Deletes      = await _context.AuditLogs.CountAsync(l => l.PerformedBy == username && l.Action == "Delete"),
                LastSeen     = await _context.AuditLogs
                                   .Where(l => l.PerformedBy == username)
                                   .MaxAsync(l => (DateTime?)l.Timestamp)
            };

            return Json(new { logs, summary });
        }

        // ── EXPORT LOGS AS CSV (ADMIN ONLY) ─────────────────
        [HttpGet]
        public async Task<IActionResult> ExportPdf(
            string? search, string? module, string? action,
            string? role, string? from, string? to, string? userId)
        {
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

            if (!string.IsNullOrWhiteSpace(userId) && int.TryParse(userId, out var uid) && uid > 0)
                query = query.Where(l => l.UserId == uid);

            if (DateTime.TryParse(from, out var fromDate))
                query = query.Where(l => l.Timestamp.Date >= fromDate.Date);

            if (DateTime.TryParse(to, out var toDate))
                query = query.Where(l => l.Timestamp.Date <= toDate.Date);

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            var bytes = BuildAuditLogPdf(logs);
            return File(bytes, "application/pdf", $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        private static byte[] BuildAuditLogPdf(IReadOnlyList<AuditLog> logs)
        {
            const int LinesPerPage = 48;
            var lines = new List<string>
            {
                "Malikongkong NHS SErpS",
                $"Audit Log Report - Generated {DateTime.Now:MMMM dd, yyyy hh:mm tt}",
                $"Total Logs: {logs.Count}",
                ""
            };

            if (logs.Count == 0)
            {
                lines.Add("No audit logs found for the selected filters.");
            }
            else
            {
                var row = 1;
                foreach (var log in logs)
                {
                    var userId = log.UserId > 0 ? log.UserId.ToString() : "-";
                    var ip = string.IsNullOrWhiteSpace(log.IpAddress) ? "-" : log.IpAddress;

                    lines.Add($"{row}. {log.Timestamp:yyyy-MM-dd HH:mm:ss} | {log.PerformedBy} | ID: {userId}");
                    lines.Add($"   {log.Role} | {log.Action} | {log.Module} | IP: {ip}");
                    foreach (var part in WrapText($"   {log.Description}", 104))
                        lines.Add(part);
                    lines.Add("");
                    row++;
                }
            }

            var pages = lines
                .Select((line, index) => new { line, index })
                .GroupBy(x => x.index / LinesPerPage)
                .Select(g => g.Select(x => x.line).ToList())
                .ToList();

            var objects = new List<string> { "", "", "" };
            var pageIds = new List<int>();

            foreach (var page in pages)
            {
                var content = new StringBuilder();
                content.AppendLine("BT");
                content.AppendLine("/F1 9 Tf");
                content.AppendLine("13 TL");
                content.AppendLine("40 790 Td");

                foreach (var line in page)
                    content.AppendLine($"({EscapePdfText(line)}) Tj T*");

                content.AppendLine("ET");

                var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
                var contentId = objects.Count + 1;
                objects.Add($"<< /Length {contentBytes.Length} >>\nstream\n{content}endstream");

                var pageId = objects.Count + 1;
                pageIds.Add(pageId);
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentId} 0 R >>");
            }

            objects[0] = "<< /Type /Catalog /Pages 2 0 R >>";
            objects[1] = $"<< /Type /Pages /Kids [{string.Join(" ", pageIds.Select(id => $"{id} 0 R"))}] /Count {pageIds.Count} >>";
            objects[2] = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>";

            using var stream = new MemoryStream();
            var offsets = new List<long> { 0 };

            WriteAscii(stream, "%PDF-1.4\n");
            for (var i = 0; i < objects.Count; i++)
            {
                offsets.Add(stream.Position);
                WriteAscii(stream, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }

            var xrefPosition = stream.Position;
            WriteAscii(stream, $"xref\n0 {objects.Count + 1}\n");
            WriteAscii(stream, "0000000000 65535 f \n");
            for (var i = 1; i < offsets.Count; i++)
                WriteAscii(stream, $"{offsets[i]:0000000000} 00000 n \n");

            WriteAscii(stream, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
            return stream.ToArray();
        }

        private static IEnumerable<string> WrapText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            for (var start = 0; start < text.Length; start += maxLength)
                yield return text.Substring(start, Math.Min(maxLength, text.Length - start));
        }

        private static string EscapePdfText(string text)
        {
            var ascii = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(text));
            return ascii.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static void WriteAscii(Stream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
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
