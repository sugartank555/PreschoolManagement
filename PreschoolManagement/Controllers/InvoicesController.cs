using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Models;

namespace PreschoolManagement.Controllers
{
    [Authorize(Roles = "Parent")]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoicesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /ParentInvoices
        public async Task<IActionResult> Index(int? studentId, string? status)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            // Query hóa đơn chỉ của con em phụ huynh hiện tại
            var q = _db.FeeInvoices
                .AsNoTracking()
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .Where(f => f.Student.ParentId == me.Id)
                .AsQueryable();

            if (studentId.HasValue)
                q = q.Where(f => f.StudentId == studentId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(f => f.Status == status);

            var data = await q.OrderByDescending(f => f.Month).ToListAsync();

            // Dropdown danh sách con em của phụ huynh
            var myKids = await _db.Students
                .AsNoTracking()
                .Where(s => s.ParentId == me.Id)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewBag.Students = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(myKids, "Id", "FullName", studentId);
            ViewBag.Status = status;
            ViewData["Title"] = "Hóa đơn của con em";

            return View(data);
        }

        // GET: /ParentInvoices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            var item = await _db.FeeInvoices
                .AsNoTracking()
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (item == null) return NotFound();

            // Chặn xem hóa đơn không thuộc con mình
            if (item.Student.ParentId != me.Id) return Forbid();

            ViewData["Title"] = "Chi tiết hóa đơn";
            return View(item);
        }
    }
}
