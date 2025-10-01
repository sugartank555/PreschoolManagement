using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.ViewModels.Home;

namespace PreschoolManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy Top 5 thông báo mới nhất
            var latest = await _db.Announcements
                .OrderByDescending(a => a.PublishedAt)
                .Select(a => new HomeIndexVM.AnnouncementItem
                {
                    Id = a.Id,
                    Title = a.Title,
                    Excerpt = (a.Content.Length > 140 ? a.Content.Substring(0, 140) + "..." : a.Content),
                    PublishedAt = a.PublishedAt
                })
                .Take(5)
                .ToListAsync();

            // "Sự kiện sắp tới": tạm lấy từ Announcements có từ khóa hoặc ngày công bố trong tương lai
            var now = DateTime.UtcNow;
            var events = await _db.Announcements
                .Where(a =>
                    a.Title.Contains("Sự kiện") ||
                    a.Title.Contains("Event") ||
                    a.Content.Contains("Sự kiện") ||
                    a.PublishedAt > now)
                .OrderBy(a => a.PublishedAt)
                .Select(a => new HomeIndexVM.AnnouncementItem
                {
                    Id = a.Id,
                    Title = a.Title,
                    Excerpt = (a.Content.Length > 120 ? a.Content.Substring(0, 120) + "..." : a.Content),
                    PublishedAt = a.PublishedAt
                })
                .Take(5)
                .ToListAsync();

            var vm = new HomeIndexVM
            {
                LatestAnnouncements = latest,
                UpcomingEvents = events,
                Testimonials = new List<HomeIndexVM.TestimonialItem>
                {
                    new() { ParentName = "Chị Lan (phụ huynh bé Bống)", Content = "Con về nhà rất vui mỗi ngày, cô giáo gần gũi và chương trình phong phú.", ChildName="Bống" },
                    new() { ParentName = "Anh Minh (phụ huynh bé Tí)", Content = "Cơ sở vật chất sạch đẹp, bữa ăn đa dạng, bé tăng cân đều.", ChildName="Tí" },
                    new() { ParentName = "Chị Hạnh (phụ huynh bé Na)", Content = "Thầy cô cập nhật tình hình hằng ngày, rất yên tâm khi gửi con.", ChildName="Na" }
                }
            };

            ViewData["Title"] = "Mầm non tư thục – Trang chủ";
            return View(vm);
        }

        public IActionResult Privacy() => View();
    }
}
