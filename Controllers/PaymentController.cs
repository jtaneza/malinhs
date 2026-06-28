using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── INDEX ──────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            // all statuses that count as "paid/completed"
            var paidStatuses = new[] { "Paid", "Full Payment", "Partial Payment", "Late Payment" };

            var vm = new CashierDashboardVM
            {
                TotalRevenue      = await _context.Payments
                                        .Where(p => paidStatuses.Contains(p.Status))
                                        .SumAsync(p => (decimal?)p.Amount) ?? 0,
                PaidStudents      = await _context.Payments
                                        .Where(p => paidStatuses.Contains(p.Status))
                                        .Select(p => p.StudentId)
                                        .Distinct()
                                        .CountAsync(),
                PendingPayments   = await _context.Payments
                                        .CountAsync(p => p.Status == "Pending"),
                RevenueToday      = await _context.Payments
                                        .Where(p => paidStatuses.Contains(p.Status) && p.DatePaid.Date == today)
                                        .SumAsync(p => (decimal?)p.Amount) ?? 0,
                TotalTransactions = await _context.Payments.CountAsync()
            };

            return View(vm);
        }

        // ── GET TRANSACTIONS (AJAX) ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetTransactions(string? search, string? status)
        {
            var query = _context.Payments.Include(p => p.Student).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    (p.Student!.FirstName + " " + p.Student.LastName).ToLower().Contains(search) ||
                    p.Student.StudentNo!.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(status) && status != "All Status")
                query = query.Where(p => p.Status == status);

            var data = await query
                .OrderByDescending(p => p.DatePaid)
                .Select(p => new
                {
                    p.Id,
                    StudentName  = p.Student!.FirstName + " " + p.Student.LastName,
                    GradeSection = p.Student.Section != null ? p.Student.Section.SectionName : "—",
                    p.FeeType,
                    p.Amount,
                    DatePaid     = p.DatePaid.ToString("MMM dd, yyyy"),
                    RawDate      = p.DatePaid.ToString("yyyy-MM-dd"),  // ← fix for edit
                    p.Status,
                    p.ReceiptNo,
                    p.Notes
                })
                .ToListAsync();

            return Json(data);
        }

        // ── GET PAYMENT HISTORY (AJAX) ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetHistory(string? search, string? from, string? to)
        {
            // show all payment types (not just "Paid")
            var query = _context.Payments
                .Include(p => p.Student)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    (p.Student!.FirstName + " " + p.Student.LastName).ToLower().Contains(search));
            }

            if (DateTime.TryParse(from, out var fromDate))
                query = query.Where(p => p.DatePaid.Date >= fromDate.Date);

            if (DateTime.TryParse(to, out var toDate))
                query = query.Where(p => p.DatePaid.Date <= toDate.Date);

            var data = await query
                .OrderByDescending(p => p.DatePaid)
                .Select(p => new
                {
                    p.ReceiptNo,
                    StudentName = p.Student!.FirstName + " " + p.Student.LastName,
                    p.FeeType,
                    p.Amount,
                    DatePaid    = p.DatePaid.ToString("MMM dd, yyyy"),
                    p.Status,
                    p.Cashier
                })
                .ToListAsync();

            return Json(data);
        }

        // ── GET RECEIPT (AJAX) ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetReceipt(string search)
        {
            var payment = await _context.Payments
                .Include(p => p.Student)
                    .ThenInclude(s => s!.Section)
                .Where(p =>
                    ((p.Student!.FirstName + " " + p.Student.LastName).ToLower().Contains(search.ToLower()) ||
                     p.ReceiptNo == search))
                .OrderByDescending(p => p.DatePaid)
                .FirstOrDefaultAsync();

            if (payment == null)
                return Json(null);

            return Json(new
            {
                payment.ReceiptNo,
                StudentName  = payment.Student!.FirstName + " " + payment.Student.LastName,
                GradeSection = payment.Student.Section?.SectionName ?? "—",
                payment.FeeType,
                payment.Amount,
                DatePaid     = payment.DatePaid.ToString("MMMM dd, yyyy"),
                payment.Status,
                payment.Cashier
            });
        }

        // ── CREATE PAYMENT (POST) ──────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create(int studentId, string feeType, decimal amount,
                                                 DateTime date, string? status, string? notes)
        {
            var cashier   = HttpContext.Session.GetString("Username") ?? "Unknown";
            var count     = await _context.Payments.CountAsync() + 1;
            var receiptNo = $"REC-{count:D6}";

            var payment = new Payment
            {
                StudentId = studentId,
                FeeType   = feeType,
                Amount    = amount,
                DatePaid  = date,
                Notes     = notes,
                Status    = status ?? "Full Payment",
                Cashier   = cashier,
                ReceiptNo = receiptNo
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return Json(new { success = true, receiptNo });
        }

        // ── EDIT ───────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string feeType, decimal amount,
                                               string date, string status, string? notes)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return Json(new { success = false });

            payment.FeeType  = feeType;
            payment.Amount   = amount;
            payment.DatePaid = DateTime.Parse(date);
            payment.Status   = status;
            payment.Notes    = notes;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ── DELETE ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return Json(new { success = false });

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ── DOWNLOAD HISTORY AS PDF ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> DownloadHistory(string? from, string? to)
        {
            var query = _context.Payments
                .Include(p => p.Student)
                .AsQueryable();

            if (DateTime.TryParse(from, out var fromDate))
                query = query.Where(p => p.DatePaid.Date >= fromDate.Date);
            if (DateTime.TryParse(to, out var toDate))
                query = query.Where(p => p.DatePaid.Date <= toDate.Date);

            var data = await query.OrderByDescending(p => p.DatePaid)
                .Select(p => new {
                    p.ReceiptNo,
                    StudentName = p.Student!.FirstName + " " + p.Student.LastName,
                    p.FeeType,
                    p.Amount,
                    DatePaid    = p.DatePaid.ToString("MMM dd, yyyy"),
                    p.Status,
                    p.Cashier
                }).ToListAsync();

            var fromLabel = string.IsNullOrEmpty(from) ? "All time" : from;
            var toLabel   = string.IsNullOrEmpty(to)   ? "present"  : to;

            var rows = string.Join("", data.Select((p, i) => $@"
                <tr>
                    <td>{i + 1}</td>
                    <td>{p.ReceiptNo}</td>
                    <td>{p.StudentName}</td>
                    <td>{p.FeeType}</td>
                    <td>₱{p.Amount:N2}</td>
                    <td>{p.DatePaid}</td>
                    <td>{p.Status}</td>
                    <td>{p.Cashier}</td>
                </tr>"));

            var total = data.Sum(p => p.Amount);

            var html = $@"
            <html>
            <head>
            <style>
                body {{ font-family: Arial, sans-serif; font-size: 12px; padding: 20px; }}
                h2 {{ text-align: center; color: #c0392b; margin-bottom: 4px; }}
                p.sub {{ text-align: center; color: #666; margin-top: 0; }}
                table {{ width: 100%; border-collapse: collapse; margin-top: 16px; }}
                th {{ background: #c0392b; color: #fff; padding: 8px; text-align: left; font-size: 11px; }}
                td {{ padding: 7px 8px; border-bottom: 1px solid #eee; }}
                tr:nth-child(even) {{ background: #f9f9f9; }}
                .total-row {{ font-weight: bold; background: #f1f1f1; }}
                .footer {{ margin-top: 20px; text-align: right; font-size: 11px; color: #888; }}
            </style>
            </head>
            <body>
                <h2>Malikongkong NHS</h2>
                <p class='sub'>Payment History Report &mdash; {fromLabel} to {toLabel}</p>
                <table>
                    <thead>
                        <tr>
                            <th>#</th><th>Receipt #</th><th>Student Name</th>
                            <th>Fee Type</th><th>Amount</th><th>Date</th>
                            <th>Status</th><th>Cashier</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                        <tr class='total-row'>
                            <td colspan='4' style='text-align:right;'>Total Revenue:</td>
                            <td>₱{total:N2}</td>
                            <td colspan='3'></td>
                        </tr>
                    </tbody>
                </table>
                <div class='footer'>Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}</div>
            </body>
            </html>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            return File(bytes, "text/html", $"PaymentHistory_{DateTime.Now:yyyyMMdd}.html");
        }

        // ── SEARCH STUDENTS (AJAX) ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SearchStudents(string term)
        {
            var students = await _context.Students
                .Where(s => s.IsActive &&
                    ((s.FirstName + " " + s.LastName).ToLower().Contains(term.ToLower()) ||
                     s.StudentNo!.Contains(term)))
                .Select(s => new
                {
                    s.StudentId,
                    FullName  = s.FirstName + " " + s.LastName,
                    s.StudentNo
                })
                .Take(10)
                .ToListAsync();

            return Json(students);
        }
    }
}