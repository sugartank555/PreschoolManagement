using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;

namespace PreschoolManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var latest = await _db.Announcements
                .AsNoTracking()
                .OrderByDescending(a => a.PublishedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.LatestAnnouncements = latest;
            ViewData["Title"] = "Trang chính";
            return View();
        }
    }
}
