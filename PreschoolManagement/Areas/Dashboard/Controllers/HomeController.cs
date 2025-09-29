using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Areas.Dashboard.ViewModels;


namespace PreschoolManagement.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            // 1) Đồng bộ trạng thái Overdue trước, rồi lưu
            var needOverdue = await _db.FeeInvoices
                .Where(f => f.Status != "Paid" && f.Month < monthStart)
                .ToListAsync();
            foreach (var inv in needOverdue) inv.Status = "Overdue";
            if (needOverdue.Count > 0) await _db.SaveChangesAsync();

            // 2) KPI (tuần tự + AsNoTracking)
            var totalStudents = await _db.Students.AsNoTracking().CountAsync();
            var totalClasses = await _db.ClassRooms.AsNoTracking().CountAsync();
            var pendingInvoices = await _db.FeeInvoices.AsNoTracking().CountAsync(f => f.Status == "Pending");
            var overdueInvoices = await _db.FeeInvoices.AsNoTracking().CountAsync(f => f.Status == "Overdue");

            // 3) Top lớp
            var topClasses = await _db.ClassRooms
                .AsNoTracking()
                .Include(c => c.Students)
                .OrderByDescending(c => c.Students.Count)
                .Take(5)
                .Select(c => new TopClassVM
                {
                    ClassId = c.Id,
                    ClassName = c.Name,
                    Grade = c.Grade,
                    Room = c.Room,
                    StudentCount = c.Students.Count
                })
                .ToListAsync();

            // 4) Hóa đơn quá hạn mới nhất
            var latestOverdue = await _db.FeeInvoices
                .AsNoTracking()
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .Where(f => f.Status == "Overdue")
                .OrderBy(f => f.Month)
                .Take(10)
                .ToListAsync();

            // 5) Thông báo mới
            var recentAnnouncements = await _db.Announcements
                .AsNoTracking()
                .OrderByDescending(a => a.PublishedAt)
                .Take(6)
                .ToListAsync();

            // 6) Dữ liệu biểu đồ 6 tháng
            var sixMonthsWindowStart = monthStart.AddMonths(-5);
            var revenue = await _db.FeeInvoices
                .AsNoTracking()
                .Where(f => f.Month >= sixMonthsWindowStart)
                .GroupBy(f => new { f.Month.Year, f.Month.Month })
                .Select(g => new RevenuePoint
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Paid = g.Sum(x => x.Paid),
                    Amount = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var vm = new DashboardHomeVM
            {
                TotalStudents = totalStudents,
                TotalClasses = totalClasses,
                PendingInvoices = pendingInvoices,
                OverdueInvoices = overdueInvoices,

                TopClasses = topClasses,
                LatestOverdue = latestOverdue,
                RecentAnnouncements = recentAnnouncements,

                Chart = new RevenueChartVM
                {
                    Labels = Enumerable.Range(0, 6)
                        .Select(i => monthStart.AddMonths(-5 + i))
                        .Select(d => $"{d:MM/yyyy}")
                        .ToList(),
                    PaidSeries = BuildSeries(monthStart, revenue, v => v.Paid),
                    AmountSeries = BuildSeries(monthStart, revenue, v => v.Amount)
                }
            };

            return View(vm);
        }

        private static List<decimal> BuildSeries(DateTime monthStart, IEnumerable<RevenuePoint> points, Func<RevenuePoint, decimal> pick)
        {
            var map = points.ToDictionary(k => (k.Year, k.Month), v => pick(v));
            var result = new List<decimal>(6);
            for (int i = -5; i <= 0; i++)
            {
                var d = monthStart.AddMonths(i);
                result.Add(map.TryGetValue((d.Year, d.Month), out var val) ? val : 0m);
            }
            return result;
        }
    }

    // Giữ nguyên các ViewModel: DashboardHomeVM, TopClassVM, RevenueChartVM, RevenuePoint
}
