using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;

namespace PreschoolManagement.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin,Staff")]
    public class VisitRegistrationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public VisitRegistrationsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
        {
            var q = _db.VisitRegistrations
                .Include(v => v.ClassRoom)
                .Include(v => v.Student)
                .AsNoTracking();

            if (from.HasValue) q = q.Where(x => x.VisitDate >= from.Value.Date);
            if (to.HasValue) q = q.Where(x => x.VisitDate <= to.Value.Date);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = pageSize;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.VisitRegistrations
                .Include(v => v.ClassRoom)
                .Include(v => v.Student)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _db.VisitRegistrations.FindAsync(id);
            if (e == null) return NotFound();
            _db.Remove(e); await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xoá đăng ký tham quan.";
            return RedirectToAction(nameof(Index));
        }
    }

}
