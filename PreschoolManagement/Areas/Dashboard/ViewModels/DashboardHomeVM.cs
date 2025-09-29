using PreschoolManagement.Models;

namespace PreschoolManagement.Areas.Dashboard.ViewModels
{
    public class DashboardHomeVM
    {
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int PendingInvoices { get; set; }
        public int OverdueInvoices { get; set; }

        public List<TopClassVM> TopClasses { get; set; } = new();
        public List<FeeInvoice> LatestOverdue { get; set; } = new();
        public List<Announcement> RecentAnnouncements { get; set; } = new();

        public RevenueChartVM Chart { get; set; } = new();
    }

    public class TopClassVM
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = default!;
        public string? Grade { get; set; }
        public string? Room { get; set; }
        public int StudentCount { get; set; }
    }

    public class RevenueChartVM
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> PaidSeries { get; set; } = new();
        public List<decimal> AmountSeries { get; set; } = new();
    }

    public class RevenuePoint
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Paid { get; set; }
        public decimal Amount { get; set; }
    }
}
