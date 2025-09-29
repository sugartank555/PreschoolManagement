using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;

namespace PreschoolManagement.Controllers
{
    [Authorize(Roles = "Parent,Admin")] // phụ huynh & admin
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public InvoicesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? studentId, string? status)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            var invoices = _db.FeeInvoices
                .AsNoTracking()
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
                invoices = invoices.Where(f => f.Student.ParentId == userId);

            if (studentId.HasValue)
                invoices = invoices.Where(f => f.StudentId == studentId.Value);
            if (!string.IsNullOrWhiteSpace(status))
                invoices = invoices.Where(f => f.Status == status);

            var data = await invoices.OrderByDescending(f => f.Month).ToListAsync();

            // dropdown học sinh (chỉ của user khi là parent)
            var students = _db.Students.AsNoTracking().AsQueryable();
            if (!User.IsInRole("Admin"))
                students = students.Where(s => s.ParentId == userId);

            ViewBag.Students = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await students.OrderBy(s => s.FullName).ToListAsync(), "Id", "FullName", studentId);

            ViewBag.Status = status;
            ViewData["Title"] = "Hóa đơn";
            return View(data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var item = await _db.FeeInvoices
                .AsNoTracking()
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (item == null) return NotFound();
            if (!User.IsInRole("Admin") && item.Student.ParentId != userId) return Forbid();

            ViewData["Title"] = "Chi tiết hóa đơn";
            return View(item);
        }
    }
}
