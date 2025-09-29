using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;

namespace PreschoolManagement.Controllers
{
    [Authorize(Roles = "Parent,Admin,Teacher")] // tuỳ quyền bạn muốn
    public class MyChildrenController : Controller
    {
        private readonly ApplicationDbContext _db;
        public MyChildrenController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            // Phụ huynh: chỉ con của mình; Admin/Teacher: xem tất cả (hoặc tuỳ chỉnh)
            var query = _db.Students
                .AsNoTracking()
                .Include(s => s.ClassRoom)
                .AsQueryable();

            if (!User.IsInRole("Admin") && !User.IsInRole("Teacher"))
                query = query.Where(s => s.ParentId == userId);

            var data = await query.OrderBy(s => s.FullName).ToListAsync();
            ViewData["Title"] = "Con em";
            return View(data);
        }
    }
}
