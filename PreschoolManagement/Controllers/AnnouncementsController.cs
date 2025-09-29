using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;

namespace PreschoolManagement.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AnnouncementsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, string? audience)
        {
            var qr = _db.Announcements.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                qr = qr.Where(a => a.Title.Contains(q) || a.Content.Contains(q));
            if (!string.IsNullOrWhiteSpace(audience))
                qr = qr.Where(a => a.Audience == audience || a.Audience == "All");

            var data = await qr.OrderByDescending(a => a.PublishedAt).ToListAsync();
            ViewBag.Q = q; ViewBag.Audience = audience;
            ViewData["Title"] = "Thông báo";
            return View(data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.Announcements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            return item == null ? NotFound() : View(item);
        }
    }
}
