using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Models;

namespace PreschoolManagement.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin,Staff")]
    public class ContactMessagesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ContactMessagesController(ApplicationDbContext db) => _db = db;

        // List + filter + paginate
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Liên hệ từ phụ huynh";

            var query = _db.ContactMessages
                .Include(c => c.CreatedBy)
                .Include(c => c.RelatedStudent)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim();
                query = query.Where(c =>
                    (c.Name != null && c.Name.Contains(k)) ||
                    (c.Email != null && c.Email.Contains(k)) ||
                    (c.Subject != null && c.Subject.Contains(k)) ||
                    (c.Message != null && c.Message.Contains(k)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.ContactMessages
                .Include(c => c.CreatedBy)
                .Include(c => c.RelatedStudent)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (item == null) return NotFound();

            ViewData["Title"] = "Chi tiết liên hệ";
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.ContactMessages.FindAsync(id);
            if (item == null) return NotFound();

            _db.ContactMessages.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xoá liên hệ.";
            return RedirectToAction(nameof(Index));
        }

        // (Tuỳ chọn) Xuất CSV nhanh
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? q)
        {
            var query = _db.ContactMessages.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim();
                query = query.Where(c =>
                    (c.Name != null && c.Name.Contains(k)) ||
                    (c.Email != null && c.Email.Contains(k)) ||
                    (c.Subject != null && c.Subject.Contains(k)) ||
                    (c.Message != null && c.Message.Contains(k)));
            }

            var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Id,Name,Email,Subject,Message,CreatedAt");
            foreach (var c in list)
            {
                string esc(string s) => "\"" + (s?.Replace("\"", "\"\"") ?? "") + "\"";
                sb.AppendLine(string.Join(",",
                    c.Id,
                    esc(c.Name),
                    esc(c.Email),
                    esc(c.Subject ?? ""),
                    esc(c.Message),
                    c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"contact_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }
    }
}
